using WariSalud.Core.Entities;

namespace WariSalud.Core.Interfaces;

public interface IPacienteRepository
{
    Task<Paciente?> ObtenerPorIdAsync(int id);
    Task<Paciente?> ObtenerPorUsuarioIdAsync(int usuarioId);
    Task<IEnumerable<Paciente>> ObtenerTodosAsync();
    Task<Paciente> AgregarAsync(Paciente paciente);
    Task ActualizarAsync(Paciente paciente);
}
