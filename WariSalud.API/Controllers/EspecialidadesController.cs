using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WariSalud.API.DTOs;
using WariSalud.Core.Entities;
using WariSalud.Core.Interfaces;

namespace WariSalud.API.Controllers;

/// <summary>
/// Gestión de especialidades médicas. T4.3.
/// </summary>
[ApiController]
[Route("api/especialidades")]
[Authorize]
public class EspecialidadesController : ControllerBase
{
    private readonly IEspecialidadRepository _repository;

    public EspecialidadesController(IEspecialidadRepository repository)
        => _repository = repository;

    /// <summary>Lista todas las especialidades disponibles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EspecialidadResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodas()
    {
        var especialidades = await _repository.ObtenerTodasAsync();
        var response = especialidades.Select(e => new EspecialidadResponse(
            e.Id, e.Nombre, e.Descripcion, e.DuracionCitaMinutos));
        return Ok(response);
    }

    /// <summary>Crea una nueva especialidad. Solo Admin.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EspecialidadResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] CrearEspecialidadRequest request)
    {
        var especialidad = new Especialidad
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            DuracionCitaMinutos = request.DuracionCitaMinutos
        };
        var creada = await _repository.AgregarAsync(especialidad);
        var response = new EspecialidadResponse(creada.Id, creada.Nombre, creada.Descripcion, creada.DuracionCitaMinutos);
        return CreatedAtAction(nameof(ObtenerTodas), response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CrearEspecialidadRequest request)
    {
        var esp = await _repository.ObtenerPorIdAsync(id);
        if (esp is null) return NotFound();
        esp.Nombre = request.Nombre;
        esp.Descripcion = request.Descripcion;
        esp.DuracionCitaMinutos = request.DuracionCitaMinutos;
        await _repository.ActualizarAsync(esp);
        return Ok(new EspecialidadResponse(esp.Id, esp.Nombre, esp.Descripcion, esp.DuracionCitaMinutos));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var esp = await _repository.ObtenerPorIdAsync(id);
        if (esp is null) return NotFound();
        await _repository.EliminarAsync(esp);
        return NoContent();
    }
}
