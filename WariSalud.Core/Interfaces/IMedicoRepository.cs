using WariSalud.Core.Entities;

namespace WariSalud.Core.Interfaces;

public interface IMedicoRepository
{
    Task<Medico?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Medico>> ObtenerTodosAsync(int? especialidadId = null);
    Task<Medico> AgregarAsync(Medico medico);
    Task ActualizarAsync(Medico medico);
    Task EliminarAsync(Medico medico);
}
