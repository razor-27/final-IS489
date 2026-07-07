# WariSalud Frontend

> Frontend de WariSalud — Sistema de Gestión de Citas Médicas.
> Stack: React 18 + TypeScript + Vite + React Router + TanStack Query + React Hook Form + Zod + Axios + Lucide React

## Requisitos
- Node.js 18+
- Backend de WariSalud corriendo en `https://localhost:7001` (o configura `VITE_API_URL`)

## Instalación y desarrollo

```bash
npm install
npm run dev
```

## Variables de entorno

Crea un archivo `.env.local` si el backend corre en otra URL:

```
VITE_API_URL=https://localhost:7001
```

## Estructura

```
src/
├── types/         # Tipos de dominio TypeScript
├── lib/           # api.ts (Axios), auth.ts (JWT), utils.ts
├── context/       # AuthContext, ToastContext
├── hooks/         # useQueryWithToast
├── components/    # ProtectedRoute, AppLayout, ErrorPages
└── pages/
    ├── auth/      # Login, Register
    ├── paciente/  # Inicio, Buscar, Disponibilidad, Agendar, Citas, CitaDetalle, Perfil
    ├── medico/    # Agenda, Horario
    └── admin/     # Dashboard, Médicos (CRUD), Especialidades (CRUD), ConfigClinica
```

## Rutas

| Ruta | Pantalla | Rol |
|------|----------|-----|
| `/login` | Login | Público |
| `/registro` | Registro | Público |
| `/paciente/inicio` | Inicio paciente | Paciente |
| `/paciente/buscar` | Buscar médico | Paciente |
| `/paciente/medicos/:id/disponibilidad` | Disponibilidad | Paciente |
| `/paciente/agendar` | Agendar cita | Paciente |
| `/paciente/citas` | Mis citas | Paciente |
| `/paciente/citas/:id` | Detalle cita | Paciente |
| `/paciente/perfil` | Mi perfil | Paciente |
| `/medico/agenda` | Agenda del día | Médico |
| `/medico/horario` | Mi horario | Médico |
| `/admin/dashboard` | Dashboard | Admin |
| `/admin/medicos` | CRUD Médicos | Admin |
| `/admin/especialidades` | CRUD Especialidades | Admin |
| `/admin/configuracion-clinica` | Config clínica | Admin |
