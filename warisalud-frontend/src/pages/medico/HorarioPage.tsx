import { useQuery } from '@tanstack/react-query';
import { Clock, Calendar } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import { useAuth } from '../../context/AuthContext';
import api from '../../lib/api';
import type { HorarioMedico, ConfiguracionClinica } from '../../types';

export default function HorarioPage() {
  const { user } = useAuth();
  const medicoId = user?.sub;

  const { data: horario, isLoading: loadH } = useQuery<HorarioMedico | null>({
    queryKey: ['horario-medico', medicoId],
    queryFn: async () => {
      try {
        return (await api.get<HorarioMedico>(`/api/medicos/${medicoId}/horario`)).data;
      } catch {
        return null;
      }
    },
    enabled: !!medicoId,
  });

  const { data: config, isLoading: loadC } = useQuery<ConfiguracionClinica>({
    queryKey: ['config-clinica'],
    queryFn: async () => (await api.get<ConfiguracionClinica>('/api/configuracion-clinica')).data,
  });

  const efectivo = horario ?? config;
  const esPersonalizado = !!horario;

  // Normalize fields: HorarioMedico uses horaInicio/horaFin, ConfiguracionClinica uses horaApertura/horaCierre
  const horaInicio = horario?.horaInicio ?? config?.horaApertura ?? '';
  const horaFin = horario?.horaFin ?? config?.horaCierre ?? '';
  const diasLaborables = efectivo?.diasLaborables ?? '';

  return (
    <AppLayout>
      <div style={{ maxWidth: 480, margin: '0 auto' }}>
        <div className="page-header">
          <h1>Mi Horario</h1>
          <p>Horario efectivo de atención</p>
        </div>

        {(loadH || loadC) && <div className="loading-state"><div className="spinner spinner-lg" /></div>}

        {!loadH && !loadC && efectivo && (
          <>
            <div style={{ marginBottom: 'var(--space-md)' }}>
              <span className={`chip ${esPersonalizado ? 'chip-primary' : ''}`} style={{ border: '1px solid var(--color-outline-variant)' }}>
                {esPersonalizado ? 'Horario personalizado' : 'Horario global de la clínica'}
              </span>
            </div>

            <div className="card mb-lg">
              <h2 style={{ fontWeight: 600, marginBottom: 'var(--space-lg)', fontSize: '1rem' }}>Detalle del horario</h2>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
                <div className="flex items-center gap-md">
                  <div className="stat-card-icon" style={{ width: 40, height: 40 }}><Clock size={18} /></div>
                  <div>
                    <p className="text-label-sm text-muted">Horario de atención</p>
                    <p style={{ fontWeight: 600, fontSize: '1.125rem' }}>{horaInicio} — {horaFin}</p>
                  </div>
                </div>
                <div className="flex items-center gap-md">
                  <div className="stat-card-icon" style={{ width: 40, height: 40 }}><Calendar size={18} /></div>
                  <div>
                    <p className="text-label-sm text-muted">Días laborables</p>
                    <p style={{ fontWeight: 600, fontSize: '1.125rem' }}>{diasLaborables}</p>
                  </div>
                </div>
              </div>
            </div>

            {!esPersonalizado && (
              <div style={{ padding: 'var(--space-md)', background: 'var(--color-surface-container-low)', borderRadius: 'var(--radius-md)', fontSize: '0.875rem', color: 'var(--color-on-surface-variant)' }}>
                ℹ️ Este es el horario global de la clínica. Contacta al administrador si necesitas un horario personalizado.
              </div>
            )}
          </>
        )}
      </div>
    </AppLayout>
  );
}
