using WariSalud.Core.Entities;

namespace WariSalud.Core.Interfaces;

public interface IEspecialidadRepository
{
    Task<Especialidad?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Especialidad>> ObtenerTodasAsync();
    Task<Especialidad> AgregarAsync(Especialidad especialidad);
    Task ActualizarAsync(Especialidad especialidad);
    Task EliminarAsync(Especialidad especialidad);
}
