import { useQuery } from '@tanstack/react-query';
import { User, Mail, Phone, Shield } from 'lucide-react';
import AppLayout from '../../components/AppLayout';
import { useAuth } from '../../context/AuthContext';
import api from '../../lib/api';
import type { PerfilUsuario } from '../../types';
import { getInitials } from '../../lib/utils';

export default function PerfilPage() {
  const { user } = useAuth();

  const { data: perfil, isLoading } = useQuery<PerfilUsuario>({
    queryKey: ['perfil'],
    queryFn: async () => (await api.get<PerfilUsuario>('/api/usuarios/me')).data,
  });

  const nombre = perfil?.nombreCompleto ?? user?.nombre ?? user?.email ?? 'Usuario';

  return (
    <AppLayout>
      <div style={{ maxWidth: 560, margin: '0 auto' }}>
        <div className="page-header">
          <h1>Mi Perfil</h1>
          <p>Información de tu cuenta</p>
        </div>

        {isLoading && <div className="loading-state"><div className="spinner spinner-lg" /></div>}

        {!isLoading && (
          <>
            <div className="card mb-lg" style={{ textAlign: 'center', padding: 'var(--space-xl)' }}>
              <div className="avatar" style={{ width: 80, height: 80, fontSize: '1.5rem', margin: '0 auto var(--space-md)' }}>
                {getInitials(nombre)}
              </div>
              <h2 style={{ fontWeight: 700, fontSize: '1.25rem' }}>{nombre}</h2>
              <p style={{ color: 'var(--color-on-surface-variant)', marginTop: 4 }}>{perfil?.email ?? user?.email}</p>
              <div className="chip chip-primary" style={{ marginTop: 'var(--space-sm)' }}>{user?.role}</div>
            </div>

            <div className="card">
              <h2 style={{ fontWeight: 600, marginBottom: 'var(--space-lg)', fontSize: '1rem' }}>Datos de la cuenta</h2>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
                <div className="flex items-center gap-md">
                  <div className="stat-card-icon" style={{ width: 40, height: 40 }}><User size={18} /></div>
                  <div>
                    <p className="text-label-sm text-muted">Nombre completo</p>
                    <p style={{ fontWeight: 500 }}>{nombre}</p>
                  </div>
                </div>
                <div className="flex items-center gap-md">
                  <div className="stat-card-icon" style={{ width: 40, height: 40 }}><Mail size={18} /></div>
                  <div>
                    <p className="text-label-sm text-muted">Correo electrónico</p>
                    <p style={{ fontWeight: 500 }}>{perfil?.email ?? user?.email}</p>
                  </div>
                </div>
                {perfil?.telefono && (
                  <div className="flex items-center gap-md">
                    <div className="stat-card-icon" style={{ width: 40, height: 40 }}><Phone size={18} /></div>
                    <div>
                      <p className="text-label-sm text-muted">Teléfono</p>
                      <p style={{ fontWeight: 500 }}>{perfil.telefono}</p>
                    </div>
                  </div>
                )}
                <div className="flex items-center gap-md">
                  <div className="stat-card-icon" style={{ width: 40, height: 40 }}><Shield size={18} /></div>
                  <div>
                    <p className="text-label-sm text-muted">Rol</p>
                    <p style={{ fontWeight: 500 }}>{user?.role}</p>
                  </div>
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </AppLayout>
  );
}
