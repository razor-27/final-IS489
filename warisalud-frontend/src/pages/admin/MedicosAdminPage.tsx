import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, X, Save, Stethoscope } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import AppLayout from '../../components/AppLayout';
import { useToast } from '../../context/ToastContext';
import api from '../../lib/api';
import type { Medico, Especialidad } from '../../types';
import { getApiErrorMessage, getInitials } from '../../lib/utils';

const schema = z.object({
  nombreCompleto: z.string().min(3, 'Campo requerido'),
  numeroColegiatura: z.string().min(2, 'Campo requerido'),
  especialidadId: z.number({ message: 'Selecciona una especialidad' }),
  email: z.string().email('Email inválido'),
  password: z.string().min(6).optional().or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

export default function MedicosAdminPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Medico | null>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);

  const { data: medicos = [], isLoading } = useQuery<Medico[]>({
    queryKey: ['admin-medicos'],
    queryFn: async () => (await api.get<Medico[]>('/api/medicos')).data,
  });

  const { data: especialidades = [] } = useQuery<Especialidad[]>({
    queryKey: ['especialidades'],
    queryFn: async () => (await api.get<Especialidad[]>('/api/especialidades')).data,
  });

  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: editing ? { nombreCompleto: editing.nombreCompleto, numeroColegiatura: editing.numeroColegiatura, especialidadId: editing.especialidadId } : {},
  });

  function openCreate() { setEditing(null); reset({}); setShowForm(true); }
  function openEdit(m: Medico) { setEditing(m); reset({ nombreCompleto: m.nombreCompleto, numeroColegiatura: m.numeroColegiatura, especialidadId: m.especialidadId }); setShowForm(true); }

  const saveMutation = useMutation({
    mutationFn: async (data: FormValues) => {
      if (editing) {
        return api.put(`/api/medicos/${editing.id}`, data);
      }
      return api.post('/api/medicos', data);
    },
    onSuccess: () => {
      toast.success(editing ? 'Médico actualizado.' : 'Médico creado.');
      qc.invalidateQueries({ queryKey: ['admin-medicos'] });
      setShowForm(false);
    },
    onError: (err) => toast.error(getApiErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/medicos/${id}`),
    onSuccess: () => {
      toast.success('Médico eliminado.');
      qc.invalidateQueries({ queryKey: ['admin-medicos'] });
      setDeleteId(null);
    },
    onError: (err) => toast.error(getApiErrorMessage(err)),
  });

  return (
    <AppLayout>
      <div>
        <div className="flex items-center justify-between mb-lg">
          <div className="page-header" style={{ marginBottom: 0 }}>
            <h1>Médicos</h1>
            <p>Gestiona los médicos de la clínica</p>
          </div>
          <button id="btn-nuevo-medico" className="btn btn-primary btn-sm" onClick={openCreate}>
            <Plus size={16} /> Nuevo Médico
          </button>
        </div>

        {isLoading && <div className="loading-state"><div className="spinner spinner-lg" /></div>}

        {!isLoading && medicos.length === 0 && (
          <div className="empty-state">
            <div className="empty-state-icon"><Stethoscope size={32} /></div>
            <p style={{ fontWeight: 600 }}>Sin médicos</p>
            <p style={{ fontSize: '0.875rem' }}>Crea el primer médico de la clínica.</p>
          </div>
        )}

        {!isLoading && medicos.length > 0 && (
          <div className="table-wrapper">
            <table id="admin-medicos-table">
              <thead>
                <tr>
                  <th>Médico</th>
                  <th>Especialidad</th>
                  <th>Colegiatura</th>
                  <th style={{ textAlign: 'right' }}>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {medicos.map((m) => (
                  <tr key={m.id}>
                    <td>
                      <div className="flex items-center gap-sm">
                        <div className="avatar" style={{ width: 32, height: 32, fontSize: '0.75rem' }}>{getInitials(m.nombreCompleto)}</div>
                        <span style={{ fontWeight: 500 }}>{m.nombreCompleto}</span>
                      </div>
                    </td>
                    <td>{m.especialidad ? <span className="chip chip-primary">{m.especialidad.nombre}</span> : '—'}</td>
                    <td style={{ color: 'var(--color-on-surface-variant)' }}>{m.numeroColegiatura}</td>
                    <td style={{ textAlign: 'right' }}>
                      <div className="flex gap-sm" style={{ justifyContent: 'flex-end' }}>
                        <button id={`edit-medico-${m.id}`} className="btn btn-ghost btn-sm" onClick={() => openEdit(m)} title="Editar">
                          <Pencil size={15} />
                        </button>
                        <button id={`delete-medico-${m.id}`} className="btn btn-ghost btn-sm" onClick={() => setDeleteId(m.id)} style={{ color: 'var(--color-error)' }} title="Eliminar">
                          <Trash2 size={15} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Form Modal */}
      {showForm && (
        <div className="modal-backdrop" role="dialog" aria-modal="true">
          <div className="modal" style={{ maxWidth: 520 }}>
            <div className="flex items-center justify-between mb-lg">
              <h2 style={{ fontWeight: 700, fontSize: '1.125rem' }}>{editing ? 'Editar Médico' : 'Nuevo Médico'}</h2>
              <button className="btn btn-ghost btn-sm" onClick={() => setShowForm(false)}><X size={18} /></button>
            </div>

            <form onSubmit={handleSubmit((d) => saveMutation.mutate(d))} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
              <div className="form-group">
                <label htmlFor="medico-nombre" className="form-label">Nombre completo</label>
                <input id="medico-nombre" type="text" className={`form-input${errors.nombreCompleto ? ' error' : ''}`} {...register('nombreCompleto')} />
                {errors.nombreCompleto && <span className="form-error">{errors.nombreCompleto.message}</span>}
              </div>

              <div className="form-group">
                <label htmlFor="medico-colegiatura" className="form-label">Número de colegiatura</label>
                <input id="medico-colegiatura" type="text" className={`form-input${errors.numeroColegiatura ? ' error' : ''}`} {...register('numeroColegiatura')} />
                {errors.numeroColegiatura && <span className="form-error">{errors.numeroColegiatura.message}</span>}
              </div>

              <div className="form-group">
                <label htmlFor="medico-especialidad" className="form-label">Especialidad</label>
                <select id="medico-especialidad" className={`form-select${errors.especialidadId ? ' error' : ''}`} {...register('especialidadId', { valueAsNumber: true })}>
                  <option value="">Selecciona una especialidad</option>
                  {especialidades.map((e) => <option key={e.id} value={e.id}>{e.nombre}</option>)}
                </select>
                {errors.especialidadId && <span className="form-error">{errors.especialidadId.message}</span>}
              </div>

              {!editing && (
                <>
                  <div className="form-group">
                    <label htmlFor="medico-email" className="form-label">Email de acceso</label>
                    <input id="medico-email" type="email" className={`form-input${errors.email ? ' error' : ''}`} {...register('email')} />
                    {errors.email && <span className="form-error">{errors.email.message}</span>}
                  </div>
                  <div className="form-group">
                    <label htmlFor="medico-password" className="form-label">Contraseña temporal</label>
                    <input id="medico-password" type="password" className="form-input" placeholder="Mínimo 6 caracteres" {...register('password')} />
                  </div>
                </>
              )}

              <div className="flex gap-md" style={{ marginTop: 'var(--space-sm)' }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)}>Cancelar</button>
                <button id="save-medico-btn" type="submit" className="btn btn-primary" disabled={saveMutation.isPending}>
                  {saveMutation.isPending ? <span className="spinner" style={{ width: 18, height: 18, borderWidth: 2 }} /> : <Save size={16} />}
                  {editing ? 'Guardar Cambios' : 'Crear Médico'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete confirm */}
      {deleteId !== null && (
        <div className="modal-backdrop" role="dialog" aria-modal="true">
          <div className="modal">
            <h2 style={{ fontWeight: 700, marginBottom: 'var(--space-sm)' }}>Confirmar eliminación</h2>
            <p style={{ color: 'var(--color-on-surface-variant)', marginBottom: 'var(--space-lg)' }}>Esta acción no se puede deshacer.</p>
            <div className="flex gap-md">
              <button className="btn btn-secondary" onClick={() => setDeleteId(null)}>Cancelar</button>
              <button id="confirm-delete-medico" className="btn btn-danger" onClick={() => deleteMutation.mutate(deleteId!)} disabled={deleteMutation.isPending}>
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
