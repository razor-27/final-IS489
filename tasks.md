# Plan de Tareas de Implementación (TASKS)
## Sistema de Gestión de Citas Médicas — "WariSalud"

> Fase SDD: **Tasks** — Lista ordenada y accionable para que una IA de coding (o un
> desarrollador) implemente el sistema paso a paso, en base a `spec.md` (qué) y `plan.md`
> (cómo). Cada tarea indica su objetivo, criterio de "hecho" (Definition of Done) y, cuando
> aplica, los tests que debe llevar asociados.
>
> **Instrucción para la IA de coding:** completa las tareas en orden. No pases a la
> siguiente hasta que la actual compile y sus tests (si los tiene) pasen. Si una decisión
> no está clara, revisa la sección "Preguntas Abiertas" de `spec.md` antes de asumir algo.

---

## Fase 0 — Setup del Proyecto

- [ ] **T0.1** Crear solución `WariSalud.sln` con los 4 proyectos definidos en `plan.md`
      sección 2 (Core, Infrastructure, API, Tests) y las referencias correctas entre ellos.
- [ ] **T0.2** Configurar `WariSalud.Tests` con xUnit + FluentAssertions + Moq (paquetes
      NuGet) y verificar que corre un test dummy.
- [ ] **T0.3** Configurar `appsettings.json` con connection string a SQL Server
      (usar `appsettings.Development.json` para credenciales locales, no versionarlo).
- [ ] **T0.4** Configurar Swagger/OpenAPI en `Program.cs`.

**DoD:** `dotnet build` y `dotnet test` corren sin errores sobre la solución vacía.

---

## Fase 1 — Dominio (WariSalud.Core)

- [ ] **T1.1** Crear entidades: `Usuario`, `Paciente`, `Medico`, `Especialidad`, `Cita`,
      `ConfiguracionClinica`, `HorarioMedico` según el modelo de `plan.md` sección 3.
      Incluir `Especialidad.DuracionCitaMinutos` y `Cita.DuracionMinutos` (snapshot).
- [ ] **T1.1b** Implementar el método de dominio `HorarioEfectivo(medico)` que resuelve en
      cascada: `HorarioMedico(medico) ?? ConfiguracionClinica` (decisión 9.1 de `spec.md`).
      Este método es usado por T2.1 y T2.3.
- [ ] **T1.2** Crear excepciones de dominio: `DoubleBookingException`,
      `FueraDeHorarioException`, `LimiteDeCitasException`,
      `CancelacionFueraDePlazoException`, `RecursoNoEncontradoException`,
      `AccesoNoAutorizadoException`.
- [ ] **T1.3** Definir interfaces de repositorio en `Core/Interfaces`:
      `IPacienteRepository`, `IMedicoRepository`, `ICitaRepository`,
      `IEspecialidadRepository`.
- [ ] **T1.4** Definir interfaz `ICitaService` con métodos:
      `AgendarCitaAsync(...)`, `CancelarCitaAsync(...)`, `ObtenerDisponibilidadAsync(...)`.

**DoD:** Core compila sin dependencias externas (solo librerías estándar de .NET).

---

## Fase 2 — Lógica de Negocio + Tests (el corazón del proyecto — RNF01)

> Implementar con TDD: escribir el test primero, luego el código que lo hace pasar.

- [ ] **T2.1** Implementar `CitaService.AgendarCitaAsync`:
  - Valida horario laboral (RF04).
  - Valida que no haya solapamiento con otra cita del mismo médico (RF03).
  - Valida que el paciente no tenga ya 2 citas activas ese día (RF05).
  - **Tests obligatorios** (usar Moq para simular `ICitaRepository`/`IMedicoRepository`):
    - ✅ Agendar en horario válido y disponible → éxito.
    - ❌ Agendar a las 20:00+ con cierre a las 18:00/20:00 → `FueraDeHorarioException`.
    - ❌ Agendar 3ª cita del día para el mismo paciente → `LimiteDeCitasException`.
    - ❌ Agendar sobre horario ya ocupado del médico → `DoubleBookingException`.
    - ❌ Agendar con médico inexistente/inactivo → `RecursoNoEncontradoException`.

