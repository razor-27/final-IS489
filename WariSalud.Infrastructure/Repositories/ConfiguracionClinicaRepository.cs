using Microsoft.EntityFrameworkCore;
using WariSalud.Core.Entities;
using WariSalud.Core.Interfaces;
using WariSalud.Infrastructure.Persistence;

namespace WariSalud.Infrastructure.Repositories;

public class ConfiguracionClinicaRepository : IConfiguracionClinicaRepository
{
    private readonly ApplicationDbContext _context;

    public ConfiguracionClinicaRepository(ApplicationDbContext context)
        => _context = context;

    /// <summary>
    /// Siempre devuelve el registro único de configuración global.
    /// Si no existe (p. ej. BD nueva sin seed), crea un registro por defecto.
    /// </summary>
    public async Task<ConfiguracionClinica> ObtenerConfiguracionAsync()
    {
        var config = await _context.ConfiguracionClinica.AsNoTracking().FirstOrDefaultAsync();
        if (config is null)
        {
            config = new ConfiguracionClinica
            {
                Id = 1,
                HoraApertura = new TimeOnly(8, 0),
                HoraCierre = new TimeOnly(20, 0),
                DiasLaborables = "1,2,3,4,5,6"
            };
            _context.ConfiguracionClinica.Add(config);
            await _context.SaveChangesAsync();
        }
        return config;
    }

    public async Task ActualizarAsync(ConfiguracionClinica configuracion)
    {
        _context.ConfiguracionClinica.Update(configuracion);
        await _context.SaveChangesAsync();
    }
}
