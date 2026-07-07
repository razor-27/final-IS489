import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Calendar, Search, Clock, Plus, ChevronRight } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import { useAuth } from '../../context/AuthContext';
import api from '../../lib/api';
import type { Cita } from '../../types';
import { formatFecha, formatHora, getEstadoClass, getEstadoLabel } from '../../lib/utils';

export default function InicioPage() {
  const { user } = useAuth();
  const nombre = user?.nombre ?? 'Paciente';

  const { data: citas = [], isLoading } = useQuery<Cita[]>({
    queryKey: ['citas', 'mias'],
    queryFn: async () => {
      const res = await api.get<Cita[]>('/api/citas/mias');
      return res.data;
    },
  });

  const proximas = citas
    .filter((c) => c.estado === 'Pendiente' && new Date(c.fechaHora) > new Date())
    .sort((a, b) => new Date(a.fechaHora).getTime() - new Date(b.fechaHora).getTime())
    .slice(0, 3);

  const hoy = new Date().toISOString().split('T')[0];
  const citasHoy = citas.filter((c) => c.fechaHora.startsWith(hoy) && c.estado === 'Pendiente').length;

  return (
    <AppLayout>
      <div>
        {/* Hero */}
        <div className="hero-gradient mb-lg">
          <div style={{ position: 'relative', zIndex: 1 }}>
            <p style={{ opacity: 0.8, fontSize: '0.875rem', marginBottom: 'var(--space-xs)', letterSpacing: '0.05em', textTransform: 'uppercase', fontWeight: 600 }}>Bienvenido</p>
            <h1 style={{ fontSize: '1.75rem', fontWeight: 700, marginBottom: 'var(--space-sm)' }}>
              Hola, {nombre.split(' ')[0]} 👋
            </h1>
            <p style={{ opacity: 0.85, fontSize: '1rem' }}>
              {proximas.length > 0 ? `Tienes ${proximas.length} cita(s) próxima(s) pendiente(s).` : 'No tienes citas próximas. ¡Agenda una ahora!'}
            </p>
          </div>
        </div>

        {/* Quick actions */}
        <div className="grid-3 mb-lg">
          <Link to="/paciente/buscar" id="quick-buscar" style={{ textDecoration: 'none' }}>
            <div className="stat-card" style={{ cursor: 'pointer' }}>
              <div className="stat-card-icon"><Search size={24} /></div>
              <div className="stat-value" style={{ fontSize: '1.25rem' }}>Buscar Médico</div>
              <div className="stat-label">Encuentra tu especialista</div>
            </div>
          </Link>
          <Link to="/paciente/citas" id="quick-citas" style={{ textDecoration: 'none' }}>
            <div className="stat-card" style={{ cursor: 'pointer' }}>
              <div className="stat-card-icon"><Calendar size={24} /></div>
              <div className="stat-value" style={{ fontSize: '1.25rem' }}>Mis Citas</div>
              <div className="stat-label">{citas.length} en total</div>
            </div>
          </Link>
          <div className="stat-card">
            <div className="stat-card-icon"><Clock size={24} /></div>
            <div className="stat-value">{citasHoy} / 2</div>
            <div className="stat-label">Citas usadas hoy</div>
          </div>
        </div>

        {/* Upcoming appointments */}
        <div className="card">
          <div className="flex items-center justify-between mb-md">
            <h2 style={{ fontSize: '1.125rem', fontWeight: 600 }}>Próximas Citas</h2>
            <Link to="/paciente/citas" style={{ color: 'var(--color-primary)', fontSize: '0.875rem', fontWeight: 600, display: 'flex', alignItems: 'center', gap: 4 }}>
              Ver todas <ChevronRight size={16} />
            </Link>
          </div>

          {isLoading && (
            <div className="loading-state"><div className="spinner" /></div>
          )}

          {!isLoading && proximas.length === 0 && (
            <div className="empty-state">
              <div className="empty-state-icon"><Calendar size={32} /></div>
              <p style={{ fontWeight: 600, marginBottom: 4 }}>Sin citas próximas</p>
              <p style={{ fontSize: '0.875rem' }}>Agenda tu primera cita con un médico.</p>
              <Link to="/paciente/buscar" className="btn btn-primary btn-sm" style={{ marginTop: 'var(--space-md)' }}>
                <Plus size={16} /> Agendar Cita
              </Link>
            </div>
          )}

          {!isLoading && proximas.map((cita) => (
            <Link key={cita.id} to={`/paciente/citas/${cita.id}`} style={{ textDecoration: 'none' }}>
              <div className={`appointment-card card card-interactive status-pending mb-sm`} style={{ marginBottom: 'var(--space-sm)' }}>
                <div className="flex items-center justify-between">
                  <div>
                    <p style={{ fontWeight: 600, fontSize: '0.9375rem' }}>{cita.medico?.nombreCompleto ?? `Médico #${cita.medicoId}`}</p>
                    <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.8125rem', marginTop: 2 }}>
                      {cita.medico?.especialidad?.nombre ?? 'Especialidad'}
                    </p>
                    <div className="flex items-center gap-sm" style={{ marginTop: 'var(--space-sm)' }}>
                      <Clock size={14} style={{ color: 'var(--color-primary)' }} />
                      <span style={{ fontSize: '0.875rem', color: 'var(--color-on-surface-variant)' }}>
                        {formatFecha(cita.fechaHora)} — {formatHora(cita.fechaHora)}
                      </span>
                    </div>
                  </div>
                  <div className="flex flex-col items-center gap-sm">
                    <span className={`chip ${getEstadoClass(cita.estado)}`}>{getEstadoLabel(cita.estado)}</span>
                    <ChevronRight size={18} style={{ color: 'var(--color-outline)' }} />
                  </div>
                </div>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </AppLayout>
  );
}
