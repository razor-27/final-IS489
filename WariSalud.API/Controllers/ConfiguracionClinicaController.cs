using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WariSalud.API.DTOs;
using WariSalud.Core.Interfaces;

namespace WariSalud.API.Controllers;

[ApiController]
[Route("api/configuracion-clinica")]
public class ConfiguracionClinicaController : ControllerBase
{
    private readonly IConfiguracionClinicaRepository _repository;

    public ConfiguracionClinicaController(IConfiguracionClinicaRepository repository)
        => _repository = repository;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener()
    {
        var config = await _repository.ObtenerConfiguracionAsync();
        return Ok(new
        {
            id = config.Id,
            horaApertura = config.HoraApertura.ToString("HH:mm"),
            horaCierre = config.HoraCierre.ToString("HH:mm"),
            diasLaborables = config.DiasLaborables
        });
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar([FromBody] ActualizarConfiguracionRequest request)
    {
        var config = await _repository.ObtenerConfiguracionAsync();
        config.HoraApertura = TimeOnly.Parse(request.HoraApertura);
        config.HoraCierre = TimeOnly.Parse(request.HoraCierre);
        config.DiasLaborables = request.DiasLaborables;

        await _repository.ActualizarAsync(config);

        return Ok(new
        {
            id = config.Id,
            horaApertura = config.HoraApertura.ToString("HH:mm"),
            horaCierre = config.HoraCierre.ToString("HH:mm"),
            diasLaborables = config.DiasLaborables
        });
    }
}
