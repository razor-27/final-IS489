namespace WariSalud.API.DTOs;

// ─── Auth ────────────────────────────────────────────────────────────────────

public record RegisterRequest(
    string Email,
    string Password,
    string NombreCompleto,
    string Telefono
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    string Email,
    string Rol,
    int UsuarioId
);

// ─── Especialidades ───────────────────────────────────────────────────────────

public record CrearEspecialidadRequest(
    string Nombre,
    string Descripcion,
    int DuracionCitaMinutos = 30
);

public record EspecialidadResponse(
    int Id,
    string Nombre,
    string Descripcion,
    int DuracionCitaMinutos
);

// ─── Médicos ──────────────────────────────────────────────────────────────────

public record MedicoResponse(
    int Id,
    string NombreCompleto,
    string NumeroColegiatura,
    bool Activo,
    EspecialidadResponse? Especialidad
);

public record DisponibilidadResponse(
    string HoraInicio,
    string HoraFin
);

// ─── Citas ────────────────────────────────────────────────────────────────────

public record AgendarCitaRequestDto(
    int MedicoId,
    DateTime FechaHora,
    string Motivo
);

public record CitaResponse(
    int Id,
    int PacienteId,
    int MedicoId,
    DateTime FechaHora,
    int DuracionMinutos,
    string Estado,
    string Motivo,
    MedicoResponse? Medico = null
);

public record CrearMedicoRequest(
    string NombreCompleto,
    string NumeroColegiatura,
    int EspecialidadId,
    string Email,
    string? Password
);

public record ActualizarConfiguracionRequest(
    string HoraApertura,
    string HoraCierre,
    string DiasLaborables
);
