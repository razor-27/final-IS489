import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { Heart, Eye, EyeOff } from 'lucide-react';
import { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { getApiErrorMessage } from '../../lib/utils';
import { getUserRole } from '../../lib/auth';
import api from '../../lib/api';
import type { AuthResponse } from '../../types';

const schema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(6, 'Mínimo 6 caracteres'),
});
type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();
  const [showPass, setShowPass] = useState(false);
  const [loading, setLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const from = (location.state as { from?: Location })?.from?.pathname ?? null;

  const onSubmit = async (data: FormValues) => {
    setLoading(true);
    try {
      const res = await api.post<AuthResponse>('/api/auth/login', data);
      login(res.data.token);
      toast.success('¡Bienvenido a WariSalud!');

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const r = res.data.rol || (res.data as any).Rol || getUserRole();
      let dest = '/';
      if (r === 'Paciente') {
        dest = (from && from.startsWith('/paciente')) ? from : '/paciente/inicio';
      } else if (r === 'Medico') {
        dest = (from && from.startsWith('/medico')) ? from : '/medico/agenda';
      } else if (r === 'Admin') {
        dest = (from && from.startsWith('/admin')) ? from : '/admin/dashboard';
      }
      navigate(dest, { replace: true });
    } catch (err) {
      toast.error(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-header">
          <div className="auth-logo">
            <Heart size={28} fill="currentColor" />
          </div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--color-on-surface)', marginBottom: 'var(--space-xs)' }}>
            WariSalud
          </h1>
          <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.9375rem' }}>
            Inicia sesión en tu cuenta
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} noValidate style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
          <div className="form-group">
            <label htmlFor="login-email" className="form-label">Correo electrónico</label>
            <input
              id="login-email"
              type="email"
              className={`form-input${errors.email ? ' error' : ''}`}
              placeholder="tu@email.com"
              autoComplete="email"
              {...register('email')}
            />
            {errors.email && <span className="form-error">{errors.email.message}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="login-password" className="form-label">Contraseña</label>
            <div style={{ position: 'relative' }}>
              <input
                id="login-password"
                type={showPass ? 'text' : 'password'}
                className={`form-input${errors.password ? ' error' : ''}`}
                placeholder="••••••••"
                autoComplete="current-password"
                style={{ paddingRight: 44 }}
                {...register('password')}
              />
              <button
                type="button"
                onClick={() => setShowPass(!showPass)}
                style={{ position: 'absolute', right: 12, top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-outline)', display: 'flex' }}
                aria-label={showPass ? 'Ocultar contraseña' : 'Mostrar contraseña'}
              >
                {showPass ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.password && <span className="form-error">{errors.password.message}</span>}
          </div>

          <button
            id="login-submit"
            type="submit"
            className="btn btn-primary w-full"
            disabled={loading}
            style={{ marginTop: 'var(--space-sm)' }}
          >
            {loading ? <span className="spinner" style={{ width: 20, height: 20, borderWidth: 2 }} /> : null}
            {loading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
          </button>
        </form>

        <div className="divider" />
        <p className="text-center" style={{ fontSize: '0.875rem', color: 'var(--color-on-surface-variant)' }}>
          ¿No tienes cuenta?{' '}
          <Link to="/registro" id="go-to-register" style={{ color: 'var(--color-primary)', fontWeight: 600 }}>Regístrate</Link>
        </p>
      </div>
    </div>
  );
}
