using FluentAssertions;
using Moq;
using WariSalud.Core.Entities;
using WariSalud.Core.Interfaces;
using WariSalud.Core.Services;

namespace WariSalud.Tests.Services;

/// <summary>
/// Tests unitarios para CitaService.ObtenerDisponibilidadAsync (T2.3).
/// </summary>
public class CitaService_DisponibilidadTests
{
    private readonly Mock<ICitaRepository> _citaRepoMock = new();
    private readonly Mock<IMedicoRepository> _medicoRepoMock = new();
    private readonly Mock<IConfiguracionClinicaRepository> _configRepoMock = new();

    private CitaService CrearServicio() =>
        new CitaService(_citaRepoMock.Object, _medicoRepoMock.Object, _configRepoMock.Object);

    private static ConfiguracionClinica ConfigClinica() => new()
    {
        Id = 1,
        HoraApertura = new TimeOnly(8, 0),
        HoraCierre = new TimeOnly(10, 0),  // ventana pequeña para test manejable: 08:00–10:00
        DiasLaborables = "1,2,3,4,5,6"
    };

    private static Medico MedicoConEspecialidad(int duracionMin = 30) => new()
    {
        Id = 1,
        Activo = true,
        Especialidad = new Especialidad { DuracionCitaMinutos = duracionMin },
        HorarioMedico = null
    };

    // ─── T2.3 — Test 1: Médico con 2 citas — disponibilidad no incluye esos bloques

    [Fact]
    public async Task ObtenerDisponibilidad_MedicoConDosCitas_NoDebeIncluirBloquesCitados()
    {
        // Arrange
        var medico = MedicoConEspecialidad(duracionMin: 30);
        var config = ConfigClinica(); // 08:00–10:00, bloques: 08:00, 08:30, 09:00, 09:30
        var fecha = ObtenerProximoLunes();

        // El médico tiene citas a las 08:00 y 09:00
        var citas = new List<Cita>
        {
            new() { PacienteId = 1, MedicoId = 1, FechaHora = fecha.ToDateTime(new TimeOnly(8, 0)), DuracionMinutos = 30, Estado = EstadoCita.Pendiente },
            new() { PacienteId = 2, MedicoId = 1, FechaHora = fecha.ToDateTime(new TimeOnly(9, 0)), DuracionMinutos = 30, Estado = EstadoCita.Pendiente }
        };

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);
        _citaRepoMock.Setup(r => r.ObtenerPorMedicoYFechaAsync(1, fecha)).ReturnsAsync(citas);

        // Act
        var disponibilidad = (await CrearServicio().ObtenerDisponibilidadAsync(1, fecha)).ToList();

        // Assert
        disponibilidad.Should().HaveCount(2); // 08:30 y 09:30 libres
        disponibilidad.Should().NotContain(b => b.Inicio.TimeOfDay == new TimeSpan(8, 0, 0));
        disponibilidad.Should().NotContain(b => b.Inicio.TimeOfDay == new TimeSpan(9, 0, 0));
        disponibilidad.Should().Contain(b => b.Inicio.TimeOfDay == new TimeSpan(8, 30, 0));
        disponibilidad.Should().Contain(b => b.Inicio.TimeOfDay == new TimeSpan(9, 30, 0));
    }

    // ─── Test adicional: Sin citas → todos los bloques disponibles ───────────

    [Fact]
    public async Task ObtenerDisponibilidad_SinCitas_DebeRetornarTodosLosBloques()
    {
        // Arrange
        var medico = MedicoConEspecialidad(30);
        var config = ConfigClinica(); // 08:00–10:00 → 4 bloques de 30 min
        var fecha = ObtenerProximoLunes();

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);
        _citaRepoMock.Setup(r => r.ObtenerPorMedicoYFechaAsync(1, fecha)).ReturnsAsync(new List<Cita>());

        // Act
        var disponibilidad = (await CrearServicio().ObtenerDisponibilidadAsync(1, fecha)).ToList();

        // Assert — 08:00, 08:30, 09:00, 09:30
        disponibilidad.Should().HaveCount(4);
    }

    // ─── Test adicional: Día no laborable → lista vacía ──────────────────────

    [Fact]
    public async Task ObtenerDisponibilidad_DiaDomingo_DebeRetornarListaVacia()
    {
        // Arrange
        var medico = MedicoConEspecialidad(30);
        var config = ConfigClinica();

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var diasHastaDomingo = ((int)DayOfWeek.Sunday - (int)hoy.DayOfWeek + 7) % 7;
        if (diasHastaDomingo == 0) diasHastaDomingo = 7;
        var domingo = hoy.AddDays(diasHastaDomingo);

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);

        // Act
        var disponibilidad = await CrearServicio().ObtenerDisponibilidadAsync(1, domingo);

        // Assert
        disponibilidad.Should().BeEmpty();
    }

    // ─── Test adicional: Médico con horario personalizado (override spec §9.1)

    [Fact]
    public async Task ObtenerDisponibilidad_MedicoConHorarioPersonalizado_UsaHorarioDelMedico()
    {
        // Arrange — médico con horario 09:00–10:00 (solo 2 bloques de 30 min)
        var medico = new Medico
        {
            Id = 1,
            Activo = true,
            Especialidad = new Especialidad { DuracionCitaMinutos = 30 },
            HorarioMedico = new HorarioMedico
            {
                HoraInicio = new TimeOnly(9, 0),
                HoraFin = new TimeOnly(10, 0),
                DiasLaborables = "1,2,3,4,5,6"
            }
        };
        var config = ConfigClinica(); // 08:00–10:00 — debería ser ignorado
        var fecha = ObtenerProximoLunes();

        _medicoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(medico);
        _configRepoMock.Setup(r => r.ObtenerConfiguracionAsync()).ReturnsAsync(config);
        _citaRepoMock.Setup(r => r.ObtenerPorMedicoYFechaAsync(1, fecha)).ReturnsAsync(new List<Cita>());

        // Act
        var disponibilidad = (await CrearServicio().ObtenerDisponibilidadAsync(1, fecha)).ToList();

        // Assert — solo bloques dentro de 09:00–10:00
        disponibilidad.Should().HaveCount(2);
        disponibilidad.Should().AllSatisfy(b => b.Inicio.Hour.Should().BeGreaterThanOrEqualTo(9));
        disponibilidad.Should().NotContain(b => b.Inicio.Hour < 9);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static DateOnly ObtenerProximoLunes()
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var diasHastaLunes = ((int)DayOfWeek.Monday - (int)hoy.DayOfWeek + 7) % 7;
        if (diasHastaLunes == 0) diasHastaLunes = 7;
        return hoy.AddDays(diasHastaLunes);
    }
}
