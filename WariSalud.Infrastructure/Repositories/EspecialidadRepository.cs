using Microsoft.EntityFrameworkCore;
using WariSalud.Core.Entities;
using WariSalud.Core.Interfaces;
using WariSalud.Infrastructure.Persistence;

namespace WariSalud.Infrastructure.Repositories;

public class EspecialidadRepository : IEspecialidadRepository
{
    private readonly ApplicationDbContext _context;

    public EspecialidadRepository(ApplicationDbContext context)
        => _context = context;

    public async Task<Especialidad?> ObtenerPorIdAsync(int id)
        => await _context.Especialidades.FindAsync(id);

    public async Task<IEnumerable<Especialidad>> ObtenerTodasAsync()
        => await _context.Especialidades.AsNoTracking().ToListAsync();

    public async Task<Especialidad> AgregarAsync(Especialidad especialidad)
    {
        _context.Especialidades.Add(especialidad);
        await _context.SaveChangesAsync();
        return especialidad;
    }

    public async Task ActualizarAsync(Especialidad especialidad)
    {
        _context.Especialidades.Update(especialidad);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Especialidad especialidad)
    {
        _context.Especialidades.Remove(especialidad);
        await _context.SaveChangesAsync();
    }
}
