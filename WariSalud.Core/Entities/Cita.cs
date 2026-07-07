namespace WariSalud.Core.Entities;

public class Cita
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }
    public DateTime FechaHora { get; set; }

    /// <summary>
    /// Snapshot de Especialidad.DuracionCitaMinutos al momento de crear la cita.
    /// Decisión spec.md §9.2: un cambio futuro en la especialidad no altera citas ya agendadas.
    /// </summary>
    public int DuracionMinutos { get; set; }

    public string Estado { get; set; } = EstadoCita.Pendiente;
    public string Motivo { get; set; } = string.Empty;

    public Paciente? Paciente { get; set; }
    public Medico? Medico { get; set; }

    /// <summary>Fin calculado de la cita (FechaHora + DuracionMinutos).</summary>
    public DateTime FechaHoraFin => FechaHora.AddMinutes(DuracionMinutos);

    /// <summary>Indica si la cita está activa (Pendiente).</summary>
    public bool EstaActiva => Estado == EstadoCita.Pendiente;

    /// <summary>
    /// Verifica si esta cita se solapa con el rango [inicio, fin).
    /// Solapamiento parcial o total incluido.
    /// </summary>
    public bool SeSolapa(DateTime inicio, DateTime fin)
    {
        return inicio < FechaHoraFin && fin > FechaHora;
    }
}

public static class EstadoCita
{
    public const string Pendiente = "Pendiente";
    public const string Completada = "Completada";
    public const string Cancelada = "Cancelada";
}
