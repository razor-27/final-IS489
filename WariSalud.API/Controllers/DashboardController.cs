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
        var peruHoy = WariSalud.Core.Utils.TimeHelper.GetPeruTime().Date;
        var peruMañana = peruHoy.AddDays(1);

        var utcHoy = WariSalud.Core.Utils.TimeHelper.ToUtc(peruHoy);
        var utcMañana = WariSalud.Core.Utils.TimeHelper.ToUtc(peruMañana);

        var citasHoy = await _context.Citas
            .Where(c => c.FechaHora >= utcHoy && c.FechaHora < utcMañana)
            .CountAsync();

        var pacientes = await _context.Pacientes.CountAsync();

        return Ok(new
        {
            citasHoy = citasHoy,
            pacientes = pacientes
        });
    }
}
