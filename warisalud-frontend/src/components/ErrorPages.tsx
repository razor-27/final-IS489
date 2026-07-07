import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export function NotFoundPage() {
  return (
    <div className="loading-state" style={{ minHeight: '100vh', gap: 'var(--space-lg)' }}>
      <div style={{ fontSize: '5rem', fontWeight: 700, color: 'var(--color-primary)', lineHeight: 1 }}>404</div>
      <div>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 600, marginBottom: 'var(--space-sm)' }}>Página no encontrada</h1>
        <p className="text-muted">La ruta que buscas no existe.</p>
      </div>
      <Link to="/" className="btn btn-primary">Ir al inicio</Link>
    </div>
  );
}

export function ForbiddenPage() {
  const { role } = useAuth();
  const home = role === 'Paciente' ? '/paciente/inicio'
    : role === 'Medico' ? '/medico/agenda'
    : '/admin/dashboard';
  return (
    <div className="loading-state" style={{ minHeight: '100vh', gap: 'var(--space-lg)' }}>
      <div style={{ fontSize: '5rem', fontWeight: 700, color: 'var(--color-error)', lineHeight: 1 }}>403</div>
      <div>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 600, marginBottom: 'var(--space-sm)' }}>Acceso denegado</h1>
        <p className="text-muted">No tienes permiso para ver esta página.</p>
      </div>
      <Link to={home} className="btn btn-primary">Volver al inicio</Link>
    </div>
  );
}
