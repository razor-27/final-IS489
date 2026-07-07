import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Search, Filter } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import api from '../../lib/api';
import type { Medico, Especialidad } from '../../types';
import { getInitials } from '../../lib/utils';

export default function BuscarPage() {
  const [query, setQuery] = useState('');
  const [especialidadId, setEspecialidadId] = useState<number | ''>('');
  const navigate = useNavigate();

  const { data: especialidades = [] } = useQuery<Especialidad[]>({
    queryKey: ['especialidades'],
    queryFn: async () => (await api.get<Especialidad[]>('/api/especialidades')).data,
  });

  const { data: medicos = [], isLoading } = useQuery<Medico[]>({
    queryKey: ['medicos', especialidadId],
    queryFn: async () => {
      const params = especialidadId ? { especialidadId } : {};
      return (await api.get<Medico[]>('/api/medicos', { params })).data;
    },
  });

  const filtered = medicos.filter((m) =>
    m.nombreCompleto.toLowerCase().includes(query.toLowerCase()) ||
    m.especialidad?.nombre.toLowerCase().includes(query.toLowerCase()),
  );

  return (
    <AppLayout>
      <div>
        <div className="page-header">
          <h1>Buscar Médico</h1>
          <p>Encuentra el especialista que necesitas</p>
        </div>

        {/* Filters */}
        <div className="card mb-lg">
          <div className="grid-2" style={{ gap: 'var(--space-md)' }}>
            <div className="form-group">
              <label htmlFor="buscar-nombre" className="form-label">Nombre del médico</label>
              <div style={{ position: 'relative' }}>
                <input
                  id="buscar-nombre"
                  type="search"
                  className="form-input"
                  placeholder="Buscar por nombre..."
                  value={query}
                  onChange={(e) => setQuery(e.target.value)}
                  style={{ paddingLeft: 40 }}
                />
                <Search size={18} style={{ position: 'absolute', left: 12, top: '50%', transform: 'translateY(-50%)', color: 'var(--color-outline)' }} />
              </div>
            </div>
            <div className="form-group">
              <label htmlFor="buscar-especialidad" className="form-label">Especialidad</label>
              <select
                id="buscar-especialidad"
                className="form-select"
                value={especialidadId}
                onChange={(e) => setEspecialidadId(e.target.value ? Number(e.target.value) : '')}
              >
                <option value="">Todas las especialidades</option>
                {especialidades.map((e) => (
                  <option key={e.id} value={e.id}>{e.nombre}</option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {/* Results */}
        <div style={{ marginBottom: 'var(--space-sm)', color: 'var(--color-on-surface-variant)', fontSize: '0.875rem' }}>
          <Filter size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          {filtered.length} médico(s) encontrado(s)
        </div>

        {isLoading && <div className="loading-state"><div className="spinner spinner-lg" /><p>Buscando médicos...</p></div>}

        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
          {!isLoading && filtered.map((medico) => (
            <div
              key={medico.id}
              id={`medico-${medico.id}`}
              className="doctor-card"
              onClick={() => navigate(`/paciente/medicos/${medico.id}/disponibilidad`)}
              role="button"
              tabIndex={0}
              onKeyDown={(e) => e.key === 'Enter' && navigate(`/paciente/medicos/${medico.id}/disponibilidad`)}
            >
              <div className="doctor-avatar">{getInitials(medico.nombreCompleto)}</div>
              <div style={{ flex: 1 }}>
                <p style={{ fontWeight: 600, fontSize: '1rem', marginBottom: 4 }}>{medico.nombreCompleto}</p>
                {medico.especialidad && (
                  <span className="chip chip-primary">{medico.especialidad.nombre}</span>
                )}
                <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.8125rem', marginTop: 'var(--space-sm)' }}>
                  Colegiatura: {medico.numeroColegiatura}
                </p>
                {medico.especialidad && (
                  <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.8125rem', marginTop: 2 }}>
                    {medico.especialidad.descripcion}
                  </p>
                )}
              </div>
              <div style={{ color: 'var(--color-primary)', fontSize: '0.875rem', fontWeight: 600, whiteSpace: 'nowrap' }}>Ver disponibilidad →</div>
            </div>
          ))}
          {!isLoading && filtered.length === 0 && (
            <div className="empty-state">
              <div className="empty-state-icon"><Search size={32} /></div>
              <p style={{ fontWeight: 600 }}>No se encontraron médicos</p>
              <p style={{ fontSize: '0.875rem' }}>Prueba con otros filtros.</p>
            </div>
          )}
        </div>
      </div>
    </AppLayout>
  );
}
