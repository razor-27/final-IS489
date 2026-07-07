using Microsoft.EntityFrameworkCore;
using WariSalud.Core.Entities;
using WariSalud.Core.Interfaces;
using WariSalud.Infrastructure.Persistence;

namespace WariSalud.Infrastructure.Repositories;

public class MedicoRepository : IMedicoRepository
{
    private readonly ApplicationDbContext _context;

    public MedicoRepository(ApplicationDbContext context)
        => _context = context;

    public async Task<Medico?> ObtenerPorIdAsync(int id)
        => await _context.Medicos
            .Include(m => m.Usuario)
            .Include(m => m.Especialidad)
            .Include(m => m.HorarioMedico)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<Medico>> ObtenerTodosAsync(int? especialidadId = null)
    {
        var query = _context.Medicos
            .Include(m => m.Especialidad)
            .Include(m => m.HorarioMedico)
            .AsQueryable();

        if (especialidadId.HasValue)
            query = query.Where(m => m.EspecialidadId == especialidadId.Value);

        return await query.ToListAsync();
    }

    public async Task<Medico> AgregarAsync(Medico medico)
    {
        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();
        return medico;
    }

    public async Task ActualizarAsync(Medico medico)
    {
        _context.Medicos.Update(medico);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Medico medico)
    {
        _context.Medicos.Remove(medico);
        await _context.SaveChangesAsync();
    }
}
