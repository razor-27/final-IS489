using WariSalud.Core.Entities;

namespace WariSalud.Core.Services;

public interface ICitaService
{
    /// <summary>
    /// Agenda una nueva cita aplicando validaciones RF02-RF05.
    /// </summary>
    Task<Cita> AgendarCitaAsync(AgendarCitaRequest request);

    /// <summary>
    /// Cancela una cita existente aplicando validaciones RF06 y control de propiedad.
    /// </summary>
    /// <param name="citaId">ID de la cita a cancelar.</param>
    /// <param name="pacienteIdSolicitante">ID del paciente que solicita la cancelación (para validar propiedad).</param>
    Task CancelarCitaAsync(int citaId, int pacienteIdSolicitante);

    /// <summary>
    /// Devuelve los bloques de tiempo libres del médico para una fecha específica.
    /// </summary>
    Task<IEnumerable<BloqueDisponible>> ObtenerDisponibilidadAsync(int medicoId, DateOnly fecha);
}
