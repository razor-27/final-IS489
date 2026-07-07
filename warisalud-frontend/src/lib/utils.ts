import type { EstadoCita } from '../types';

export function formatFecha(iso: string): string {
  return new Date(iso).toLocaleDateString('es-PE', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

export function formatHora(iso: string): string {
  return new Date(iso).toLocaleTimeString('es-PE', {
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function formatFechaCorta(iso: string): string {
  return new Date(iso).toLocaleDateString('es-PE', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });
}

export function getEstadoClass(estado: EstadoCita): string {
  switch (estado) {
    case 'Pendiente': return 'chip-pending';
    case 'Completada': return 'chip-done';
    case 'Cancelada': return 'chip-cancelled';
    default: return 'chip-primary';
  }
}

export function getEstadoLabel(estado: EstadoCita): string {
  switch (estado) {
    case 'Pendiente': return 'Pendiente';
    case 'Completada': return 'Completada';
    case 'Cancelada': return 'Cancelada';
    default: return estado;
  }
}

export function canCancelar(fechaHora: string): boolean {
  const diff = new Date(fechaHora).getTime() - Date.now();
  return diff >= 24 * 60 * 60 * 1000;
}

export function getInitials(name: string): string {
  return name
    .split(' ')
    .slice(0, 2)
    .map((n) => n[0]?.toUpperCase() ?? '')
    .join('');
}

export function getApiErrorMessage(error: unknown): string {
  if (!error || typeof error !== 'object') return 'Error desconocido';
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const e = error as any;
  const status = e?.response?.status;
  const msg = e?.response?.data?.message ?? e?.response?.data?.detail ?? e?.message ?? '';

  switch (status) {
    case 409: return msg || 'Ese horario ya no está disponible o ya tienes el máximo de citas ese día.';
    case 422: return msg || 'Fuera del horario de atención o no se puede cancelar con menos de 24h.';
    case 403: return 'No tienes permiso para esta acción.';
    case 401: return 'Tu sesión expiró. Inicia sesión de nuevo.';
    case 404: return 'Recurso no encontrado.';
    default: return msg || 'Ocurrió un error. Intenta de nuevo.';
  }
}
