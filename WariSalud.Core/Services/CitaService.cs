using WariSalud.Core.Entities;
using WariSalud.Core.Exceptions;
using WariSalud.Core.Interfaces;

namespace WariSalud.Core.Services;

/// <summary>
/// Implementación del servicio de citas médicas.
/// Aplica las reglas de negocio RF02-RF06 definidas en spec.md.
/// </summary>
public class CitaService : ICitaService
{
    private readonly ICitaRepository _citaRepository;
    private readonly IMedicoRepository _medicoRepository;
    private readonly IConfiguracionClinicaRepository _configuracionRepository;

    public CitaService(
        ICitaRepository citaRepository,
        IMedicoRepository medicoRepository,
        IConfiguracionClinicaRepository configuracionRepository)
    {
        _citaRepository = citaRepository;
        _medicoRepository = medicoRepository;
        _configuracionRepository = configuracionRepository;
    }

    /// <summary>
    /// Agenda una cita aplicando todas las reglas de negocio.
    /// T2.1
    /// </summary>
    public async Task<Cita> AgendarCitaAsync(AgendarCitaRequest request)
    {
        // 1. Verificar que el médico existe y está activo (RF-edge case 6)
        var medico = await _medicoRepository.ObtenerPorIdAsync(request.MedicoId);
        if (medico is null || !medico.Activo)
            throw new RecursoNoEncontradoException("Médico", request.MedicoId);

        // 2. Validar horario laboral (RF04)
        var configuracion = await _configuracionRepository.ObtenerConfiguracionAsync();
        var (horaInicio, horaFin, diasLaborables) = medico.ObtenerHorarioEfectivo(configuracion);

        ValidarHorarioLaboral(request.FechaHora, horaInicio, horaFin, diasLaborables);

        // 3. Calcular duración de la cita (snapshot de la especialidad — spec §9.2)
        var duracionMinutos = medico.Especialidad?.DuracionCitaMinutos ?? 30;
        var fechaHoraFin = request.FechaHora.AddMinutes(duracionMinutos);

        // 4. Validar que la cita entera cabe en el horario laboral
        var horaFinCita = TimeOnly.FromDateTime(fechaHoraFin);
        if (horaFinCita > horaFin)
            throw new FueraDeHorarioException(
                $"La cita terminaría a las {horaFinCita:HH:mm}, fuera del horario de cierre ({horaFin:HH:mm}).");

        // 5. Validar double-booking con el médico (RF03)
        var citasDelMedico = await _citaRepository.ObtenerPorMedicoYFechaAsync(
            request.MedicoId, DateOnly.FromDateTime(request.FechaHora));

        foreach (var citaExistente in citasDelMedico.Where(c => c.EstaActiva))
        {
            if (citaExistente.SeSolapa(request.FechaHora, fechaHoraFin))
                throw new DoubleBookingException();
        }

        // 6. Validar límite de citas del paciente ese día (RF05: máx 2 activas)
        var citasDelPaciente = await _citaRepository.ObtenerPorPacienteYFechaAsync(
            request.PacienteId, DateOnly.FromDateTime(request.FechaHora));

        var citasActivasDelPaciente = citasDelPaciente.Count(c => c.EstaActiva);
        if (citasActivasDelPaciente >= 2)
            throw new LimiteDeCitasException();

        // 7. Crear y persistir la cita
        var nuevaCita = new Cita
        {
            PacienteId = request.PacienteId,
            MedicoId = request.MedicoId,
            FechaHora = request.FechaHora,
            DuracionMinutos = duracionMinutos,
            Estado = EstadoCita.Pendiente,
            Motivo = request.Motivo
        };

        return await _citaRepository.AgregarAsync(nuevaCita);
    }

