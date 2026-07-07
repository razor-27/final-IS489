# 🏥 WariSalud - Sistema Integral de Gestión de Citas Médicas

![Andean Heritage Health Design](https://img.shields.io/badge/Estilo-Andean%20Heritage%20Health-8B4513?style=for-the-badge)
![.NET 10 Web API](https://img.shields.io/badge/.NET%2010-Web%20API-512BD4?style=for-the-badge&logo=dotnet)
![React 18 TypeScript](https://img.shields.io/badge/React%2018-TypeScript-61DAFB?style=for-the-badge&logo=react)
![Vite](https://img.shields.io/badge/Vite-Build%20Tool-646CFF?style=for-the-badge&logo=vite)

**WariSalud** es una plataforma web moderna y segura para la gestión clínica y agendamiento de citas médicas. Está diseñada bajo los principios de **Arquitectura Limpia (Clean Architecture)** en el backend y una interfaz moderna y altamente responsiva en el frontend inspirada en la paleta estética **Andean Heritage Health** (tonos tierra virreinal, terracota, esmeralda andina y glassmorphism).

---

## 🛠️ Stack Tecnológico

### 🔹 Backend (.NET 10 / ASP.NET Core Web API)
* **Framework Core**: .NET 10 (C# 13) con ASP.NET Core Web API.
* **Arquitectura**: Clean Architecture con separación estricta en capas (`Core`, `Infrastructure`, `API`, `Tests`).
* **Base de Datos & ORM**: Entity Framework Core con proveedor `InMemory` (para desarrollo, pruebas y ejecución instantánea sin dependencias externas). Compatible con SQL Server y SQLite.
* **Seguridad & Autenticación**:
  * Autenticación basada en **JSON Web Tokens (JWT)** con claims por rol (`Admin`, `Medico`, `Paciente`).
  * Encriptación criptográfica de contraseñas mediante **PBKDF2 (SHA-256)** con sal aleatoria de 16 bytes y 100,000 iteraciones.
* **Documentación API**: OpenAPI / Swagger UI integrado para exploración interactiva de endpoints.

### 🔹 Frontend (React 18 + TypeScript + Vite)
* **Core & Enrutamiento**: React 18, TypeScript y React Router DOM v6 con protección de rutas por roles (`ProtectedRoute`).
* **Gestión de Estado & API**: TanStack Query (React Query) v5 para caché de datos, sincronización en tiempo real e invalidación inteligente, junto con `axios` e interceptores JWT.
* **Formularios & Validación**: React Hook Form combinado con **Zod** para validación estricta de esquemas de entrada.
* **Iconografía & UI**: Lucide React.
* **Diseño & Estilos**: Vanilla CSS personalizado y responsivo, implementando variables de diseño modernas, animaciones suaves y micro-interacciones sin frameworks CSS pesados.

---

## 👥 Roles y Funcionalidades del Sistema

El sistema es multi-usuario y adapta su interfaz y permisos según el rol del usuario autenticado:

### 👑 1. Administrador (`Admin`)
* **Panel de Control (Dashboard)**: Visualización general del estado de la clínica.
* **Gestión de Médicos**: Creación, edición, eliminación y listado del personal médico (asigna automáticamente credenciales de usuario con encriptación segura).
* **Gestión de Especialidades**: CRUD completo para especialidades médicas (Cardiología, Pediatría, Medicina General, Dermatología, etc.), configurando la duración de cita predeterminada.
* **Configuración de la Clínica**: Definición del horario de apertura, horario de cierre y los días laborables de atención general del centro hospitalario.

### 🩺 2. Médico (`Medico`)
* **Mi Agenda**: Visualización interactiva de las citas programadas para el día actual o cualquier fecha elegida. Permite gestionar el estado clínico de cada cita (**Pendiente**, **Completada** o **Cancelada**).
* **Mi Horario**: Configuración de horas laborales personalizadas de inicio y fin de jornada, así como los días de atención de la semana.

### 🧑 3. Paciente (`Paciente`)
* **Búsqueda de Médicos**: Exploración y filtrado de profesionales de la salud por especialidad o nombre.
* **Consulta de Disponibilidad en Tiempo Real**: Cálculo dinámico en el backend de los bloques de tiempo libres, restando los horarios ya reservados y respetando las reglas de la clínica y del médico.
* **Reserva y Cancelación de Citas**: Agendamiento de nuevas citas clínicas con motivo de consulta y opción de cancelación cumpliendo las políticas de anticipación (24 horas).
* **Historial y Perfil**: Seguimiento de citas pasadas y futuras, e información personal.

---

## 🔑 Credenciales de Acceso Preconfiguradas (Seeder)

El sistema cuenta con un inicializador automático (`DbSeeder`) que precarga datos reales en la base de datos cada vez que se arranca el backend. Puedes usar cualquiera de las siguientes credenciales para probar los diferentes paneles:

| Rol | Correo Electrónico | Contraseña | Detalle / Especialidad |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin@warisalud.pe` | `admin123` | Control total del centro clínico |
| **Médico** | `carlos.mendoza@warisalud.pe` | `medico123` | Dr. Carlos Mendoza (Cardiología) |
| **Médico** | `ana.huaman@warisalud.pe` | `medico123` | Dra. Ana Huamán (Pediatría) |
| **Médico** | `luis.quispe@warisalud.pe` | `medico123` | Dr. Luis Quispe (Medicina General) |
| **Médico** | `sofia.vargas@warisalud.pe` | `medico123` | Dra. Sofía Vargas (Dermatología) |
| **Paciente** | `maria.rojas@gmail.com` | `paciente123` | Paciente de prueba con historial de citas |
| **Paciente** | `juan.perez@gmail.com` | `paciente123` | Paciente de prueba con historial de citas |

> 💡 *Nota: Además de estos usuarios, cualquier persona puede registrar una cuenta nueva de **Paciente** desde la pantalla de Registro de la aplicación.*

---

## 🚀 Guía de Instalación y Arranque

### 📋 Prerequisitos
* [.NET SDK 10.0](https://dotnet.microsoft.com/download) (compatible con entorno .NET actual).
* [Node.js](https://nodejs.org/) (v18 o superior) y `npm`.

---

### 🖥️ Paso 1: Arrancar el Servidor Backend (.NET Web API)

1. Abre una terminal y navega a la carpeta de la API:
   ```bash
   cd WariSalud.API
   ```
2. Restaura las dependencias y ejecuta el servidor:
   ```bash
   dotnet run
   ```
3. El servidor iniciará en las siguientes URLs (por defecto según configuración en `launchSettings.json`):
   * **HTTPS**: `https://localhost:7159`
   * **HTTP**: `http://localhost:5288`
   * **Swagger UI (Documentación API)**: [https://localhost:7159/swagger](https://localhost:7159/swagger)

> *Al iniciar, el sistema ejecutará el `DbSeeder`, creando las tablas en memoria e insertando los usuarios, especialidades, médicos y citas de demostración.*

---

### 🌐 Paso 2: Arrancar la Aplicación Frontend (React + Vite)

1. Abre una nueva terminal (sin cerrar la del backend) y entra a la carpeta del frontend:
   ```bash
   cd warisalud-frontend
   ```
2. Instala los paquetes de Node.js:
   ```bash
   npm install
   ```
3. Inicia el servidor de desarrollo local:
   ```bash
   npm run dev
   ```
4. Abre tu navegador web en la dirección indicada por la consola (normalmente):
   * **URL Web**: [http://localhost:5173](http://localhost:5173)

---

## 📁 Estructura del Proyecto

```text
WariSalud/
├── WariSalud.Core/                  # Capa del Dominio (Entidades, Interfaces, Excepciones y Servicios de Negocio)
├── WariSalud.Infrastructure/        # Capa de Infraestructura (EF Core DbContext, Repositorios, DbSeeder)
├── WariSalud.API/                   # Capa de Presentación (Controladores REST, DTOs, Autenticación JWT, Swagger)
├── WariSalud.Tests/                 # Pruebas Unitarias y de Integración
├── WariSalud.slnx                   # Archivo de Solución de .NET
└── warisalud-frontend/              # Aplicación Web Frontend (React 18 + TypeScript + Vite)
    ├── src/
    │   ├── components/              # Componentes UI reutilizables y layout de la aplicación
    │   ├── context/                 # Contextos de React (AuthContext, ToastContext)
    │   ├── lib/                     # Configuración de Axios, JWT y utilidades generales
    │   ├── pages/                   # Vistas por Rol (admin/, medico/, paciente/, auth/)
    │   ├── types/                   # Definiciones e interfaces de TypeScript
    │   └── index.css                # Sistema de Diseño CSS (Andean Heritage Health Palette)
    └── package.json                 # Dependencias y scripts de Node.js
```

---

## 🧪 Ejecución de Pruebas Unitarias

Para validar las reglas de negocio y la lógica de agendamiento en el backend, puedes correr la suite de pruebas automatizadas:

```bash
dotnet test
```

---
*Desarrollado con estándares profesionales de ingeniería de software para el sistema de atención médica WariSalud.*
