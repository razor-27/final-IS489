using WariSalud.Core.Entities;

namespace WariSalud.Core.Interfaces;

public interface ICitaRepository
{
    Task<Cita?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Cita>> ObtenerPorMedicoYFechaAsync(int medicoId, DateOnly fecha);
    Task<IEnumerable<Cita>> ObtenerPorPacienteYFechaAsync(int pacienteId, DateOnly fecha);
    Task<IEnumerable<Cita>> ObtenerPorPacienteAsync(int pacienteId);
    Task<Cita> AgregarAsync(Cita cita);
    Task ActualizarAsync(Cita cita);
}
