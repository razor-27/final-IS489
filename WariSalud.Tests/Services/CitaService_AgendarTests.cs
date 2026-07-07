using FluentAssertions;
using Moq;
using WariSalud.Core.Entities;
using WariSalud.Core.Exceptions;
using WariSalud.Core.Interfaces;
using WariSalud.Core.Services;

namespace WariSalud.Tests.Services;

/// <summary>
/// Tests unitarios para CitaService.AgendarCitaAsync (T2.1).
/// Cubre los 5 escenarios obligatorios + edge cases de spec.md §7.
/// Patrón: AAA (Arrange, Act, Assert) con Moq.
/// </summary>
public class CitaService_AgendarTests
{
    // ─── Setup compartido ────────────────────────────────────────────────────

    private readonly Mock<ICitaRepository> _citaRepoMock = new();
    private readonly Mock<IMedicoRepository> _medicoRepoMock = new();
    private readonly Mock<IConfiguracionClinicaRepository> _configRepoMock = new();

    private CitaService CrearServicio() =>
        new CitaService(_citaRepoMock.Object, _medicoRepoMock.Object, _configRepoMock.Object);

    private static ConfiguracionClinica ConfigClinicaDefault() => new()
    {
        Id = 1,
        HoraApertura = new TimeOnly(8, 0),
        HoraCierre = new TimeOnly(20, 0),
        DiasLaborables = "1,2,3,4,5,6" // Lunes–Sábado
    };

    private static Medico MedicoActivo(int id = 1) => new()
    {
        Id = id,
        Activo = true,
        Especialidad = new Especialidad { DuracionCitaMinutos = 30 },
        HorarioMedico = null // usa horario de clínica
    };

    // ─── T2.1 — Test 1: Agendar en horario válido → éxito ───────────────────

    [Fact]
    public async Task AgendarCita_HorarioValidoYDisponible_DebePersistirYRetornarCita()
    {
        // Arrange
        var medico = MedicoActivo();
        var config = ConfigClinicaDefault();
        var request = new AgendarCitaRequest(
            PacienteId: 1,
            MedicoId: 1,
            FechaHora: ObtenerProximoLunes(new TimeOnly(10, 0)),
            Motivo: "Control anual"
        );
        var citaCreada = new Cita { Id = 42, PacienteId = 1, MedicoId = 1, Estado = EstadoCita.Pendiente };

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);
        _citaRepoMock.Setup(r => r.ObtenerPorMedicoYFechaAsync(1, DateOnly.FromDateTime(request.FechaHora)))
            .ReturnsAsync(new List<Cita>());
        _citaRepoMock.Setup(r => r.ObtenerPorPacienteYFechaAsync(1, DateOnly.FromDateTime(request.FechaHora)))
            .ReturnsAsync(new List<Cita>());
        _citaRepoMock.Setup(r => r.AgregarAsync(It.IsAny<Cita>())).ReturnsAsync(citaCreada);

