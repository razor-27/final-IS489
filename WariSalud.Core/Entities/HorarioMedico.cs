namespace WariSalud.Core.Entities;

/// <summary>
/// Override opcional del horario de clínica para un médico específico (decisión spec.md §9.1).
/// </summary>
public class HorarioMedico
{
    public int Id { get; set; }
    public int MedicoId { get; set; }

    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    /// <summary>
    /// Días laborables como flags de DayOfWeek separados por coma.
    /// Ej: "1,2,3,4,5" = Lunes a Viernes.
    /// </summary>
    public string DiasLaborables { get; set; } = "1,2,3,4,5";

    public Medico? Medico { get; set; }
}
