using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WariSalud.Core.Entities;

namespace WariSalud.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Especialidades
        if (!await context.Especialidades.AnyAsync())
        {
            var esp1 = new Especialidad { Nombre = "Cardiología", Descripcion = "Prevención y tratamiento de enfermedades cardiovasculares", DuracionCitaMinutos = 30 };
            var esp2 = new Especialidad { Nombre = "Pediatría", Descripcion = "Atención integral del niño y adolescente", DuracionCitaMinutos = 30 };
            var esp3 = new Especialidad { Nombre = "Medicina General", Descripcion = "Consulta preventiva y diagnóstico primario", DuracionCitaMinutos = 20 };
            var esp4 = new Especialidad { Nombre = "Dermatología", Descripcion = "Especialistas en salud de la piel, cabello y uñas", DuracionCitaMinutos = 30 };
            var esp5 = new Especialidad { Nombre = "Ginecología", Descripcion = "Cuidado integral de la salud del sistema reproductor femenino", DuracionCitaMinutos = 40 };

            context.Especialidades.AddRange(esp1, esp2, esp3, esp4, esp5);
            await context.SaveChangesAsync();
        }

        var especialidades = await context.Especialidades.ToListAsync();
        var card = especialidades.FirstOrDefault(e => e.Nombre == "Cardiología") ?? especialidades[0];
        var ped = especialidades.FirstOrDefault(e => e.Nombre == "Pediatría") ?? especialidades[0];
        var gen = especialidades.FirstOrDefault(e => e.Nombre == "Medicina General") ?? especialidades[0];
        var derm = especialidades.FirstOrDefault(e => e.Nombre == "Dermatología") ?? especialidades[0];

        // 2. Administrador
        if (!await context.Usuarios.AnyAsync(u => u.Email == "admin@warisalud.pe"))
        {
            var adminUser = new Usuario
            {
                Email = "admin@warisalud.pe",
                PasswordHash = HashPassword("admin123"),
                Rol = "Admin"
            };
            context.Usuarios.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // 3. Médicos (y sus usuarios + horarios)
        if (!await context.Medicos.AnyAsync())
        {
            var med1User = new Usuario { Email = "carlos.mendoza@warisalud.pe", PasswordHash = HashPassword("medico123"), Rol = "Medico" };
            var med2User = new Usuario { Email = "ana.huaman@warisalud.pe", PasswordHash = HashPassword("medico123"), Rol = "Medico" };
            var med3User = new Usuario { Email = "luis.quispe@warisalud.pe", PasswordHash = HashPassword("medico123"), Rol = "Medico" };
            var med4User = new Usuario { Email = "sofia.vargas@warisalud.pe", PasswordHash = HashPassword("medico123"), Rol = "Medico" };

            context.Usuarios.AddRange(med1User, med2User, med3User, med4User);
            await context.SaveChangesAsync();

            var med1 = new Medico { UsuarioId = med1User.Id, EspecialidadId = card.Id, NombreCompleto = "Dr. Carlos Mendoza", NumeroColegiatura = "CMP-12345", Activo = true };
            var med2 = new Medico { UsuarioId = med2User.Id, EspecialidadId = ped.Id, NombreCompleto = "Dra. Ana Huamán", NumeroColegiatura = "CMP-23456", Activo = true };
            var med3 = new Medico { UsuarioId = med3User.Id, EspecialidadId = gen.Id, NombreCompleto = "Dr. Luis Quispe", NumeroColegiatura = "CMP-34567", Activo = true };
            var med4 = new Medico { UsuarioId = med4User.Id, EspecialidadId = derm.Id, NombreCompleto = "Dra. Sofía Vargas", NumeroColegiatura = "CMP-45678", Activo = true };

            context.Medicos.AddRange(med1, med2, med3, med4);
            await context.SaveChangesAsync();

            // Horarios
            context.HorariosMedico.AddRange(
                new HorarioMedico { MedicoId = med1.Id, HoraInicio = new TimeOnly(8, 0), HoraFin = new TimeOnly(16, 0), DiasLaborables = "1,2,3,4,5" },
                new HorarioMedico { MedicoId = med2.Id, HoraInicio = new TimeOnly(9, 0), HoraFin = new TimeOnly(17, 0), DiasLaborables = "1,2,3,4,5,6" },
                new HorarioMedico { MedicoId = med3.Id, HoraInicio = new TimeOnly(8, 0), HoraFin = new TimeOnly(18, 0), DiasLaborables = "1,2,3,4,5,6" },
                new HorarioMedico { MedicoId = med4.Id, HoraInicio = new TimeOnly(10, 0), HoraFin = new TimeOnly(19, 0), DiasLaborables = "1,2,3,4,5" }
            );
            await context.SaveChangesAsync();
        }

        // 4. Pacientes (y sus usuarios)
        if (!await context.Usuarios.AnyAsync(u => u.Email == "maria.rojas@gmail.com"))
        {
            var pac1User = new Usuario { Email = "maria.rojas@gmail.com", PasswordHash = HashPassword("paciente123"), Rol = "Paciente" };
            var pac2User = new Usuario { Email = "jorge.torres@gmail.com", PasswordHash = HashPassword("paciente123"), Rol = "Paciente" };
            var pac3User = new Usuario { Email = "lucia.alarcon@gmail.com", PasswordHash = HashPassword("paciente123"), Rol = "Paciente" };
            var pac4User = new Usuario { Email = "carlos.ruiz@gmail.com", PasswordHash = HashPassword("paciente123"), Rol = "Paciente" };

            context.Usuarios.AddRange(pac1User, pac2User, pac3User, pac4User);
            await context.SaveChangesAsync();

            var pac1 = new Paciente { UsuarioId = pac1User.Id, NombreCompleto = "María Rojas", Telefono = "987654321" };
            var pac2 = new Paciente { UsuarioId = pac2User.Id, NombreCompleto = "Jorge Torres", Telefono = "912345678" };
            var pac3 = new Paciente { UsuarioId = pac3User.Id, NombreCompleto = "Lucía Alarcón", Telefono = "955667788" };
            var pac4 = new Paciente { UsuarioId = pac4User.Id, NombreCompleto = "Carlos Ruiz", Telefono = "944332211" };

            context.Pacientes.AddRange(pac1, pac2, pac3, pac4);
            await context.SaveChangesAsync();
        }

        // 5. Citas (Appointments)
        if (!await context.Citas.AnyAsync())
        {
            var medicos = await context.Medicos.Include(m => m.Especialidad).ToListAsync();
            var pacientes = await context.Pacientes.ToListAsync();

            if (medicos.Count >= 3 && pacientes.Count >= 3)
            {
                var medCard = medicos[0];
                var medPed = medicos[1];
                var medGen = medicos[2];

                var pac1 = pacientes[0];
                var pac2 = pacientes[1];
                var pac3 = pacientes[2];
                var pac4 = pacientes.Count > 3 ? pacientes[3] : pacientes[0];

                var hoy = DateTime.Today;

                // Citas pasadas
                context.Citas.AddRange(
                    new Cita { PacienteId = pac1.Id, MedicoId = medCard.Id, FechaHora = hoy.AddDays(-3).AddHours(10), DuracionMinutos = 30, Estado = EstadoCita.Completada, Motivo = "Control de presión arterial post-tratamiento" },
                    new Cita { PacienteId = pac2.Id, MedicoId = medPed.Id, FechaHora = hoy.AddDays(-5).AddHours(11), DuracionMinutos = 30, Estado = EstadoCita.Completada, Motivo = "Chequeo pediátrico anual" },
                    new Cita { PacienteId = pac3.Id, MedicoId = medGen.Id, FechaHora = hoy.AddDays(-2).AddHours(9), DuracionMinutos = 20, Estado = EstadoCita.Cancelada, Motivo = "Fiebre leve y dolor de garganta" }
                );

                // Citas futuras (Pendientes)
                context.Citas.AddRange(
                    new Cita { PacienteId = pac1.Id, MedicoId = medCard.Id, FechaHora = hoy.AddDays(1).AddHours(10), DuracionMinutos = 30, Estado = EstadoCita.Pendiente, Motivo = "Revisión de análisis cardiológicos" },
                    new Cita { PacienteId = pac2.Id, MedicoId = medGen.Id, FechaHora = hoy.AddDays(2).AddHours(15), DuracionMinutos = 20, Estado = EstadoCita.Pendiente, Motivo = "Dolor abdominal recurrente" },
                    new Cita { PacienteId = pac3.Id, MedicoId = medPed.Id, FechaHora = hoy.AddDays(3).AddHours(11).AddMinutes(30), DuracionMinutos = 30, Estado = EstadoCita.Pendiente, Motivo = "Consulta pediátrica por alergia cutánea" },
                    new Cita { PacienteId = pac4.Id, MedicoId = medCard.Id, FechaHora = hoy.AddDays(4).AddHours(9).AddMinutes(30), DuracionMinutos = 30, Estado = EstadoCita.Pendiente, Motivo = "Chequeo de rutina por antecedentes familiares" },
                    new Cita { PacienteId = pac1.Id, MedicoId = medGen.Id, FechaHora = hoy.AddDays(5).AddHours(16), DuracionMinutos = 20, Estado = EstadoCita.Pendiente, Motivo = "Certificado médico laboral" }
                );

                await context.SaveChangesAsync();
            }
        }

        // 6. Corregir usuarios creados desde Admin sin hash previo
        var unhashedUsers = await context.Usuarios.Where(u => u.PasswordHash == "admin_created_hash").ToListAsync();
        if (unhashedUsers.Count > 0)
        {
            foreach (var u in unhashedUsers)
            {
                u.PasswordHash = HashPassword("medico123");
            }
            await context.SaveChangesAsync();
        }
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
