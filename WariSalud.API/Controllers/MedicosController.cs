using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WariSalud.API.DTOs;
using WariSalud.Core.Entities;
using WariSalud.Core.Interfaces;
using WariSalud.Core.Services;
using WariSalud.Infrastructure.Persistence;

namespace WariSalud.API.Controllers;

/// <summary>
/// Gestión de médicos y consulta de disponibilidad. T4.3.
/// </summary>
[ApiController]
[Route("api/medicos")]
[Authorize]
public class MedicosController : ControllerBase
{
    private readonly IMedicoRepository _medicoRepository;
    private readonly ICitaService _citaService;
    private readonly ICitaRepository _citaRepository;
    private readonly ApplicationDbContext _context;

    public MedicosController(IMedicoRepository medicoRepository, ICitaService citaService, ICitaRepository citaRepository, ApplicationDbContext context)
    {
        _medicoRepository = medicoRepository;
        _citaService = citaService;
        _citaRepository = citaRepository;
        _context = context;
    }

    /// <summary>Lista todos los médicos (filtrado opcional por especialidad).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MedicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? especialidadId = null)
    {
        var medicos = await _medicoRepository.ObtenerTodosAsync(especialidadId);
        var response = medicos.Select(m => new MedicoResponse(
            m.Id,
            m.NombreCompleto,
            m.NumeroColegiatura,
            m.Activo,
            m.Especialidad is null ? null : new EspecialidadResponse(
                m.Especialidad.Id, m.Especialidad.Nombre, m.Especialidad.Descripcion, m.Especialidad.DuracionCitaMinutos)));
        return Ok(response);
    }

    /// <summary>
    /// Devuelve los horarios libres del médico para una fecha específica.
    /// RNF04: latencia &lt;500ms.
    /// </summary>
    [HttpGet("{id}/disponibilidad")]
    [ProducesResponseType(typeof(IEnumerable<DisponibilidadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerDisponibilidad(int id, [FromQuery] DateOnly fecha)
    {
        var bloques = await _citaService.ObtenerDisponibilidadAsync(id, fecha);
        var response = bloques.Select(b => new DisponibilidadResponse(b.Inicio.ToString("HH:mm"), b.Fin.ToString("HH:mm")));
        return Ok(response);
    }

    /// <summary>
    /// Devuelve la agenda del médico para una fecha. Solo el médico dueño o Admin.
    /// </summary>
    [HttpGet("{id}/agenda")]
    [Authorize(Roles = "Medico,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerAgenda(int id, [FromQuery] DateOnly fecha)
    {
        var medicos = await _medicoRepository.ObtenerTodosAsync();
        var medico = medicos.FirstOrDefault(m => m.Id == id || m.UsuarioId == id);
        if (medico is null)
            return Ok(Array.Empty<CitaResponse>());

        var citas = await _citaRepository.ObtenerPorMedicoYFechaAsync(medico.Id, fecha);
        var response = citas.Select(c => new CitaResponse(
            c.Id, c.PacienteId, c.MedicoId,
            c.FechaHora, c.DuracionMinutos, c.Estado, c.Motivo,
            new MedicoResponse(
                medico.Id, medico.NombreCompleto, medico.NumeroColegiatura, medico.Activo,
                medico.Especialidad is null ? null : new EspecialidadResponse(
                    medico.Especialidad.Id, medico.Especialidad.Nombre, medico.Especialidad.Descripcion, medico.Especialidad.DuracionCitaMinutos))));
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearMedicoRequest request)
    {
        var passwordToUse = string.IsNullOrWhiteSpace(request.Password) ? "medico123" : request.Password;
        var user = new Usuario
        {
            Email = request.Email,
            PasswordHash = HashPassword(passwordToUse),
            Rol = "Medico"
        };
        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();

        var medico = new Medico
        {
            UsuarioId = user.Id,
            EspecialidadId = request.EspecialidadId,
            NombreCompleto = request.NombreCompleto,
            NumeroColegiatura = request.NumeroColegiatura,
            Activo = true
        };
        var creado = await _medicoRepository.AgregarAsync(medico);
        
        var response = new MedicoResponse(
            creado.Id,
            creado.NombreCompleto,
            creado.NumeroColegiatura,
            creado.Activo,
            null);

        return StatusCode(201, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CrearMedicoRequest request)
    {
        var m = await _medicoRepository.ObtenerPorIdAsync(id);
        if (m is null) return NotFound();
        m.NombreCompleto = request.NombreCompleto;
        m.NumeroColegiatura = request.NumeroColegiatura;
        m.EspecialidadId = request.EspecialidadId;
        await _medicoRepository.ActualizarAsync(m);
        
        var response = new MedicoResponse(
            m.Id,
            m.NombreCompleto,
            m.NumeroColegiatura,
            m.Activo,
            m.Especialidad is null ? null : new EspecialidadResponse(
                m.Especialidad.Id, m.Especialidad.Nombre, m.Especialidad.Descripcion, m.Especialidad.DuracionCitaMinutos));

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var m = await _medicoRepository.ObtenerPorIdAsync(id);
        if (m is null) return NotFound();
        await _medicoRepository.EliminarAsync(m);
        return NoContent();
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
