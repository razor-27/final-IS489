import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Calendar, Plus } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import api from '../../lib/api';
import type { Cita, EstadoCita } from '../../types';
import { formatFecha, formatHora, getEstadoClass, getEstadoLabel } from '../../lib/utils';

const ESTADOS: { value: EstadoCita | ''; label: string }[] = [
  { value: '', label: 'Todas' },
  { value: 'Pendiente', label: 'Pendientes' },
  { value: 'Completada', label: 'Completadas' },
  { value: 'Cancelada', label: 'Canceladas' },
];

export default function CitasPage() {
  const [estadoFilter, setEstadoFilter] = useState<EstadoCita | ''>('');

  const { data: citas = [], isLoading } = useQuery<Cita[]>({
    queryKey: ['citas', 'mias', estadoFilter],
    queryFn: async () => {
      const params: Record<string, string> = {};
      if (estadoFilter) params.estado = estadoFilter;
      return (await api.get<Cita[]>('/api/citas/mias', { params })).data;
    },
  });

  return (
    <AppLayout>
      <div>
        <div className="flex items-center justify-between mb-lg">
          <div className="page-header" style={{ marginBottom: 0 }}>
            <h1>Mis Citas</h1>
            <p>Gestiona todas tus citas médicas</p>
          </div>
          <Link to="/paciente/buscar" id="btn-nueva-cita" className="btn btn-primary btn-sm">
            <Plus size={16} /> Nueva Cita
          </Link>
        </div>

        {/* Filter chips */}
        <div className="flex gap-sm mb-lg" style={{ flexWrap: 'wrap' }}>
          {ESTADOS.map((e) => (
            <button
              key={e.value}
              id={`filter-${e.value || 'todas'}`}
              onClick={() => setEstadoFilter(e.value as EstadoCita | '')}
              className={`chip ${estadoFilter === e.value ? 'chip-primary' : ''}`}
              style={{
                cursor: 'pointer',
                border: '2px solid',
                borderColor: estadoFilter === e.value ? 'var(--color-primary)' : 'var(--color-outline-variant)',
                background: estadoFilter === e.value ? 'var(--color-surface-container)' : 'transparent',
                color: estadoFilter === e.value ? 'var(--color-primary)' : 'var(--color-on-surface-variant)',
                padding: '6px 16px',
              }}
            >
              {e.label}
            </button>
          ))}
        </div>

        {isLoading && <div className="loading-state"><div className="spinner spinner-lg" /><p>Cargando citas...</p></div>}

        {!isLoading && citas.length === 0 && (
          <div className="empty-state">
            <div className="empty-state-icon"><Calendar size={32} /></div>
            <p style={{ fontWeight: 600, marginBottom: 4 }}>Sin citas</p>
            <p style={{ fontSize: '0.875rem' }}>No tienes citas con el filtro seleccionado.</p>
            <Link to="/paciente/buscar" className="btn btn-primary btn-sm" style={{ marginTop: 'var(--space-md)' }}>
              <Plus size={16} /> Agendar Cita
            </Link>
          </div>
        )}

        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-sm)' }}>
          {citas.map((cita) => {
            const statusClass = cita.estado === 'Pendiente' ? 'status-pending' : cita.estado === 'Completada' ? 'status-done' : 'status-cancelled';
            return (
              <Link key={cita.id} to={`/paciente/citas/${cita.id}`} id={`cita-${cita.id}`} style={{ textDecoration: 'none' }}>
                <div className={`card card-interactive appointment-card ${statusClass}`}>
                  <div className="flex items-center justify-between">
                    <div style={{ flex: 1 }}>
                      <p style={{ fontWeight: 600, fontSize: '0.9375rem' }}>
                        {cita.medico?.nombreCompleto ?? `Médico #${cita.medicoId}`}
                      </p>
                      {cita.medico?.especialidad && (
                        <span className="chip chip-primary" style={{ marginTop: 4, display: 'inline-flex' }}>{cita.medico.especialidad.nombre}</span>
                      )}
                      <div className="flex items-center gap-sm" style={{ marginTop: 'var(--space-sm)' }}>
                        <Calendar size={14} style={{ color: 'var(--color-primary)', flexShrink: 0 }} />
                        <span style={{ fontSize: '0.8125rem', color: 'var(--color-on-surface-variant)' }}>
                          {formatFecha(cita.fechaHora)} — {formatHora(cita.fechaHora)}
                        </span>
                      </div>
                      {cita.motivo && (
                        <p style={{ fontSize: '0.8125rem', color: 'var(--color-on-surface-variant)', marginTop: 4 }}>Motivo: {cita.motivo}</p>
                      )}
                    </div>
                    <span className={`chip ${getEstadoClass(cita.estado)}`}>{getEstadoLabel(cita.estado)}</span>
                  </div>
                </div>
              </Link>
            );
          })}
        </div>
      </div>
    </AppLayout>
  );
}
