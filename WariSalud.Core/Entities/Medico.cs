namespace WariSalud.Core.Entities;

public class Medico
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int EspecialidadId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NumeroColegiatura { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public Usuario? Usuario { get; set; }
    public Especialidad? Especialidad { get; set; }
    public HorarioMedico? HorarioMedico { get; set; }
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();

    /// <summary>
    /// Resuelve el horario efectivo del médico: HorarioMedico ?? HorarioClinica.
    /// Implementa la decisión spec.md §9.1 (fallback en cascada).
    /// T1.1b.
    /// </summary>
    public (TimeOnly HoraInicio, TimeOnly HoraFin, string DiasLaborables)
        ObtenerHorarioEfectivo(ConfiguracionClinica configuracionClinica)
    {
        if (HorarioMedico is not null)
        {
            return (HorarioMedico.HoraInicio, HorarioMedico.HoraFin, HorarioMedico.DiasLaborables);
        }

        return (configuracionClinica.HoraApertura, configuracionClinica.HoraCierre, configuracionClinica.DiasLaborables);
    }
}
