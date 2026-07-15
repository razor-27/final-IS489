using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WariSalud.API.DTOs;
using WariSalud.Core.Exceptions;
using WariSalud.Core.Interfaces;
using WariSalud.Core.Services;

namespace WariSalud.API.Controllers;

/// <summary>
/// Gestión de citas médicas: agendar, cancelar y consultar. T4.4.
/// </summary>
[ApiController]
[Route("api/citas")]
[Authorize(Roles = "Paciente")]
public class CitasController : ControllerBase
{
    private readonly ICitaService _citaService;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly ICitaRepository _citaRepository;

    public CitasController(ICitaService citaService, IPacienteRepository pacienteRepository, ICitaRepository citaRepository)
    {
        _citaService = citaService;
        _pacienteRepository = pacienteRepository;
        _citaRepository = citaRepository;
    }

    /// <summary>
    /// Agenda una nueva cita para el paciente autenticado.
    /// Aplica RF02, RF03, RF04, RF05.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CitaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AgendarCita([FromBody] AgendarCitaRequestDto request)
    {
        var pacienteId = await ObtenerPacienteIdDelTokenAsync();

        var agendarRequest = new AgendarCitaRequest(
            PacienteId: pacienteId,
            MedicoId: request.MedicoId,
            FechaHora: request.FechaHora,
            Motivo: request.Motivo
        );

        var cita = await _citaService.AgendarCitaAsync(agendarRequest);

        var response = new CitaResponse(
            cita.Id, cita.PacienteId, cita.MedicoId,
            cita.FechaHora, cita.DuracionMinutos, cita.Estado, cita.Motivo);

        return CreatedAtAction(nameof(MisCitas), response);
    }

    /// <summary>
    /// Cancela una cita del paciente autenticado.
    /// Aplica RF06 y validación de propiedad (RNF03).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelarCita(int id)
    {
        var pacienteId = await ObtenerPacienteIdDelTokenAsync();
        await _citaService.CancelarCitaAsync(id, pacienteId);
        return NoContent();
    }

    [HttpGet("mias")]
    [ProducesResponseType(typeof(IEnumerable<CitaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MisCitas()
    {
        var pacienteId = await ObtenerPacienteIdDelTokenAsync();
        var citas = await _citaRepository.ObtenerPorPacienteAsync(pacienteId);
        
        var ahora = DateTime.UtcNow;
        var response = citas.Select(c => 
        {
            var estadoReal = c.Estado;
            if (estadoReal == WariSalud.Core.Entities.EstadoCita.Pendiente && c.FechaHora.AddMinutes(c.DuracionMinutos) < ahora)
            {
                estadoReal = WariSalud.Core.Entities.EstadoCita.Completada;
            }

            return new CitaResponse(
                c.Id, c.PacienteId, c.MedicoId,
                c.FechaHora, c.DuracionMinutos, estadoReal, c.Motivo,
                c.Medico is null ? null : new MedicoResponse(
                    c.Medico.Id, c.Medico.NombreCompleto, c.Medico.NumeroColegiatura, c.Medico.Activo,
                    c.Medico.Especialidad is null ? null : new EspecialidadResponse(
                        c.Medico.Especialidad.Id, c.Medico.Especialidad.Nombre, c.Medico.Especialidad.Descripcion, c.Medico.Especialidad.DuracionCitaMinutos)));
        });
        return Ok(response);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<int> ObtenerPacienteIdDelTokenAsync()
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Token inválido: falta el claim 'sub'.");

        var usuarioId = int.Parse(usuarioIdClaim);
        var paciente = await _pacienteRepository.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new RecursoNoEncontradoException("Perfil de paciente", usuarioId);

        return paciente.Id;
    }
}
