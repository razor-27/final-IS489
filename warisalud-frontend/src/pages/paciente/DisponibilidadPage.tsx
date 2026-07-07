import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Clock, ArrowLeft, ChevronRight } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import api from '../../lib/api';
import type { Medico, DisponibilidadSlot } from '../../types';
import { getInitials } from '../../lib/utils';

export default function DisponibilidadPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const today = new Date().toISOString().split('T')[0];
  const [fecha, setFecha] = useState(today);
  const [selected, setSelected] = useState<string | null>(null);

  const { data: medico } = useQuery<Medico>({
    queryKey: ['medico', id],
    queryFn: async () => (await api.get<Medico>(`/api/medicos/${id}`)).data,
    enabled: !!id,
  });

  const { data: slots = [], isLoading } = useQuery<DisponibilidadSlot[]>({
    queryKey: ['disponibilidad', id, fecha],
    queryFn: async () => (await api.get<DisponibilidadSlot[]>(`/api/medicos/${id}/disponibilidad`, { params: { fecha } })).data,
    enabled: !!id && !!fecha,
  });

  const handleAgendar = () => {
    if (!selected || !id) return;
    const fechaHora = `${fecha}T${selected}:00`;
    navigate('/paciente/agendar', { state: { medicoId: Number(id), medico, fechaHora } });
  };

  return (
    <AppLayout>
      <div>
        <button className="btn btn-ghost btn-sm mb-md" onClick={() => navigate(-1)}>
          <ArrowLeft size={16} /> Volver
        </button>

        {medico && (
          <div className="card mb-lg">
            <div className="flex items-center gap-md">
              <div className="doctor-avatar">{getInitials(medico.nombreCompleto)}</div>
              <div>
                <h1 style={{ fontSize: '1.25rem', fontWeight: 700 }}>{medico.nombreCompleto}</h1>
                {medico.especialidad && <span className="chip chip-primary">{medico.especialidad.nombre}</span>}
                <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.8125rem', marginTop: 4 }}>Colegiatura: {medico.numeroColegiatura}</p>
              </div>
            </div>
          </div>
        )}

        <div className="card">
          <h2 style={{ fontSize: '1.125rem', fontWeight: 600, marginBottom: 'var(--space-md)' }}>Selecciona fecha y hora</h2>

          <div className="form-group mb-lg">
            <label htmlFor="fecha-disponibilidad" className="form-label">Fecha de consulta</label>
            <input
              id="fecha-disponibilidad"
              type="date"
              className="form-input"
              value={fecha}
              min={today}
              onChange={(e) => { setFecha(e.target.value); setSelected(null); }}
              style={{ maxWidth: 220 }}
            />
          </div>

          {isLoading && <div className="loading-state"><div className="spinner" /><p>Buscando horarios...</p></div>}

          {!isLoading && slots.length === 0 && (
            <div className="empty-state">
              <div className="empty-state-icon"><Clock size={32} /></div>
              <p style={{ fontWeight: 600 }}>Sin horarios disponibles</p>
              <p style={{ fontSize: '0.875rem' }}>Intenta con otra fecha.</p>
            </div>
          )}

          {!isLoading && slots.length > 0 && (
            <>
              <p className="text-label-md text-muted mb-sm">{slots.length} horario(s) disponible(s)</p>
              <div className="time-slot-grid">
                {slots.map((slot) => (
                  <button
                    key={slot.horaInicio}
                    id={`slot-${slot.horaInicio}`}
                    className={`time-slot${selected === slot.horaInicio ? ' selected' : ''}`}
                    onClick={() => setSelected(slot.horaInicio)}
                  >
                    {slot.horaInicio}
                  </button>
                ))}
              </div>
            </>
          )}

          {selected && (
            <div style={{ marginTop: 'var(--space-lg)', padding: 'var(--space-md)', background: 'var(--color-surface-container-low)', borderRadius: 'var(--radius-md)', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <div>
                <p style={{ fontWeight: 600 }}>Horario seleccionado</p>
                <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.875rem' }}>{fecha} a las {selected}</p>
              </div>
              <button id="btn-continuar-agendar" className="btn btn-primary" onClick={handleAgendar}>
                Continuar <ChevronRight size={16} />
              </button>
            </div>
          )}
        </div>
      </div>
    </AppLayout>
  );
}
