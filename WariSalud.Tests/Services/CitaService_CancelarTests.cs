using FluentAssertions;
using Moq;
using WariSalud.Core.Entities;
using WariSalud.Core.Exceptions;
using WariSalud.Core.Interfaces;
using WariSalud.Core.Services;

namespace WariSalud.Tests.Services;

/// <summary>
/// Tests unitarios para CitaService.CancelarCitaAsync (T2.2).
/// Cubre los 3 escenarios obligatorios de spec.md §7.
/// </summary>
public class CitaService_CancelarTests
{
    private readonly Mock<ICitaRepository> _citaRepoMock = new();
    private readonly Mock<IMedicoRepository> _medicoRepoMock = new();
    private readonly Mock<IConfiguracionClinicaRepository> _configRepoMock = new();

    private CitaService CrearServicio() =>
        new CitaService(_citaRepoMock.Object, _medicoRepoMock.Object, _configRepoMock.Object);

    // ─── T2.2 — Test 1: Cancelar con exactamente 24h de antelación → éxito
    // spec.md §7 caso 4 / §9.3: umbral inclusivo >=

    [Fact]
    public async Task CancelarCita_Exactamente24HorasAntelacion_DebePermitirCancelacion()
    {
        // Arrange
        var ahora = DateTime.UtcNow;
        var fechaCita = ahora.AddHours(24).AddSeconds(5); // umbral inclusivo + pequeño margen anti-flakiness

        var cita = new Cita
        {
            Id = 1,
            PacienteId = 5,
            MedicoId = 1,
            FechaHora = fechaCita,
            DuracionMinutos = 30,
            Estado = EstadoCita.Pendiente
        };

        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(cita);
        _citaRepoMock.Setup(r => r.ActualizarAsync(It.IsAny<Cita>())).Returns(Task.CompletedTask);

        // Act
        await CrearServicio().CancelarCitaAsync(citaId: 1, pacienteIdSolicitante: 5);

        // Assert
        cita.Estado.Should().Be(EstadoCita.Cancelada);
        _citaRepoMock.Verify(r => r.ActualizarAsync(It.Is<Cita>(c => c.Estado == EstadoCita.Cancelada)), Times.Once);
    }

    // ─── T2.2 — Test 2: Cancelar con 23h de antelación → CancelacionFueraDePlazoException
    // spec.md §7 caso 3

    [Fact]
    public async Task CancelarCita_Con23HorasAntelacion_DebeLanzarCancelacionFueraDePlazo()
    {
        // Arrange
        var ahora = DateTime.UtcNow;
        var fechaCita = ahora.AddHours(23); // justo bajo el límite

        var cita = new Cita
        {
            Id = 2,
            PacienteId = 5,
            MedicoId = 1,
            FechaHora = fechaCita,
            DuracionMinutos = 30,
            Estado = EstadoCita.Pendiente
        };

        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(2)).ReturnsAsync(cita);

        // Act
        var act = async () => await CrearServicio().CancelarCitaAsync(citaId: 2, pacienteIdSolicitante: 5);

        // Assert
        await act.Should().ThrowAsync<CancelacionFueraDePlazoException>();
        _citaRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Cita>()), Times.Never);
    }

    // ─── T2.2 — Test 3: Cancelar cita de otro paciente → AccesoNoAutorizadoException
    // spec.md §7 caso 7

    [Fact]
    public async Task CancelarCita_PacienteDistintoAlDueno_DebeLanzarAccesoNoAutorizado()
    {
        // Arrange
        var cita = new Cita
        {
            Id = 3,
            PacienteId = 10,   // dueño real
            MedicoId = 1,
            FechaHora = DateTime.UtcNow.AddDays(5),
            DuracionMinutos = 30,
            Estado = EstadoCita.Pendiente
        };

        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(cita);

        // Act — paciente 99 intenta cancelar la cita de paciente 10
        var act = async () => await CrearServicio().CancelarCitaAsync(citaId: 3, pacienteIdSolicitante: 99);

        // Assert
        await act.Should().ThrowAsync<AccesoNoAutorizadoException>();
        _citaRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Cita>()), Times.Never);
    }

    // ─── Test adicional: Cita inexistente → RecursoNoEncontradoException ─────

    [Fact]
    public async Task CancelarCita_CitaInexistente_DebeLanzarRecursoNoEncontrado()
    {
        // Arrange
        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Cita?)null);

        // Act
        var act = async () => await CrearServicio().CancelarCitaAsync(citaId: 999, pacienteIdSolicitante: 1);

        // Assert
        await act.Should().ThrowAsync<RecursoNoEncontradoException>();
    }

    // ─── Test adicional: Cancelar con más de 24h → éxito ─────────────────────

    [Fact]
    public async Task CancelarCita_Con48HorasAntelacion_DebePermitirCancelacion()
    {
        // Arrange
        var cita = new Cita
        {
            Id = 4,
            PacienteId = 7,
            MedicoId = 1,
            FechaHora = DateTime.UtcNow.AddHours(48),
            DuracionMinutos = 30,
            Estado = EstadoCita.Pendiente
        };

        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(4)).ReturnsAsync(cita);
        _citaRepoMock.Setup(r => r.ActualizarAsync(It.IsAny<Cita>())).Returns(Task.CompletedTask);

        // Act
        await CrearServicio().CancelarCitaAsync(citaId: 4, pacienteIdSolicitante: 7);

        // Assert
        cita.Estado.Should().Be(EstadoCita.Cancelada);
    }

    // ─── Test adicional: Cancelar con exactamente 23h59m → debe fallar ───────

    [Fact]
    public async Task CancelarCita_Con23h59mAntelacion_DebeLanzarCancelacionFueraDePlazo()
    {
        // Arrange
        var cita = new Cita
        {
            Id = 5,
            PacienteId = 3,
            MedicoId = 1,
            FechaHora = DateTime.UtcNow.AddHours(23).AddMinutes(59),
            DuracionMinutos = 30,
            Estado = EstadoCita.Pendiente
        };

        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(cita);

        // Act
        var act = async () => await CrearServicio().CancelarCitaAsync(citaId: 5, pacienteIdSolicitante: 3);

        // Assert
        await act.Should().ThrowAsync<CancelacionFueraDePlazoException>();
    }
}
