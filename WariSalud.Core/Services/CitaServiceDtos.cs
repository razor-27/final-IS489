namespace WariSalud.Core.Services;

public record AgendarCitaRequest(
    int PacienteId,
    int MedicoId,
    DateTime FechaHora,
    string Motivo
);

public record BloqueDisponible(
    DateTime Inicio,
    DateTime Fin
);
