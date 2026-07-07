// ── Domain Types ──────────────────────────────────────────────

export type Rol = 'Paciente' | 'Medico' | 'Admin';

export interface Usuario {
  id: number;
  email: string;
  rol: Rol;
  nombreCompleto?: string;
}

export interface AuthTokenPayload {
  sub: string;
  email: string;
  role: Rol;
  exp: number;
  iat: number;
  nombre?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  nombreCompleto: string;
  telefono: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  rol: Rol;
  usuarioId: number;
}

export interface Especialidad {
  id: number;
  nombre: string;
  descripcion: string;
  duracionCitaMinutos: number;
}

export interface Medico {
  id: number;
  usuarioId: number;
  especialidadId: number;
  especialidad?: Especialidad;
  nombreCompleto: string;
  numeroColegiatura: string;
}

export type EstadoCita = 'Pendiente' | 'Completada' | 'Cancelada';

export interface Cita {
  id: number;
  pacienteId: number;
  medicoId: number;
  medico?: Medico;
  fechaHora: string; // ISO datetime
  duracionMinutos: number;
  estado: EstadoCita;
  motivo?: string;
}

export interface DisponibilidadSlot {
  horaInicio: string; // HH:mm
  horaFin: string;
}

export interface AgendarCitaRequest {
  medicoId: number;
  fechaHora: string; // ISO datetime
  motivo?: string;
}

export interface HorarioMedico {
  id: number;
  medicoId: number;
  horaInicio: string;
  horaFin: string;
  diasLaborables: string;
}

export interface ConfiguracionClinica {
  id: number;
  horaApertura: string;
  horaCierre: string;
  diasLaborables: string;
}

export interface PerfilUsuario {
  id: number;
  email: string;
  rol: Rol;
  nombreCompleto: string;
  telefono?: string;
}

export interface ApiError {
  status: number;
  message: string;
  detail?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}
