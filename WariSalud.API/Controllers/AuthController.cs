using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WariSalud.API.DTOs;
using WariSalud.Core.Entities;
using WariSalud.Infrastructure.Persistence;

namespace WariSalud.API.Controllers;

/// <summary>
/// Endpoints de autenticación: registro y login. T4.2.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    /// <summary>Registra un nuevo usuario como Paciente.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { error = "El email ya está registrado." });

        var passwordHash = HashPassword(request.Password);

        var usuario = new Usuario
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Rol = "Paciente"
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            UsuarioId = usuario.Id,
            NombreCompleto = request.NombreCompleto,
            Telefono = request.Telefono
        };
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        var token = GenerarToken(usuario);
        return CreatedAtAction(nameof(Register), new AuthResponse(token, usuario.Email, usuario.Rol, usuario.Id));
    }

    /// <summary>Autentica un usuario y devuelve un JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario is null || !VerifyPassword(request.Password, usuario.PasswordHash))
            return Unauthorized(new { error = "Credenciales incorrectas." });

        var token = GenerarToken(usuario);
        return Ok(new AuthResponse(token, usuario.Email, usuario.Rol, usuario.Id));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private string GenerarToken(Usuario usuario)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("role", usuario.Rol),
            new Claim("rol", usuario.Rol),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim("email", usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
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

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);
        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }
}
