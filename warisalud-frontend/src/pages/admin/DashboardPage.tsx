import { useQuery } from '@tanstack/react-query';
import { Users, Stethoscope, BookOpen, Calendar } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import api from '../../lib/api';
import type { Medico, Especialidad } from '../../types';

export default function DashboardPage() {
  const { data: medicos = [] } = useQuery<Medico[]>({
    queryKey: ['admin-medicos'],
    queryFn: async () => (await api.get<Medico[]>('/api/medicos')).data,
  });

  const { data: especialidades = [] } = useQuery<Especialidad[]>({
    queryKey: ['especialidades'],
    queryFn: async () => (await api.get<Especialidad[]>('/api/especialidades')).data,
  });

  const { data: stats } = useQuery({
    queryKey: ['admin-stats'],
    queryFn: async () => (await api.get<{ citasHoy: number; pacientes: number }>('/api/admin/dashboard/stats')).data,
  });

  return (
    <AppLayout>
      <div>
        <div className="page-header">
          <h1>Dashboard Administrativo</h1>
          <p>Resumen general de WariSalud</p>
        </div>

        <div className="hero-gradient mb-lg">
          <div style={{ position: 'relative', zIndex: 1 }}>
            <p style={{ opacity: 0.8, fontSize: '0.875rem', marginBottom: 'var(--space-xs)', letterSpacing: '0.05em', textTransform: 'uppercase', fontWeight: 600 }}>Panel de Control</p>
            <h2 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: 'var(--space-xs)' }}>WariSalud Admin</h2>
            <p style={{ opacity: 0.85 }}>Gestiona médicos, especialidades y la configuración de la clínica.</p>
          </div>
        </div>

        <div className="grid-4 mb-lg">
          <div className="stat-card">
            <div className="stat-card-icon"><Stethoscope size={22} /></div>
            <div className="stat-value">{medicos.length}</div>
            <div className="stat-label">Médicos activos</div>
          </div>
          <div className="stat-card">
            <div className="stat-card-icon"><BookOpen size={22} /></div>
            <div className="stat-value">{especialidades.length}</div>
            <div className="stat-label">Especialidades</div>
          </div>
          <div className="stat-card">
            <div className="stat-card-icon"><Calendar size={22} /></div>
            <div className="stat-value">{stats !== undefined ? stats.citasHoy : <span style={{opacity: 0.5}}>&mdash;</span>}</div>
            <div className="stat-label">Citas hoy</div>
          </div>
          <div className="stat-card">
            <div className="stat-card-icon"><Users size={22} /></div>
            <div className="stat-value">{stats !== undefined ? stats.pacientes : <span style={{opacity: 0.5}}>&mdash;</span>}</div>
            <div className="stat-label">Pacientes</div>
          </div>
        </div>

        {/* Recent doctors table */}
        <div className="card">
          <h2 style={{ fontWeight: 600, marginBottom: 'var(--space-lg)', fontSize: '1rem' }}>Médicos Registrados</h2>
          {medicos.length === 0 ? (
            <div className="empty-state" style={{ padding: 'var(--space-lg)' }}>
              <div className="empty-state-icon"><Stethoscope size={28} /></div>
              <p style={{ fontWeight: 600 }}>Sin médicos registrados</p>
            </div>
          ) : (
            <div className="table-wrapper">
              <table id="medicos-table">
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th>Especialidad</th>
                    <th>Colegiatura</th>
                  </tr>
                </thead>
                <tbody>
                  {medicos.slice(0, 10).map((m) => (
                    <tr key={m.id}>
                      <td style={{ fontWeight: 500 }}>{m.nombreCompleto}</td>
                      <td>{m.especialidad ? <span className="chip chip-primary">{m.especialidad.nombre}</span> : '—'}</td>
                      <td style={{ color: 'var(--color-on-surface-variant)' }}>{m.numeroColegiatura}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </AppLayout>
  );
}