        // Act
        var resultado = await CrearServicio().AgendarCitaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(42);
        _citaRepoMock.Verify(r => r.AgregarAsync(It.IsAny<Cita>()), Times.Once);
    }

    // ─── T2.1 — Test 2: Agendar a las 20:00+ → FueraDeHorarioException ──────
    // spec.md §7 caso 1

    [Theory]
    [InlineData(20, 0)]   // exactamente a las 20:00 (igual al cierre → rechazado)
    [InlineData(20, 30)]  // después del cierre
    [InlineData(21, 0)]   // muy tarde
    public async Task AgendarCita_HoraIgualODespuesDeCierre_DebeLanzarFueraDeHorario(int hora, int minuto)
    {
        // Arrange
        var medico = MedicoActivo();
        var config = ConfigClinicaDefault(); // cierre 20:00
        var fechaHora = ObtenerProximoLunes(new TimeOnly(hora, minuto));
        var request = new AgendarCitaRequest(1, 1, fechaHora, "test");

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        // Act
        var act = async () => await CrearServicio().AgendarCitaAsync(request);

        // Assert
        await act.Should().ThrowAsync<FueraDeHorarioException>();
    }

    // ─── T2.1 — Test 3: 3ª cita del día para el mismo paciente → LimiteDeCitasException
    // spec.md §7 caso 2

    [Fact]
    public async Task AgendarCita_TerceraCitaDelDia_DebeLanzarLimiteDeCitas()
    {
        // Arrange
        var medico = MedicoActivo();
        var config = ConfigClinicaDefault();
        var fecha = ObtenerProximoLunes(new TimeOnly(10, 0));
        var request = new AgendarCitaRequest(1, 1, fecha, "tercera cita");

        var citasActivas = new List<Cita>
        {
            new() { PacienteId = 1, Estado = EstadoCita.Pendiente, FechaHora = fecha.AddHours(-3), DuracionMinutos = 30 },
            new() { PacienteId = 1, Estado = EstadoCita.Pendiente, FechaHora = fecha.AddHours(-2), DuracionMinutos = 30 }
        };

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);
        _citaRepoMock.Setup(r => r.ObtenerPorMedicoYFechaAsync(1, DateOnly.FromDateTime(fecha)))
            .ReturnsAsync(new List<Cita>());
        _citaRepoMock.Setup(r => r.ObtenerPorPacienteYFechaAsync(1, DateOnly.FromDateTime(fecha)))
            .ReturnsAsync(citasActivas);

        // Act
        var act = async () => await CrearServicio().AgendarCitaAsync(request);

        // Assert
        await act.Should().ThrowAsync<LimiteDeCitasException>();
    }

    // ─── T2.1 — Test 4: Horario ya ocupado del médico → DoubleBookingException
    // spec.md §7 caso 5

    [Fact]
    public async Task AgendarCita_HorarioOcupadoDelMedico_DebeLanzarDoubleBooking()
    {
        // Arrange
        var medico = MedicoActivo();
        var config = ConfigClinicaDefault();
        var fecha = ObtenerProximoLunes(new TimeOnly(10, 0));
        var request = new AgendarCitaRequest(2, 1, fecha, "solapamiento");

        var citaExistente = new Cita
        {
            PacienteId = 99,
            MedicoId = 1,
            FechaHora = fecha,
            DuracionMinutos = 30,
            Estado = EstadoCita.Pendiente
        };

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);
        _citaRepoMock.Setup(r => r.ObtenerPorMedicoYFechaAsync(1, DateOnly.FromDateTime(fecha)))
            .ReturnsAsync(new List<Cita> { citaExistente });
        _citaRepoMock.Setup(r => r.ObtenerPorPacienteYFechaAsync(2, DateOnly.FromDateTime(fecha)))
            .ReturnsAsync(new List<Cita>());

        // Act
        var act = async () => await CrearServicio().AgendarCitaAsync(request);

        // Assert
        await act.Should().ThrowAsync<DoubleBookingException>();
    }

    // ─── T2.1 — Test 5: Médico inexistente → RecursoNoEncontradoException
    // spec.md §7 caso 6

    [Fact]
    public async Task AgendarCita_MedicoInexistente_DebeLanzarRecursoNoEncontrado()
    {
        // Arrange
        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Medico?)null);
        var request = new AgendarCitaRequest(1, 999, DateTime.UtcNow.AddDays(1), "test");

        // Act
        var act = async () => await CrearServicio().AgendarCitaAsync(request);

        // Assert
        await act.Should().ThrowAsync<RecursoNoEncontradoException>();
    }

    // ─── Test adicional: Médico inactivo → RecursoNoEncontradoException ──────

    [Fact]
    public async Task AgendarCita_MedicoInactivo_DebeLanzarRecursoNoEncontrado()
    {
        // Arrange
        var medicoInactivo = MedicoActivo();
        medicoInactivo.Activo = false;

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medicoInactivo);
        var request = new AgendarCitaRequest(1, 1, DateTime.UtcNow.AddDays(1), "test");

        // Act
        var act = async () => await CrearServicio().AgendarCitaAsync(request);

        // Assert
        await act.Should().ThrowAsync<RecursoNoEncontradoException>();
    }

    // ─── Test adicional: Agendar antes de la apertura → FueraDeHorarioException

    [Fact]
    public async Task AgendarCita_AntesDeApertura_DebeLanzarFueraDeHorario()
    {
        // Arrange
        var medico = MedicoActivo();
        var config = ConfigClinicaDefault();
        var fechaHora = ObtenerProximoLunes(new TimeOnly(7, 30));
        var request = new AgendarCitaRequest(1, 1, fechaHora, "muy temprano");

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        // Act
        var act = async () => await CrearServicio().AgendarCitaAsync(request);

        // Assert
        await act.Should().ThrowAsync<FueraDeHorarioException>();
    }

    // ─── Test adicional: Día no laborable → FueraDeHorarioException ──────────

    [Fact]
    public async Task AgendarCita_DiaDomingo_DebeLanzarFueraDeHorario()
    {
        // Arrange — clínica: Lu-Sa (sin domingo)
        var medico = MedicoActivo();
        var config = ConfigClinicaDefault();

        // Encontrar el próximo domingo
        var hoy = DateTime.UtcNow.Date;
        var diasHastaDomingo = ((int)DayOfWeek.Sunday - (int)hoy.DayOfWeek + 7) % 7;
        if (diasHastaDomingo == 0) diasHastaDomingo = 7;
        var domingo = hoy.AddDays(diasHastaDomingo).AddHours(10);

        var request = new AgendarCitaRequest(1, 1, domingo, "domingo");

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        // Act
        var act = async () => await CrearServicio().AgendarCitaAsync(request);

        // Assert
        await act.Should().ThrowAsync<FueraDeHorarioException>();
    }

    // ─── Test adicional: Solapamiento parcial (inicio dentro de cita existente)

    [Fact]
    public async Task AgendarCita_InicioEnMitadDeCitaExistente_DebeLanzarDoubleBooking()
    {
        // Arrange
        var medico = MedicoActivo();
        var config = ConfigClinicaDefault();
        var fechaBase = ObtenerProximoLunes(new TimeOnly(10, 0));

        var citaExistente = new Cita
        {
            PacienteId = 5,
            MedicoId = 1,
            FechaHora = fechaBase,         // 10:00 - 10:30
            DuracionMinutos = 30,
            Estado = EstadoCita.Pendiente
        };

        var request = new AgendarCitaRequest(2, 1, fechaBase.AddMinutes(15), "solapamiento parcial");

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);
        _citaRepoMock.Setup(r => r.ObtenerPorMedicoYFechaAsync(1, DateOnly.FromDateTime(fechaBase)))
            .ReturnsAsync(new List<Cita> { citaExistente });
        _citaRepoMock.Setup(r => r.ObtenerPorPacienteYFechaAsync(2, DateOnly.FromDateTime(fechaBase.AddMinutes(15))))
            .ReturnsAsync(new List<Cita>());

        // Act
        var act = async () => await CrearServicio().AgendarCitaAsync(request);

        // Assert
        await act.Should().ThrowAsync<DoubleBookingException>();
    }

    // ─── Test adicional: Cita cancelada no bloquea el horario ────────────────

    [Fact]
    public async Task AgendarCita_HorarioConCitaCancelada_DebePermitirNuevaCita()
    {
        // Arrange
        var medico = MedicoActivo();
        var config = ConfigClinicaDefault();
        var fecha = ObtenerProximoLunes(new TimeOnly(10, 0));
        var request = new AgendarCitaRequest(2, 1, fecha, "reemplazo de cancelada");

        var citaCancelada = new Cita
        {
            PacienteId = 99,
            MedicoId = 1,
            FechaHora = fecha,
            DuracionMinutos = 30,
            Estado = EstadoCita.Cancelada // NO bloquea
        };
        var citaCreada = new Cita { Id = 10, PacienteId = 2, MedicoId = 1, Estado = EstadoCita.Pendiente };

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);
        _citaRepoMock.Setup(r => r.ObtenerPorMedicoYFechaAsync(1, DateOnly.FromDateTime(fecha)))
            .ReturnsAsync(new List<Cita> { citaCancelada });
        _citaRepoMock.Setup(r => r.ObtenerPorPacienteYFechaAsync(2, DateOnly.FromDateTime(fecha)))
            .ReturnsAsync(new List<Cita>());
        _citaRepoMock.Setup(r => r.AgregarAsync(It.IsAny<Cita>())).ReturnsAsync(citaCreada);

        // Act
        var resultado = await CrearServicio().AgendarCitaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        _citaRepoMock.Verify(r => r.AgregarAsync(It.IsAny<Cita>()), Times.Once);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static DateTime ObtenerProximoLunes(TimeOnly hora)
    {
        var hoy = DateTime.UtcNow.Date;
        var diasHastaLunes = ((int)DayOfWeek.Monday - (int)hoy.DayOfWeek + 7) % 7;
        if (diasHastaLunes == 0) diasHastaLunes = 7;
        return hoy.AddDays(diasHastaLunes).Add(hora.ToTimeSpan());
    }
}
