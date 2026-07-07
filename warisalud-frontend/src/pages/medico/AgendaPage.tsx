import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Calendar, ChevronLeft, ChevronRight, Clock, User } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import { useAuth } from '../../context/AuthContext';
import api from '../../lib/api';
import type { Cita } from '../../types';
import { formatHora, getEstadoClass, getEstadoLabel, getInitials } from '../../lib/utils';

function addDays(date: Date, days: number): Date {
  const d = new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}

function toISODate(date: Date): string {
  return date.toISOString().split('T')[0];
}

function formatDiaSemana(date: Date): string {
  return date.toLocaleDateString('es-PE', { weekday: 'long', day: 'numeric', month: 'short' });
}

export default function AgendaPage() {
  const { user } = useAuth();
  const medicoId = user?.sub;
  const [fecha, setFecha] = useState(new Date());

  const fechaStr = toISODate(fecha);

  const { data: citas = [], isLoading } = useQuery<Cita[]>({
    queryKey: ['agenda', medicoId, fechaStr],
    queryFn: async () => (await api.get<Cita[]>(`/api/medicos/${medicoId}/agenda`, { params: { fecha: fechaStr } })).data,
    enabled: !!medicoId,
  });

  return (
    <AppLayout>
      <div>
        <div className="page-header">
          <h1>Mi Agenda</h1>
          <p>Consulta tus citas programadas</p>
        </div>

        {/* Date navigation */}
        <div className="card mb-lg">
          <div className="flex items-center justify-between">
            <button className="btn btn-ghost btn-sm" id="prev-day" onClick={() => setFecha(addDays(fecha, -1))}>
              <ChevronLeft size={20} />
            </button>
            <div style={{ textAlign: 'center' }}>
              <p style={{ fontWeight: 700, fontSize: '1.125rem', textTransform: 'capitalize' }}>
                {formatDiaSemana(fecha)}
              </p>
              <input
                id="agenda-fecha"
                type="date"
                value={fechaStr}
                onChange={(e) => setFecha(new Date(e.target.value + 'T12:00:00'))}
                style={{ marginTop: 4, border: 'none', background: 'transparent', color: 'var(--color-primary)', fontWeight: 600, cursor: 'pointer', fontFamily: 'inherit', fontSize: '0.875rem' }}
              />
            </div>
            <button className="btn btn-ghost btn-sm" id="next-day" onClick={() => setFecha(addDays(fecha, 1))}>
              <ChevronRight size={20} />
            </button>
          </div>
        </div>

        {/* Stats */}
        <div className="grid-3 mb-lg">
          <div className="stat-card">
            <div className="stat-card-icon"><Calendar size={20} /></div>
            <div className="stat-value">{citas.filter(c => c.estado === 'Pendiente').length}</div>
            <div className="stat-label">Pendientes</div>
          </div>
          <div className="stat-card">
            <div className="stat-card-icon"><Clock size={20} /></div>
            <div className="stat-value">{citas.filter(c => c.estado === 'Completada').length}</div>
            <div className="stat-label">Completadas</div>
          </div>
          <div className="stat-card">
            <div className="stat-card-icon"><User size={20} /></div>
            <div className="stat-value">{citas.length}</div>
            <div className="stat-label">Total del día</div>
          </div>
        </div>

        {isLoading && <div className="loading-state"><div className="spinner spinner-lg" /><p>Cargando agenda...</p></div>}

        {!isLoading && citas.length === 0 && (
          <div className="empty-state">
            <div className="empty-state-icon"><Calendar size={32} /></div>
            <p style={{ fontWeight: 600 }}>Sin citas este día</p>
            <p style={{ fontSize: '0.875rem' }}>No tienes citas programadas para esta fecha.</p>
          </div>
        )}

        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
          {citas
            .sort((a, b) => new Date(a.fechaHora).getTime() - new Date(b.fechaHora).getTime())
            .map((cita) => {
              const statusClass = cita.estado === 'Pendiente' ? 'status-pending' : cita.estado === 'Completada' ? 'status-done' : 'status-cancelled';
              return (
                <div key={cita.id} id={`agenda-cita-${cita.id}`} className={`card appointment-card ${statusClass}`}>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-md">
                      <div className="doctor-avatar" style={{ width: 48, height: 48 }}>{getInitials(`Paciente ${cita.pacienteId}`)}</div>
                      <div>
                        <p style={{ fontWeight: 600 }}>Paciente #{cita.pacienteId}</p>
                        {cita.motivo && <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.8125rem', marginTop: 2 }}>Motivo: {cita.motivo}</p>}
                        <div className="flex items-center gap-sm" style={{ marginTop: 'var(--space-sm)' }}>
                          <Clock size={14} style={{ color: 'var(--color-primary)' }} />
                          <span style={{ fontSize: '0.875rem', color: 'var(--color-on-surface-variant)' }}>
                            {formatHora(cita.fechaHora)} ({cita.duracionMinutos} min)
                          </span>
                        </div>
                      </div>
                    </div>
                    <span className={`chip ${getEstadoClass(cita.estado)}`}>{getEstadoLabel(cita.estado)}</span>
                  </div>
                </div>
              );
            })}
        </div>
      </div>
    </AppLayout>
  );
}
