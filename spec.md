# Especificación Funcional (SPEC)
## Sistema de Gestión de Citas Médicas — "WariSalud"

> Fase SDD: **Specify** — Este documento define QUÉ debe hacer el sistema y CON QUÉ REGLAS,
> sin entrar en decisiones de implementación técnica (eso vive en `plan.md`).

---

## 1. Visión y Objetivo

Backend transaccional para la gestión centralizada de citas médicas en un entorno clínico.
El proyecto sirve como demostración rigurosa de buenas prácticas de ingeniería de software:
arquitectura escalable, control de versiones, diseño orientado a pruebas (TDD/Unit Testing)
y cobertura de código medible.

**Criterio de éxito del proyecto:** API funcional, con ≥90% de cobertura en la capa de
dominio, que impide algorítmicamente dobles reservas, respeta horarios laborales y aplica
reglas de cancelación con antelación mínima.

---

## 2. Alcance

### 2.1 Incluido
- Gestión de identidades y perfiles: Paciente, Médico, Administrador.
- Gestión de Especialidades médicas y su asignación a médicos.
- Agendamiento de citas con validación de horario, detección de conflictos y control de cupos.
- Cancelación de citas sujeta a reglas de tiempo.
- Pruebas unitarias exhaustivas sobre la capa de dominio/negocio.

### 2.2 Excluido (fuera de alcance)
- Pasarelas de pago / facturación electrónica.
- Historial clínico electrónico completo (evoluciones, diagnósticos, recetas).
- Integración con APIs externas de mensajería (SMS/WhatsApp/Email).
- Frontend / UI (este proyecto es backend-only, expuesto vía API REST + Swagger).

---

## 3. Actores (Roles)

| Actor | Rol en el sistema | Permisos clave |
|---|---|---|
| **Paciente** | Usuario final | Buscar disponibilidad, reservar cita, cancelar sus propias citas |
| **Médico** | Profesional de salud | Consultar su agenda diaria/semanal |
| **Administrador/Recepcionista** | Personal de la clínica | Gestionar médicos, especialidades, horarios; auditar el sistema |

Todos los roles se autentican vía JWT. Un endpoint sensible siempre valida el rol del
llamante (p. ej. un Paciente NUNCA puede modificar el horario de un Médico).

---

## 4. Historias de Usuario y Criterios de Aceptación

### HU01 — Agendar Cita
**Como** Paciente, **quiero** agendar una cita con un médico en fecha/hora específica,
**para** ser atendido.

- **Dado** que consulto la disponibilidad de un médico, **entonces** el sistema muestra
  únicamente horarios libres dentro del horario laboral.
- **Dado** que el agendamiento es válido, **entonces** recibo confirmación y la cita queda
  en estado `Pendiente`.
- **Dado** que ya tengo 2 citas activas ese mismo día, **entonces** el sistema rechaza el
  agendamiento con un error explícito.

### HU02 — Prevención de Double-Booking
**Como** Sistema, **quiero** validar la disponibilidad del médico antes de persistir la
cita, **para** evitar que dos pacientes se agenden en el mismo bloque horario.

- **Dado** que un médico ya tiene una cita activa en un rango horario, **entonces** ninguna
  otra cita puede solaparse con ese rango (ni total ni parcialmente).
- Esta validación debe ser atómica/segura ante condiciones de concurrencia razonables.

### HU03 — Validación de Horario Laboral
**Como** Administrador, **quiero** que el sistema restrinja las reservas al horario
laboral, **para** evitar citas fuera de turno.

- **Dado** que la clínica opera de 08:00 a 20:00, **entonces** cualquier intento de agendar
  fuera de ese rango debe ser rechazado.

### HU04 — Cancelación de Cita
**Como** Paciente, **quiero** cancelar mi cita programada, **para** liberar el espacio si
no puedo asistir.

- **Dado** que faltan ≥24 horas para la cita, **entonces** la cancelación es exitosa y el
  estado cambia a `Cancelada`.
- **Dado** que faltan <24 horas para la cita, **entonces** el sistema rechaza la
  cancelación con un error explícito.

---

## 5. Requerimientos Funcionales (RF)

| ID | Descripción |
|---|---|
| RF01 | Registrar, actualizar y consultar Pacientes, Médicos y Especialidades. |
| RF02 | Un paciente autenticado puede agendar cita con un médico en fecha/hora disponible. |
| RF03 🔴 | Impedir algorítmicamente que un médico tenga dos citas superpuestas (Double-booking). |
| RF04 🔴 | Las citas solo pueden agendarse dentro del horario laboral/días laborables configurados. |
| RF05 🔴 | Un paciente no puede tener más de 2 citas activas en la misma fecha. |
| RF06 🔴 | Cancelación solo permitida con ≥24h de antelación. |

