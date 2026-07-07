import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Save, Clock, Calendar } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import { useToast } from '../../context/ToastContext';
import api from '../../lib/api';
import type { ConfiguracionClinica } from '../../types';
import { getApiErrorMessage } from '../../lib/utils';

const schema = z.object({
  horaApertura: z.string().regex(/^\d{2}:\d{2}$/, 'Formato HH:mm'),
  horaCierre: z.string().regex(/^\d{2}:\d{2}$/, 'Formato HH:mm'),
  diasLaborables: z.string().min(2, 'Ingresa los días laborables'),
});
type FormValues = z.infer<typeof schema>;

export default function ConfiguracionClinicaPage() {
  const toast = useToast();
  const qc = useQueryClient();

  const { data: config, isLoading } = useQuery<ConfiguracionClinica>({
    queryKey: ['config-clinica'],
    queryFn: async () => (await api.get<ConfiguracionClinica>('/api/configuracion-clinica')).data,
  });

  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    values: config ? { horaApertura: config.horaApertura, horaCierre: config.horaCierre, diasLaborables: config.diasLaborables } : undefined,
  });

  const saveMutation = useMutation({
    mutationFn: (data: FormValues) => api.put('/api/configuracion-clinica', data),
    onSuccess: () => {
      toast.success('Configuración guardada exitosamente.');
      qc.invalidateQueries({ queryKey: ['config-clinica'] });
    },
    onError: (err) => toast.error(getApiErrorMessage(err)),
  });

  return (
    <AppLayout>
      <div style={{ maxWidth: 480, margin: '0 auto' }}>
        <div className="page-header">
          <h1>Configuración de la Clínica</h1>
          <p>Horario global de atención</p>
        </div>

        {isLoading && <div className="loading-state"><div className="spinner spinner-lg" /></div>}

        {!isLoading && (
          <div className="card">
            <form onSubmit={handleSubmit((d) => saveMutation.mutate(d))} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-lg)' }}>
              <div>
                <div className="flex items-center gap-sm mb-md">
                  <Clock size={18} style={{ color: 'var(--color-primary)' }} />
                  <h2 style={{ fontWeight: 600, fontSize: '1rem' }}>Horario de Atención</h2>
                </div>
                <div className="grid-2" style={{ gap: 'var(--space-md)' }}>
                  <div className="form-group">
                    <label htmlFor="hora-apertura" className="form-label">Hora de apertura</label>
                    <input id="hora-apertura" type="time" className={`form-input${errors.horaApertura ? ' error' : ''}`} {...register('horaApertura')} />
                    {errors.horaApertura && <span className="form-error">{errors.horaApertura.message}</span>}
                  </div>
                  <div className="form-group">
                    <label htmlFor="hora-cierre" className="form-label">Hora de cierre</label>
                    <input id="hora-cierre" type="time" className={`form-input${errors.horaCierre ? ' error' : ''}`} {...register('horaCierre')} />
                    {errors.horaCierre && <span className="form-error">{errors.horaCierre.message}</span>}
                  </div>
                </div>
              </div>

              <div className="form-group">
                <div className="flex items-center gap-sm mb-sm">
                  <Calendar size={18} style={{ color: 'var(--color-primary)' }} />
                  <label htmlFor="dias-laborables" className="form-label" style={{ marginBottom: 0 }}>Días laborables</label>
                </div>
                <input id="dias-laborables" type="text" className={`form-input${errors.diasLaborables ? ' error' : ''}`} placeholder="Ej. Lu-Sa" {...register('diasLaborables')} />
                {errors.diasLaborables && <span className="form-error">{errors.diasLaborables.message}</span>}
                <p style={{ fontSize: '0.75rem', color: 'var(--color-on-surface-variant)', marginTop: 'var(--space-xs)' }}>Ejemplo: Lu-Vi, Lu-Sa, Lu-Do</p>
              </div>

              <button id="save-config-btn" type="submit" className="btn btn-primary" disabled={saveMutation.isPending}>
                {saveMutation.isPending ? <span className="spinner" style={{ width: 20, height: 20, borderWidth: 2 }} /> : <Save size={18} />}
                {saveMutation.isPending ? 'Guardando...' : 'Guardar Configuración'}
              </button>
            </form>
          </div>
        )}
      </div>
    </AppLayout>
  );
}
