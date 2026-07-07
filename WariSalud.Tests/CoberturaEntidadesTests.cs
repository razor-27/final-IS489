using FluentAssertions;
using WariSalud.Core.Entities;
using WariSalud.Core.Exceptions;

namespace WariSalud.Tests;

/// <summary>
/// Tests de cobertura para entidades simples y excepciones de dominio.
/// Garantizan que la cobertura de WariSalud.Core alcanza ≥90% (RNF01).
/// </summary>
public class CoberturaEntidadesTests
{
    // ─── Entidades básicas ────────────────────────────────────────────────────

    [Fact]
    public void Usuario_Creacion_PropiedadesDefecto()
    {
        var usuario = new Usuario
        {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = "hash",
            Rol = "Paciente"
        };

        usuario.Id.Should().Be(1);
        usuario.Email.Should().Be("test@test.com");
        usuario.Rol.Should().Be("Paciente");
        usuario.Paciente.Should().BeNull();
        usuario.Medico.Should().BeNull();
    }

    [Fact]
    public void Paciente_Creacion_PropiedadesDefecto()
    {
        var paciente = new Paciente
        {
            Id = 1,
            UsuarioId = 2,
            NombreCompleto = "Juan Pérez",
            Telefono = "999888777"
        };

        paciente.Id.Should().Be(1);
        paciente.NombreCompleto.Should().Be("Juan Pérez");
        paciente.Citas.Should().BeEmpty();
        paciente.Usuario.Should().BeNull();
    }

    [Fact]
    public void Especialidad_Creacion_DuracionDefault()
    {
        var especialidad = new Especialidad();
        especialidad.DuracionCitaMinutos.Should().Be(30);
        especialidad.Medicos.Should().BeEmpty();
    }

    [Fact]
    public void HorarioMedico_Creacion_DiasLaborablesDefault()
    {
        var horario = new HorarioMedico
        {
            Id = 1,
            MedicoId = 5,
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(17, 0)
        };

        horario.DiasLaborables.Should().Be("1,2,3,4,5");
        horario.Medico.Should().BeNull();
    }

    [Fact]
    public void ConfiguracionClinica_Creacion_ValoresDefecto()
    {
        var config = new ConfiguracionClinica();
        config.HoraApertura.Should().Be(new TimeOnly(8, 0));
        config.HoraCierre.Should().Be(new TimeOnly(20, 0));
        config.DiasLaborables.Should().Be("1,2,3,4,5,6");
    }

    [Fact]
    public void Cita_FechaHoraFin_CalculaCorrectamente()
    {
        var cita = new Cita
        {
            FechaHora = new DateTime(2026, 8, 1, 10, 0, 0),
            DuracionMinutos = 45
        };

        cita.FechaHoraFin.Should().Be(new DateTime(2026, 8, 1, 10, 45, 0));
    }

    [Fact]
    public void Cita_EstaActiva_SoloPendiente()
    {
        var pendiente = new Cita { Estado = EstadoCita.Pendiente };
        var completada = new Cita { Estado = EstadoCita.Completada };
        var cancelada = new Cita { Estado = EstadoCita.Cancelada };

        pendiente.EstaActiva.Should().BeTrue();
        completada.EstaActiva.Should().BeFalse();
        cancelada.EstaActiva.Should().BeFalse();
    }

    [Fact]
    public void Medico_HorarioEfectivo_SinOverride_UsaClinica()
    {
        var medico = new Medico { HorarioMedico = null };
        var config = new ConfiguracionClinica
        {
            HoraApertura = new TimeOnly(8, 0),
            HoraCierre = new TimeOnly(18, 0),
            DiasLaborables = "1,2,3,4,5"
        };

        var (inicio, fin, dias) = medico.ObtenerHorarioEfectivo(config);

        inicio.Should().Be(new TimeOnly(8, 0));
        fin.Should().Be(new TimeOnly(18, 0));
        dias.Should().Be("1,2,3,4,5");
    }

    [Fact]
    public void Medico_HorarioEfectivo_ConOverride_UsaMedico()
    {
        var medico = new Medico
        {
            HorarioMedico = new HorarioMedico
            {
                HoraInicio = new TimeOnly(9, 0),
                HoraFin = new TimeOnly(13, 0),
                DiasLaborables = "1,2,3"
            }
        };
        var config = new ConfiguracionClinica();

        var (inicio, fin, dias) = medico.ObtenerHorarioEfectivo(config);

        inicio.Should().Be(new TimeOnly(9, 0));
        fin.Should().Be(new TimeOnly(13, 0));
        dias.Should().Be("1,2,3");
    }

    // ─── Excepciones de dominio — ambos constructores ─────────────────────────

    [Fact]
    public void DoubleBookingException_MensajePredeterminado()
    {
        var ex1 = new DoubleBookingException();
        var ex2 = new DoubleBookingException("Custom msg");

        ex1.Message.Should().Contain("médico");
        ex2.Message.Should().Be("Custom msg");
    }

    [Fact]
    public void FueraDeHorarioException_MensajePredeterminado()
    {
        var ex1 = new FueraDeHorarioException();
        var ex2 = new FueraDeHorarioException("Fuera");

        ex1.Message.Should().Contain("horario");
        ex2.Message.Should().Be("Fuera");
    }

    [Fact]
    public void LimiteDeCitasException_MensajePredeterminado()
    {
        var ex1 = new LimiteDeCitasException();
        var ex2 = new LimiteDeCitasException("Límite");

        ex1.Message.Should().Contain("máximo");
        ex2.Message.Should().Be("Límite");
    }

    [Fact]
    public void CancelacionFueraDePlazoException_MensajePredeterminado()
    {
        var ex1 = new CancelacionFueraDePlazoException();
        var ex2 = new CancelacionFueraDePlazoException("Cancelar");

        ex1.Message.Should().Contain("24 horas");
        ex2.Message.Should().Be("Cancelar");
    }

    [Fact]
    public void RecursoNoEncontradoException_MensajePredeterminado()
    {
        var ex1 = new RecursoNoEncontradoException("Cita", 5);
        var ex2 = new RecursoNoEncontradoException("Msg directo");

        ex1.Message.Should().Contain("Cita");
        ex2.Message.Should().Be("Msg directo");
    }

    [Fact]
    public void AccesoNoAutorizadoException_MensajePredeterminado()
    {
        var ex1 = new AccesoNoAutorizadoException();
        var ex2 = new AccesoNoAutorizadoException("Sin acceso");

        ex1.Message.Should().Contain("permiso");
        ex2.Message.Should().Be("Sin acceso");
    }
}