🔴 = Regla de negocio crítica (debe tener cobertura de test explícita, ver sección 7).

---

## 6. Requerimientos No Funcionales (RNF)

| ID | Categoría | Descripción |
|---|---|---|
| RNF01 | Calidad/Testing | Cobertura de código ≥90% en la lógica de negocio central (agendamiento, validaciones de tiempo/conflictos). |
| RNF02 | Arquitectura | Clean Architecture / Arquitectura en Capas (API, Core/Domain, Infrastructure). |
| RNF03 | Seguridad | Autenticación/autorización con JWT; todo endpoint sensible valida el rol del usuario. |
| RNF04 | Rendimiento | Consulta de disponibilidad de horarios: latencia <500ms con hasta 100 usuarios concurrentes. |
| RNF05 | Disponibilidad | 99.9% de disponibilidad en horario de operación (08:00–20:00); middleware global de manejo de excepciones (sin crashes). |

---

## 7. Reglas de Negocio Críticas y Casos Límite (Edge Cases)

Estos casos **deben** existir como pruebas unitarias explícitas (no opcionales):

1. Intentar agendar a las 20:00 (o después) cuando el cierre es 18:00/20:00 → debe fallar.
2. Intentar agendar la **cita número 3** para un mismo paciente en un mismo día → debe fallar.
3. Intentar cancelar una cita faltando **23 horas** (justo bajo el límite) → debe fallar.
4. Intentar cancelar una cita faltando exactamente 24 horas → debe tener éxito (caso límite
   inclusive, a definir en `plan.md` si es `>=` o `>`).
5. Dos solicitudes de agendamiento simultáneas para el mismo médico/horario → solo una debe
   tener éxito.
6. Agendar una cita para un médico inexistente o inactivo → debe fallar.
7. Un paciente intenta cancelar la cita de **otro** paciente → debe fallar por autorización (403).

---

## 8. Glosario de Estados

- **Cita.Estado**: `Pendiente` → `Completada` | `Cancelada`
- **Usuario.Rol**: `Paciente` | `Medico` | `Admin`

---

## 9. Decisiones de Diseño (ambigüedades resueltas)

> Estas ambigüedades existían en los documentos originales y fueron resueltas aquí para
> que la IA de coding NO tenga que asumir nada por su cuenta. Son decisiones **vinculantes**
> para `plan.md` y `tasks.md`.

### 9.1 Horario laboral: ¿clínica o por médico?

**Decisión:** modelo de dos niveles con resolución en cascada (patrón *fallback*), para
mantener el MVP simple sin sacrificar flexibilidad real de una clínica:

1. Existe **un único horario global de clínica** (`ConfiguracionClinica`): hora de
   apertura, hora de cierre y días laborables (ej. Lunes–Sábado, 08:00–20:00). Es la
   fuente de verdad por defecto y basta para el 100% de los médicos en el caso normal.
2. Un médico **puede opcionalmente** tener su propio `HorarioMedico` (mismo shape:
   horas + días) que **sobrescribe** el horario de clínica solo para ese médico (útil
   para especialistas que atienden medio turno, por ejemplo).
3. Regla de resolución: `HorarioEfectivo(medico) = HorarioMedico(medico) ?? HorarioClinica`.

Esto evita overengineering (no hay horario "por especialidad" ni "por día distinto cada
semana") y resuelve exactamente la ambigüedad del documento original ("horario... para
cada médico y para la clínica") sin duplicar configuración en el caso común.

### 9.2 Duración de una cita

**Decisión:** la duración es una propiedad de la **Especialidad**, no del médico ni de la
cita en sí — es la opción más realista clínicamente (una consulta de Cardiología dura
distinto que un control de Medicina General) y la más simple de mantener (un solo lugar
de configuración por especialidad, no por médico individual).

- `Especialidad.DuracionCitaMinutos` (int, ej. Medicina General = 20, default general si
  no se especifica = 30).
- Al crear la `Cita`, el sistema copia (snapshot) esa duración en
  `Cita.DuracionMinutos`, para que un cambio futuro en `Especialidad` no altere citas ya
  agendadas.

### 9.3 Caso límite de cancelación a las 24h exactas

**Decisión:** el umbral es **inclusivo** (`>= 24 horas` desde "ahora" hasta `FechaHora` de
la cita permite cancelar). Es la interpretación más favorable y predecible para el
paciente, y evita ambigüedad de fracciones de segundo en el borde.

### 9.4 Citas superpuestas del mismo paciente (no solo con el mismo médico)

**Decisión:** se trata igual que RF05 (máximo 2 citas activas por día) — no se agrega una
regla extra de "solapamiento propio", ya que RF05 ya cubre el caso de forma suficiente
para el alcance del proyecto y evita una validación redundante.
