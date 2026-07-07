using Microsoft.EntityFrameworkCore;
using WariSalud.Core.Entities;
using WariSalud.Core.Interfaces;
using WariSalud.Infrastructure.Persistence;

namespace WariSalud.Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly ApplicationDbContext _context;

    public PacienteRepository(ApplicationDbContext context)
        => _context = context;

    public async Task<Paciente?> ObtenerPorIdAsync(int id)
        => await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Paciente?> ObtenerPorUsuarioIdAsync(int usuarioId)
        => await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

    public async Task<IEnumerable<Paciente>> ObtenerTodosAsync()
        => await _context.Pacientes.Include(p => p.Usuario).ToListAsync();

    public async Task<Paciente> AgregarAsync(Paciente paciente)
    {
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();
        return paciente;
    }

    public async Task ActualizarAsync(Paciente paciente)
    {
        _context.Pacientes.Update(paciente);
        await _context.SaveChangesAsync();
    }
}