    /// <summary>
    /// Cancela una cita validando propiedad y antelación mínima de 24h.
    /// T2.2
    /// </summary>
    public async Task CancelarCitaAsync(int citaId, int pacienteIdSolicitante)
    {
        var cita = await _citaRepository.ObtenerPorIdAsync(citaId);
        if (cita is null)
            throw new RecursoNoEncontradoException("Cita", citaId);

        // Validar que el solicitante es el dueño de la cita (spec §7 caso 7)
        if (cita.PacienteId != pacienteIdSolicitante)
            throw new AccesoNoAutorizadoException();

        // Validar antelación mínima de 24h (RF06 — umbral inclusivo >= 24h, spec §9.3)
        var ahora = DateTime.UtcNow;
        var tiempoRestante = cita.FechaHora - ahora;
        if (tiempoRestante < TimeSpan.FromHours(24))
            throw new CancelacionFueraDePlazoException();

        cita.Estado = EstadoCita.Cancelada;
        await _citaRepository.ActualizarAsync(cita);
    }

    /// <summary>
    /// Devuelve los bloques de tiempo libres del médico para una fecha específica.
    /// T2.3
    /// </summary>
    public async Task<IEnumerable<BloqueDisponible>> ObtenerDisponibilidadAsync(int medicoId, DateOnly fecha)
    {
        var medico = await _medicoRepository.ObtenerPorIdAsync(medicoId);
        if (medico is null || !medico.Activo)
            throw new RecursoNoEncontradoException("Médico", medicoId);

        var configuracion = await _configuracionRepository.ObtenerConfiguracionAsync();
        var (horaInicio, horaFin, diasLaborables) = medico.ObtenerHorarioEfectivo(configuracion);

        // Verificar si el día solicitado es laborable
        var diaSemana = fecha.DayOfWeek;
        var diasPermitidos = ParseDiasLaborables(diasLaborables);
        if (!diasPermitidos.Contains(diaSemana))
            return Enumerable.Empty<BloqueDisponible>();

        var duracionMinutos = medico.Especialidad?.DuracionCitaMinutos ?? 30;

        // Obtener citas activas del médico ese día
        var citasExistentes = (await _citaRepository.ObtenerPorMedicoYFechaAsync(medicoId, fecha))
            .Where(c => c.EstaActiva)
            .OrderBy(c => c.FechaHora)
            .ToList();

        // Generar todos los bloques posibles dentro del horario
        var bloques = new List<BloqueDisponible>();
        var inicioBusqueda = fecha.ToDateTime(horaInicio);
        var finHorario = fecha.ToDateTime(horaFin);

        while (inicioBusqueda.AddMinutes(duracionMinutos) <= finHorario)
        {
            var finBloque = inicioBusqueda.AddMinutes(duracionMinutos);

            // El bloque está libre si no se solapa con ninguna cita existente
            var haySolapamiento = citasExistentes.Any(c => c.SeSolapa(inicioBusqueda, finBloque));
            if (!haySolapamiento)
                bloques.Add(new BloqueDisponible(inicioBusqueda, finBloque));

            inicioBusqueda = inicioBusqueda.AddMinutes(duracionMinutos);
        }

        return bloques;
    }

    // ─── Helpers privados ────────────────────────────────────────────────────

    private static void ValidarHorarioLaboral(
        DateTime fechaHora,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        string diasLaborables)
    {
        var diaSemana = fechaHora.DayOfWeek;
        var diasPermitidos = ParseDiasLaborables(diasLaborables);

        if (!diasPermitidos.Contains(diaSemana))
            throw new FueraDeHorarioException(
                $"El día {diaSemana} no es un día laborable.");

        var horaConsulta = TimeOnly.FromDateTime(fechaHora);

        // La cita debe iniciar dentro del horario (inicio <= horaConsulta < cierre)
        if (horaConsulta < horaInicio || horaConsulta >= horaFin)
            throw new FueraDeHorarioException(
                $"El horario {horaConsulta:HH:mm} está fuera del rango laboral ({horaInicio:HH:mm}–{horaFin:HH:mm}).");
    }

    private static HashSet<DayOfWeek> ParseDiasLaborables(string diasLaborables)
    {
        return diasLaborables
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => (DayOfWeek)int.Parse(d))
            .ToHashSet();
    }
}
