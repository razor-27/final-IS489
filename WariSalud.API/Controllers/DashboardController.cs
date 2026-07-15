using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WariSalud.Infrastructure.Persistence;

namespace WariSalud.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var hoy = DateTime.Today;
        var mañana = hoy.AddDays(1);

        var citasHoy = await _context.Citas
            .Where(c => c.FechaHora >= hoy && c.FechaHora < mañana)
            .CountAsync();

        var pacientes = await _context.Pacientes.CountAsync();

        return Ok(new
        {
            citasHoy = citasHoy,
            pacientes = pacientes
        });
    }
}
