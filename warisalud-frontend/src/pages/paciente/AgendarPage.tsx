import { useLocation, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQuery, useMutation } from '@tanstack/react-query';
import { ArrowLeft, Calendar, Clock, CheckCircle } from 'lucide-react';
import { useState } from 'react';
import AppLayout from '../../components/AppLayout';
import { useToast } from '../../context/ToastContext';
import api from '../../lib/api';
import type { Cita, Medico } from '../../types';
import { formatFecha, formatHora, getApiErrorMessage, getInitials } from '../../lib/utils';

const schema = z.object({
  motivo: z.string().max(500).optional(),
});
type FormValues = z.infer<typeof schema>;

interface LocationState {
  medicoId: number;
  medico?: Medico;
  fechaHora: string;
}

export default function AgendarPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const toast = useToast();
  const state = location.state as LocationState | null;

  const [confirmed, setConfirmed] = useState(false);

  const { data: citas = [] } = useQuery<Cita[]>({
    queryKey: ['citas', 'mias'],
    queryFn: async () => (await api.get<Cita[]>('/api/citas/mias')).data,
  });

  const citasHoy = state?.fechaHora
    ? citas.filter((c) => c.fechaHora.startsWith(state.fechaHora.split('T')[0]) && c.estado === 'Pendiente').length
    : 0;

  const { register, handleSubmit } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const mutation = useMutation({
    mutationFn: async (data: FormValues) => {
      return api.post<Cita>('/api/citas', {
        medicoId: state!.medicoId,
        fechaHora: state!.fechaHora,
        motivo: data.motivo,
      });
    },
    onSuccess: () => {
      setConfirmed(true);
      toast.success('¡Cita agendada exitosamente!');
    },
    onError: (err) => {
      toast.error(getApiErrorMessage(err));
    },
  });

  if (!state) {
    return <AppLayout><div className="empty-state"><p>No hay datos para agendar. <button className="btn btn-primary btn-sm" onClick={() => navigate('/paciente/buscar')}>Buscar médico</button></p></div></AppLayout>;
  }

  if (confirmed) {
    return (
      <AppLayout>
        <div style={{ maxWidth: 480, margin: '0 auto', textAlign: 'center', paddingTop: 'var(--space-xl)' }}>
          <div style={{ width: 80, height: 80, background: 'var(--color-state-done-bg)', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto var(--space-lg)' }}>
            <CheckCircle size={40} style={{ color: 'var(--color-state-done)' }} />
          </div>
          <h1 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: 'var(--space-sm)' }}>¡Cita confirmada!</h1>
          <p className="text-muted" style={{ marginBottom: 'var(--space-lg)' }}>Tu cita ha sido agendada exitosamente.</p>
          <div className="card" style={{ textAlign: 'left', marginBottom: 'var(--space-lg)' }}>
            <div className="flex items-center gap-md" style={{ marginBottom: 'var(--space-md)' }}>
              <div className="doctor-avatar" style={{ width: 48, height: 48 }}>{getInitials(state.medico?.nombreCompleto ?? 'M')}</div>
              <div>
                <p style={{ fontWeight: 600 }}>{state.medico?.nombreCompleto}</p>
                <span className="chip chip-primary">{state.medico?.especialidad?.nombre}</span>
              </div>
            </div>
            <div className="flex items-center gap-sm">
              <Clock size={16} style={{ color: 'var(--color-primary)' }} />
              <span style={{ fontSize: '0.875rem' }}>{formatFecha(state.fechaHora)} a las {formatHora(state.fechaHora)}</span>
            </div>
          </div>
          <div className="flex gap-md justify-center">
            <button className="btn btn-secondary" onClick={() => navigate('/paciente/citas')}>Ver mis citas</button>
            <button className="btn btn-primary" onClick={() => navigate('/paciente/inicio')}>Ir al inicio</button>
          </div>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <div style={{ maxWidth: 560, margin: '0 auto' }}>
        <button className="btn btn-ghost btn-sm mb-md" onClick={() => navigate(-1)}><ArrowLeft size={16} /> Volver</button>

        <div className="page-header">
          <h1>Confirmar Cita</h1>
          <p>Revisa los detalles antes de confirmar</p>
        </div>

        {/* Counter */}
        {citasHoy >= 1 && (
          <div style={{ background: 'var(--color-warning-container)', color: 'var(--color-warning)', padding: 'var(--space-md)', borderRadius: 'var(--radius-md)', marginBottom: 'var(--space-md)', fontSize: '0.875rem', fontWeight: 500 }}>
            ⚠️ Tienes {citasHoy} de 2 citas máximas para este día.
          </div>
        )}

        {/* Summary card */}
        <div className="card mb-lg">
          <h2 style={{ fontWeight: 600, marginBottom: 'var(--space-md)', fontSize: '1rem' }}>Resumen de la cita</h2>
          <div className="flex items-center gap-md mb-md">
            <div className="doctor-avatar">{getInitials(state.medico?.nombreCompleto ?? 'M')}</div>
            <div>
              <p style={{ fontWeight: 600 }}>{state.medico?.nombreCompleto}</p>
              {state.medico?.especialidad && <span className="chip chip-primary">{state.medico.especialidad.nombre}</span>}
            </div>
          </div>
          <div className="flex items-center gap-sm">
            <Calendar size={16} style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontSize: '0.875rem' }}>{formatFecha(state.fechaHora)}</span>
          </div>
          <div className="flex items-center gap-sm" style={{ marginTop: 'var(--space-xs)' }}>
            <Clock size={16} style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontSize: '0.875rem' }}>{formatHora(state.fechaHora)}</span>
            {state.medico?.especialidad && <span style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.8125rem' }}>({state.medico.especialidad.duracionCitaMinutos} min)</span>}
          </div>
        </div>

        <form onSubmit={handleSubmit((d) => mutation.mutate(d))} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
          <div className="form-group">
            <label htmlFor="agendar-motivo" className="form-label">Motivo de consulta <span style={{ color: 'var(--color-outline)' }}>(opcional)</span></label>
            <textarea
              id="agendar-motivo"
              className="form-input"
              rows={3}
              placeholder="Describe brevemente tu motivo de consulta..."
              style={{ height: 'auto', paddingTop: 12, paddingBottom: 12, resize: 'vertical' }}
              {...register('motivo')}
            />
          </div>

          <button id="btn-confirmar-cita" type="submit" className="btn btn-primary w-full" disabled={mutation.isPending}>
            {mutation.isPending ? <span className="spinner" style={{ width: 20, height: 20, borderWidth: 2 }} /> : <CheckCircle size={18} />}
            {mutation.isPending ? 'Agendando...' : 'Confirmar Cita'}
          </button>
        </form>
      </div>
    </AppLayout>
  );
}
