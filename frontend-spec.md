# Especificación de Frontend (SPEC — Frontend)
## WariSalud — React + TypeScript

> Complementa `spec.md` y `plan.md` (backend). Este documento define las pantallas, rutas
> y su mapeo a los endpoints ya definidos en `plan.md` sección 4. No introduce nuevas
> reglas de negocio — el frontend consume el contrato existente, no lo reinterpreta.

---

## 1. Stack Propuesto

- React 18 + TypeScript + Vite
- React Router (rutas protegidas por rol)
- Tailwind CSS (estilos)
- Axios (o fetch) con interceptor para adjuntar el JWT y manejar 401 globalmente
- Gestión de estado de servidor: React Query / TanStack Query (cache de disponibilidad,
  citas, etc.)
- Librería de formularios: React Hook Form + Zod (validación en cliente, espejo de las
  reglas de `spec.md` sección 5/7, sin reemplazar la validación del backend)

---

## 2. Mapa de Rutas por Rol

| Ruta | Pantalla | Rol | Endpoint(s) principales |
|---|---|---|---|
| `/login` | Login | Público | `POST /api/auth/login` |
| `/registro` | Registro | Público | `POST /api/auth/register` |
| `/` | Inicio (redirige según rol) | Autenticado | — |
| `/paciente/inicio` | Próximas citas + accesos rápidos | Paciente | `GET /api/citas/mias` |
| `/paciente/buscar` | Buscar médico/especialidad | Paciente | `GET /api/especialidades`, `GET /api/medicos` |
| `/paciente/medicos/:id/disponibilidad` | Horarios libres del médico | Paciente | `GET /api/medicos/{id}/disponibilidad?fecha=` |
| `/paciente/agendar` | Formulario de agendamiento + confirmación | Paciente | `POST /api/citas` |
| `/paciente/citas` | Mis citas (lista + filtro por estado) | Paciente | `GET /api/citas/mias` |
| `/paciente/citas/:id` | Detalle / Cancelar cita | Paciente | `DELETE /api/citas/{id}` |
| `/paciente/perfil` | Mi perfil | Paciente | (endpoint de perfil, agregar si no existe) |
| `/medico/agenda` | Agenda del día / semanal | Medico | `GET /api/medicos/{id}/agenda?fecha=` |
| `/medico/citas/:id` | Detalle de cita del paciente | Medico | (mismo endpoint de agenda, detalle embebido) |
| `/medico/horario` | Ver mi horario efectivo | Medico | (endpoint de horario, agregar si no existe) |
| `/admin/dashboard` | Métricas generales | Admin | Agregado — ver sección 5 |
| `/admin/medicos` | CRUD Médicos | Admin | `GET/POST /api/medicos` (+ PUT/DELETE, agregar si no existen) |
| `/admin/especialidades` | CRUD Especialidades | Admin | `GET/POST /api/especialidades` |
| `/admin/configuracion-clinica` | Horario global de la clínica | Admin | Agregado — ver sección 5 |

---

## 3. Componentes Transversales

- **AppLayout**: navbar/sidebar cuyo menú cambia según el rol decodificado del JWT.
- **ProtectedRoute**: wrapper de rutas que valida rol antes de renderizar (espeja RNF03).
- **ToastProvider**: traduce los códigos HTTP definidos en `plan.md` sección 4
  (`409`, `422`, `403`, `401`) a mensajes claros para el usuario:
  - `409` → "Ese horario ya no está disponible" / "Ya tienes el máximo de citas ese día".
  - `422` → "Fuera del horario de atención" / "No se puede cancelar con menos de 24h".
  - `403` → "No tienes permiso para esta acción".
  - `401` → redirige a `/login`.
- **ErrorPage** (`/403`, `/404`) y **NotFound** catch-all.

---

## 4. Validaciones de Cliente (espejo, no reemplazo, de las reglas de negocio)

Para dar feedback inmediato sin esperar al backend, replicar en el formulario de
agendar cita:
- No permitir seleccionar horarios fuera del rango devuelto por
  `/disponibilidad` (el propio endpoint ya filtra, así que el form solo debe
  deshabilitar lo no listado).
- Mostrar contador "X de 2 citas usadas hoy" en el formulario de agendar (RF05),
  alimentado por `GET /api/citas/mias`.
- En cancelación, deshabilitar el botón "Cancelar" si `FechaHora - ahora < 24h` y
  mostrar el motivo (RF06 / decisión 9.3 de `spec.md`).

> **Importante:** estas validaciones de cliente son solo UX — la fuente de verdad sigue
> siendo el backend. No omitir las validaciones de servidor por esto.

---

## 5. Gaps a resolver antes/durante el desarrollo del frontend

El backend actual (`plan.md`) no expone aún estos endpoints que el frontend necesita.
Se recomienda agregarlos como una mini-fase adicional en `tasks.md` (Fase 4b):

- `GET /api/citas/mias` — filtrar por estado (query param `estado=`).
- `PUT/DELETE /api/medicos/{id}` y `PUT/DELETE /api/especialidades/{id}` (CRUD completo,
  hoy solo hay GET/POST).
- `GET/PUT /api/configuracion-clinica` (Admin) — horario global.
- `GET/PUT /api/medicos/{id}/horario` — `HorarioMedico` (override opcional).
- `GET /api/usuarios/me` — perfil del usuario autenticado (para pantalla de perfil).

---

## 6. Prioridad de Implementación (MVP del frontend)

1. Login/Registro + layout con rutas protegidas.
2. Flujo completo de Paciente (buscar → disponibilidad → agendar → mis citas → cancelar).
3. Flujo de Médico (agenda).
4. Flujo de Admin (CRUD médicos/especialidades + configuración de clínica).

Esto permite tener una demo end-to-end funcional (RF02-RF06) antes de completar las
pantallas administrativas menos críticas para la evaluación de las reglas de negocio.