- [ ] **T2.2** Implementar `CitaService.CancelarCitaAsync`:
  - Valida que quien cancela sea el dueño de la cita (RNF03/autorización).
  - Valida antelación mínima de 24h (RF06).
  - **Tests obligatorios:**
    - ✅ Cancelar con exactamente 24h de antelación (umbral inclusivo `>=`) → éxito,
      `Estado = "Cancelada"`.
    - ❌ Cancelar con 23h de antelación → `CancelacionFueraDePlazoException`.
    - ❌ Cancelar cita de otro paciente → `AccesoNoAutorizadoException`.

- [ ] **T2.3** Implementar `CitaService.ObtenerDisponibilidadAsync`:
  - Devuelve solo bloques libres dentro del horario laboral del médico/clínica.
  - **Test obligatorio:** dado un médico con 2 citas en el día, la disponibilidad no debe
    incluir esos bloques.

**DoD:** Cobertura de `WariSalud.Core` (medida con Test Explorer o
`dotnet test --collect:"XPlat Code Coverage"`) ≥90%. Todos los tests de la Fase 2 en verde.

---

## Fase 3 — Infraestructura (WariSalud.Infrastructure)

- [ ] **T3.1** Crear `ApplicationDbContext` (EF Core) con `DbSet` para cada entidad y
      configuración Fluent API de relaciones (según diagrama ER de `plan.md`).
- [ ] **T3.2** Generar migración inicial Code-First y aplicarla a SQL Server local.
- [ ] **T3.3** Implementar los repositorios concretos (`PacienteRepository`,
      `MedicoRepository`, `CitaRepository`, `EspecialidadRepository`) implementando las
      interfaces de Core.
- [ ] **T3.4** Añadir índice en `CITA(MedicoId, FechaHora)` (soporte de RNF04).

**DoD:** `dotnet ef database update` crea el esquema correctamente; repositorios cubren
las operaciones CRUD necesarias por `ICitaService`.

---

## Fase 4 — API (WariSalud.API)

- [ ] **T4.1** Configurar ASP.NET Core Identity + JWT Bearer Authentication (RNF03).
- [ ] **T4.2** Implementar `AuthController` (`/register`, `/login`).
- [ ] **T4.3** Implementar `EspecialidadesController` y `MedicosController` según
      contratos de `plan.md` sección 4.
- [ ] **T4.4** Implementar `CitasController` (`POST /api/citas`, `DELETE /api/citas/{id}`,
      `GET /api/citas/mias`), conectando con `ICitaService` vía DI.
- [ ] **T4.5** Implementar `GlobalExceptionHandlingMiddleware` que traduce cada excepción
      de dominio (Fase 1) al código HTTP correspondiente (tabla en `plan.md` sección 4).
- [ ] **T4.6** Aplicar `[Authorize(Roles = "...")]` en cada endpoint según la tabla de
      contratos.
- [ ] **T4.7** Verificar Swagger expone todos los endpoints con sus esquemas de
      request/response.

**DoD:** Se puede hacer login, agendar y cancelar una cita end-to-end vía Swagger/Postman
contra SQL Server real, respetando roles.

---

## Fase 5 — Validación Final

- [ ] **T5.1** Correr suite completa de tests + reporte de cobertura; confirmar ≥90% en Core.
- [ ] **T5.2** Prueba manual de los 7 casos límite de `spec.md` sección 7 vía Swagger.
- [ ] **T5.3** Prueba de carga básica (100 usuarios concurrentes) sobre el endpoint de
      disponibilidad, confirmar <500ms (RNF04) — herramienta sugerida: `k6` o `Apache Bench`.
- [ ] **T5.4** Revisar que ninguna excepción no controlada devuelva un 500 sin manejar
      (RNF05) — probar con inputs malformados.

**DoD del proyecto completo:** Todos los checkboxes marcados, tests en verde, cobertura
≥90%, y los 7 casos límite verificados manualmente.
