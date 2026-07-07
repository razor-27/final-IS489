import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Calendar, Clock, User, AlertTriangle, Trash2 } from 'lucide-react';
import { useState } from 'react';
import AppLayout from '../../components/AppLayout';
import { useToast } from '../../context/ToastContext';
import api from '../../lib/api';
import type { Cita } from '../../types';
import { formatFecha, formatHora, getEstadoClass, getEstadoLabel, canCancelar, getApiErrorMessage, getInitials } from '../../lib/utils';

export default function CitaDetallePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();
  const qc = useQueryClient();
  const [showConfirm, setShowConfirm] = useState(false);

  const { data: cita, isLoading } = useQuery<Cita>({
    queryKey: ['cita', id],
    queryFn: async () => {
      const all = (await api.get<Cita[]>('/api/citas/mias')).data;
      const found = all.find((c) => String(c.id) === id);
      if (!found) throw new Error('Cita no encontrada');
      return found;
    },
    enabled: !!id,
  });

  const cancelMutation = useMutation({
    mutationFn: () => api.delete(`/api/citas/${id}`),
    onSuccess: () => {
      toast.success('Cita cancelada correctamente.');
      qc.invalidateQueries({ queryKey: ['citas'] });
      navigate('/paciente/citas');
    },
    onError: (err) => {
      toast.error(getApiErrorMessage(err));
      setShowConfirm(false);
    },
  });

  if (isLoading) return <AppLayout><div className="loading-state" style={{ minHeight: 300 }}><div className="spinner spinner-lg" /></div></AppLayout>;
  if (!cita) return <AppLayout><div className="empty-state"><p>Cita no encontrada.</p></div></AppLayout>;

  const puedeCancel = cita.estado === 'Pendiente' && canCancelar(cita.fechaHora);
  const noPuedeReason = !canCancelar(cita.fechaHora) ? 'No se puede cancelar con menos de 24h de antelación.' : cita.estado !== 'Pendiente' ? 'Solo se pueden cancelar citas pendientes.' : '';
  const statusClass = cita.estado === 'Pendiente' ? 'status-pending' : cita.estado === 'Completada' ? 'status-done' : 'status-cancelled';

  return (
    <AppLayout>
      <div style={{ maxWidth: 560, margin: '0 auto' }}>
        <button className="btn btn-ghost btn-sm mb-md" onClick={() => navigate(-1)}><ArrowLeft size={16} /> Volver</button>

        <div className="page-header">
          <h1>Detalle de Cita</h1>
        </div>

        <div className={`card appointment-card ${statusClass} mb-lg`}>
          <div className="flex items-center justify-between mb-md">
            <div className="flex items-center gap-md">
              <div className="doctor-avatar">{getInitials(cita.medico?.nombreCompleto ?? 'MD')}</div>
              <div>
                <p style={{ fontWeight: 700, fontSize: '1.0625rem' }}>{cita.medico?.nombreCompleto ?? `Médico #${cita.medicoId}`}</p>
                {cita.medico?.especialidad && <span className="chip chip-primary" style={{ marginTop: 4 }}>{cita.medico.especialidad.nombre}</span>}
              </div>
            </div>
            <span className={`chip ${getEstadoClass(cita.estado)}`}>{getEstadoLabel(cita.estado)}</span>
          </div>

          <div className="divider" />

          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-sm)' }}>
            <div className="flex items-center gap-sm">
              <Calendar size={16} style={{ color: 'var(--color-primary)', flexShrink: 0 }} />
              <span style={{ fontSize: '0.9375rem' }}>{formatFecha(cita.fechaHora)}</span>
            </div>
            <div className="flex items-center gap-sm">
              <Clock size={16} style={{ color: 'var(--color-primary)', flexShrink: 0 }} />
              <span style={{ fontSize: '0.9375rem' }}>{formatHora(cita.fechaHora)} ({cita.duracionMinutos} min)</span>
            </div>
            {cita.motivo && (
              <div className="flex items-center gap-sm">
                <User size={16} style={{ color: 'var(--color-primary)', flexShrink: 0 }} />
                <span style={{ fontSize: '0.9375rem' }}>Motivo: {cita.motivo}</span>
              </div>
            )}
          </div>
        </div>

        {cita.estado === 'Pendiente' && (
          <div className="card">
            <h2 style={{ fontWeight: 600, marginBottom: 'var(--space-md)', fontSize: '1rem' }}>Cancelar cita</h2>

            {!puedeCancel && noPuedeReason && (
              <div style={{ display: 'flex', gap: 'var(--space-sm)', background: 'var(--color-error-container)', color: 'var(--color-on-error-container)', padding: 'var(--space-md)', borderRadius: 'var(--radius-md)', marginBottom: 'var(--space-md)', fontSize: '0.875rem', fontWeight: 500 }}>
                <AlertTriangle size={18} style={{ flexShrink: 0, marginTop: 1 }} />
                {noPuedeReason}
              </div>
            )}

            <button
              id="btn-cancelar-cita"
              className="btn btn-danger"
              onClick={() => setShowConfirm(true)}
              disabled={!puedeCancel}
            >
              <Trash2 size={16} /> Cancelar Cita
            </button>
          </div>
        )}

        {/* Confirm Modal */}
        {showConfirm && (
          <div className="modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="cancel-modal-title">
            <div className="modal">
              <h2 id="cancel-modal-title" style={{ fontWeight: 700, marginBottom: 'var(--space-sm)' }}>Confirmar cancelación</h2>
              <p style={{ color: 'var(--color-on-surface-variant)', marginBottom: 'var(--space-lg)' }}>
                ¿Estás seguro de que deseas cancelar esta cita? Esta acción no se puede deshacer.
              </p>
              <div className="flex gap-md">
                <button className="btn btn-secondary" onClick={() => setShowConfirm(false)} disabled={cancelMutation.isPending}>Volver</button>
                <button id="confirm-cancel-btn" className="btn btn-danger" onClick={() => cancelMutation.mutate()} disabled={cancelMutation.isPending}>
                  {cancelMutation.isPending ? <span className="spinner" style={{ width: 18, height: 18, borderWidth: 2 }} /> : <Trash2 size={16} />}
                  Sí, cancelar
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </AppLayout>
  );
}
