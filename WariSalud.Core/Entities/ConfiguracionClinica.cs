namespace WariSalud.Core.Entities;

/// <summary>
/// Horario global de la clínica. Tabla de una sola fila (decisión spec.md §9.1).
/// </summary>
public class ConfiguracionClinica
{
    public int Id { get; set; }

    /// <summary>Hora de apertura de la clínica (default 08:00).</summary>
    public TimeOnly HoraApertura { get; set; } = new TimeOnly(8, 0);

    /// <summary>Hora de cierre de la clínica (default 20:00).</summary>
    public TimeOnly HoraCierre { get; set; } = new TimeOnly(20, 0);

    /// <summary>
    /// Días laborables como flags de DayOfWeek separados por coma.
    /// Ej: "1,2,3,4,5,6" = Lunes a Sábado.
    /// </summary>
    public string DiasLaborables { get; set; } = "1,2,3,4,5,6";
}
