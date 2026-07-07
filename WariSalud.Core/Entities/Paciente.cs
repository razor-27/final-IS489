namespace WariSalud.Core.Entities;

public class Paciente
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    public Usuario? Usuario { get; set; }
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
