import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, X, Save, BookOpen } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import AppLayout from '../../components/AppLayout';
import { useToast } from '../../context/ToastContext';
import api from '../../lib/api';
import type { Especialidad } from '../../types';
import { getApiErrorMessage } from '../../lib/utils';

const schema = z.object({
  nombre: z.string().min(2, 'Nombre requerido'),
  descripcion: z.string().min(5, 'Descripción requerida'),
  duracionCitaMinutos: z.number({ message: 'Ingresa un número' }).min(10).max(120),
});
type FormValues = z.infer<typeof schema>;

export default function EspecialidadesAdminPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Especialidad | null>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);

  const { data: especialidades = [], isLoading } = useQuery<Especialidad[]>({
    queryKey: ['especialidades'],
    queryFn: async () => (await api.get<Especialidad[]>('/api/especialidades')).data,
  });

  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  function openCreate() { setEditing(null); reset({ duracionCitaMinutos: 30 }); setShowForm(true); }
  function openEdit(e: Especialidad) { setEditing(e); reset({ nombre: e.nombre, descripcion: e.descripcion, duracionCitaMinutos: e.duracionCitaMinutos }); setShowForm(true); }

  const saveMutation = useMutation({
    mutationFn: async (data: FormValues) => editing
      ? api.put(`/api/especialidades/${editing.id}`, data)
      : api.post('/api/especialidades', data),
    onSuccess: () => {
      toast.success(editing ? 'Especialidad actualizada.' : 'Especialidad creada.');
      qc.invalidateQueries({ queryKey: ['especialidades'] });
      setShowForm(false);
    },
    onError: (err) => toast.error(getApiErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/especialidades/${id}`),
    onSuccess: () => {
      toast.success('Especialidad eliminada.');
      qc.invalidateQueries({ queryKey: ['especialidades'] });
      setDeleteId(null);
    },
    onError: (err) => toast.error(getApiErrorMessage(err)),
  });

  return (
    <AppLayout>
      <div>
        <div className="flex items-center justify-between mb-lg">
          <div className="page-header" style={{ marginBottom: 0 }}>
            <h1>Especialidades</h1>
            <p>Gestiona las especialidades médicas</p>
          </div>
          <button id="btn-nueva-especialidad" className="btn btn-primary btn-sm" onClick={openCreate}>
            <Plus size={16} /> Nueva Especialidad
          </button>
        </div>

        {isLoading && <div className="loading-state"><div className="spinner spinner-lg" /></div>}

        {!isLoading && especialidades.length === 0 && (
          <div className="empty-state">
            <div className="empty-state-icon"><BookOpen size={32} /></div>
            <p style={{ fontWeight: 600 }}>Sin especialidades</p>
          </div>
        )}

        <div className="grid-3" style={{ alignItems: 'start' }}>
          {especialidades.map((esp) => (
            <div key={esp.id} id={`esp-${esp.id}`} className="card">
              <div className="flex items-center justify-between mb-md">
                <div className="stat-card-icon" style={{ marginBottom: 0 }}><BookOpen size={20} /></div>
                <div className="flex gap-xs">
                  <button id={`edit-esp-${esp.id}`} className="btn btn-ghost btn-sm" onClick={() => openEdit(esp)} title="Editar">
                    <Pencil size={15} />
                  </button>
                  <button id={`delete-esp-${esp.id}`} className="btn btn-ghost btn-sm" onClick={() => setDeleteId(esp.id)} style={{ color: 'var(--color-error)' }} title="Eliminar">
                    <Trash2 size={15} />
                  </button>
                </div>
              </div>
              <h3 style={{ fontWeight: 700, fontSize: '1rem', marginBottom: 'var(--space-xs)' }}>{esp.nombre}</h3>
              <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.8125rem', marginBottom: 'var(--space-sm)' }}>{esp.descripcion}</p>
              <span className="chip chip-primary">{esp.duracionCitaMinutos} min por cita</span>
            </div>
          ))}
        </div>
      </div>

      {showForm && (
        <div className="modal-backdrop" role="dialog" aria-modal="true">
          <div className="modal">
            <div className="flex items-center justify-between mb-lg">
              <h2 style={{ fontWeight: 700, fontSize: '1.125rem' }}>{editing ? 'Editar Especialidad' : 'Nueva Especialidad'}</h2>
              <button className="btn btn-ghost btn-sm" onClick={() => setShowForm(false)}><X size={18} /></button>
            </div>
            <form onSubmit={handleSubmit((d) => saveMutation.mutate(d))} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
              <div className="form-group">
                <label htmlFor="esp-nombre" className="form-label">Nombre</label>
                <input id="esp-nombre" type="text" className={`form-input${errors.nombre ? ' error' : ''}`} placeholder="Ej. Cardiología" {...register('nombre')} />
                {errors.nombre && <span className="form-error">{errors.nombre.message}</span>}
              </div>
              <div className="form-group">
                <label htmlFor="esp-descripcion" className="form-label">Descripción</label>
                <textarea id="esp-descripcion" className={`form-input${errors.descripcion ? ' error' : ''}`} rows={3} style={{ height: 'auto', paddingTop: 12, paddingBottom: 12, resize: 'vertical' }} placeholder="Describe la especialidad..." {...register('descripcion')} />
                {errors.descripcion && <span className="form-error">{errors.descripcion.message}</span>}
              </div>
              <div className="form-group">
                <label htmlFor="esp-duracion" className="form-label">Duración de cita (minutos)</label>
                <input id="esp-duracion" type="number" min={10} max={120} step={5} className={`form-input${errors.duracionCitaMinutos ? ' error' : ''}`} {...register('duracionCitaMinutos', { valueAsNumber: true })} />
                {errors.duracionCitaMinutos && <span className="form-error">{errors.duracionCitaMinutos.message}</span>}
              </div>
              <div className="flex gap-md">
                <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)}>Cancelar</button>
                <button id="save-esp-btn" type="submit" className="btn btn-primary" disabled={saveMutation.isPending}>
                  {saveMutation.isPending ? <span className="spinner" style={{ width: 18, height: 18, borderWidth: 2 }} /> : <Save size={16} />}
                  {editing ? 'Guardar' : 'Crear'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {deleteId !== null && (
        <div className="modal-backdrop" role="dialog" aria-modal="true">
          <div className="modal">
            <h2 style={{ fontWeight: 700, marginBottom: 'var(--space-sm)' }}>Eliminar especialidad</h2>
            <p style={{ color: 'var(--color-on-surface-variant)', marginBottom: 'var(--space-lg)' }}>Esta acción no se puede deshacer.</p>
            <div className="flex gap-md">
              <button className="btn btn-secondary" onClick={() => setDeleteId(null)}>Cancelar</button>
              <button id="confirm-delete-esp" className="btn btn-danger" onClick={() => deleteMutation.mutate(deleteId!)} disabled={deleteMutation.isPending}>
                {deleteMutation.isPending ? <span className="spinner" style={{ width: 18, height: 18, borderWidth: 2 }} /> : <Trash2 size={16} />}
                Eliminar
              </button>
            </div>
          </div>
        </div>
      )}
    </AppLayout>
  );
}
