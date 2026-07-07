namespace WariSalud.Core.Entities;

public class Especialidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Duración en minutos de las citas de esta especialidad (default 30).
    /// Decisión spec.md §9.2.
    /// </summary>
    public int DuracionCitaMinutos { get; set; } = 30;

    public ICollection<Medico> Medicos { get; set; } = new List<Medico>();
}
