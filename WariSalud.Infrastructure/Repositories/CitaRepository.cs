using Microsoft.EntityFrameworkCore;
using WariSalud.Core.Entities;
using WariSalud.Core.Interfaces;
using WariSalud.Infrastructure.Persistence;

namespace WariSalud.Infrastructure.Repositories;

public class CitaRepository : ICitaRepository
{
    private readonly ApplicationDbContext _context;

    public CitaRepository(ApplicationDbContext context)
        => _context = context;

    public async Task<Cita?> ObtenerPorIdAsync(int id)
        => await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .FirstOrDefaultAsync(c => c.Id == id);

    /// <summary>
    /// Obtiene las citas de un médico en una fecha específica.
    /// Usa el índice IX_Cita_MedicoId_FechaHora (T3.4) para RNF04.
    /// </summary>
    public async Task<IEnumerable<Cita>> ObtenerPorMedicoYFechaAsync(int medicoId, DateOnly fecha)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue);
        var fin = fecha.ToDateTime(TimeOnly.MaxValue);

        return await _context.Citas
            .AsNoTracking()
            .Include(c => c.Medico)
            .ThenInclude(m => m!.Especialidad)
            .Where(c => c.MedicoId == medicoId && c.FechaHora >= inicio && c.FechaHora <= fin)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene las citas activas de un paciente en una fecha específica.
    /// Usa el índice IX_Cita_PacienteId_FechaHora para RF05.
    /// </summary>
    public async Task<IEnumerable<Cita>> ObtenerPorPacienteYFechaAsync(int pacienteId, DateOnly fecha)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue);
        var fin = fecha.ToDateTime(TimeOnly.MaxValue);

        return await _context.Citas
            .AsNoTracking()
            .Where(c => c.PacienteId == pacienteId && c.FechaHora >= inicio && c.FechaHora <= fin)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cita>> ObtenerPorPacienteAsync(int pacienteId)
        => await _context.Citas
            .AsNoTracking()
            .Include(c => c.Medico)
            .ThenInclude(m => m!.Especialidad)
            .Where(c => c.PacienteId == pacienteId)
            .OrderByDescending(c => c.FechaHora)
            .ToListAsync();

    public async Task<Cita> AgregarAsync(Cita cita)
    {
        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();
        return cita;
    }

    public async Task ActualizarAsync(Cita cita)
    {
        _context.Citas.Update(cita);
        await _context.SaveChangesAsync();
    }
}
