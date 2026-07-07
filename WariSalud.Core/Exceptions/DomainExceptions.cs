namespace WariSalud.Core.Exceptions;

/// <summary>
/// Se lanza cuando se intenta agendar una cita que se solapa con otra ya existente del mismo médico.
/// RF03 — mapeado a HTTP 409 Conflict.
/// </summary>
public class DoubleBookingException : Exception
{
    public DoubleBookingException()
        : base("El médico ya tiene una cita en ese horario. Por favor elija otro horario.") { }

    public DoubleBookingException(string message) : base(message) { }
}

/// <summary>
/// Se lanza cuando se intenta agendar fuera del horario laboral configurado.
/// RF04 — mapeado a HTTP 422 Unprocessable Entity.
/// </summary>
public class FueraDeHorarioException : Exception
{
    public FueraDeHorarioException()
        : base("La fecha/hora solicitada está fuera del horario laboral de la clínica.") { }

    public FueraDeHorarioException(string message) : base(message) { }
}

/// <summary>
/// Se lanza cuando el paciente supera el límite de 2 citas activas en el mismo día.
/// RF05 — mapeado a HTTP 409 Conflict.
/// </summary>
public class LimiteDeCitasException : Exception
{
    public LimiteDeCitasException()
        : base("El paciente ya tiene el máximo de 2 citas activas para ese día.") { }

    public LimiteDeCitasException(string message) : base(message) { }
}

/// <summary>
/// Se lanza cuando se intenta cancelar una cita con menos de 24 horas de antelación.
/// RF06 — mapeado a HTTP 422 Unprocessable Entity.
/// </summary>
public class CancelacionFueraDePlazoException : Exception
{
    public CancelacionFueraDePlazoException()
        : base("La cancelación solo es posible con al menos 24 horas de anticipación.") { }

    public CancelacionFueraDePlazoException(string message) : base(message) { }
}

/// <summary>
/// Se lanza cuando un recurso solicitado no existe o está inactivo.
/// Mapeado a HTTP 404 Not Found.
/// </summary>
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string recurso, object id)
        : base($"{recurso} con id '{id}' no fue encontrado o está inactivo.") { }

    public RecursoNoEncontradoException(string message) : base(message) { }
}

/// <summary>
/// Se lanza cuando un usuario intenta operar sobre un recurso que no le pertenece.
/// RNF03 — mapeado a HTTP 403 Forbidden.
/// </summary>
public class AccesoNoAutorizadoException : Exception
{
    public AccesoNoAutorizadoException()
        : base("No tiene permiso para realizar esta operación sobre este recurso.") { }

    public AccesoNoAutorizadoException(string message) : base(message) { }
}
