import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { Heart, Eye, EyeOff } from 'lucide-react';
import { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { getApiErrorMessage } from '../../lib/utils';
import api from '../../lib/api';
import type { AuthResponse } from '../../types';

const schema = z.object({
  nombreCompleto: z.string().min(3, 'Ingresa tu nombre completo'),
  email: z.string().email('Email inválido'),
  telefono: z.string().min(9, 'Teléfono inválido'),
  password: z.string().min(6, 'Mínimo 6 caracteres'),
  confirmarPassword: z.string(),
}).refine((d) => d.password === d.confirmarPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmarPassword'],
});
type FormValues = z.infer<typeof schema>;

export default function RegisterPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();
  const [showPass, setShowPass] = useState(false);
  const [loading, setLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const onSubmit = async (data: FormValues) => {
    setLoading(true);
    try {
      const payload = { email: data.email, password: data.password, nombreCompleto: data.nombreCompleto, telefono: data.telefono };
      const res = await api.post<AuthResponse>('/api/auth/register', payload);
      login(res.data.token);
      toast.success('¡Cuenta creada con éxito!');
      navigate('/paciente/inicio', { replace: true });
    } catch (err) {
      toast.error(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card" style={{ maxWidth: 480 }}>
        <div className="auth-header">
          <div className="auth-logo">
            <Heart size={28} fill="currentColor" />
          </div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--color-on-surface)', marginBottom: 'var(--space-xs)' }}>Crear cuenta</h1>
          <p style={{ color: 'var(--color-on-surface-variant)', fontSize: '0.9375rem' }}>Regístrate como paciente en WariSalud</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} noValidate style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
          <div className="form-group">
            <label htmlFor="reg-nombre" className="form-label">Nombre completo</label>
            <input id="reg-nombre" type="text" className={`form-input${errors.nombreCompleto ? ' error' : ''}`} placeholder="Nombre Apellido" autoComplete="name" {...register('nombreCompleto')} />
            {errors.nombreCompleto && <span className="form-error">{errors.nombreCompleto.message}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="reg-email" className="form-label">Correo electrónico</label>
            <input id="reg-email" type="email" className={`form-input${errors.email ? ' error' : ''}`} placeholder="tu@email.com" autoComplete="email" {...register('email')} />
            {errors.email && <span className="form-error">{errors.email.message}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="reg-telefono" className="form-label">Teléfono</label>
            <input id="reg-telefono" type="tel" className={`form-input${errors.telefono ? ' error' : ''}`} placeholder="999 999 999" autoComplete="tel" {...register('telefono')} />
            {errors.telefono && <span className="form-error">{errors.telefono.message}</span>}
          </div>

          <div className="grid-2" style={{ gap: 'var(--space-md)' }}>
            <div className="form-group">
              <label htmlFor="reg-password" className="form-label">Contraseña</label>
              <div style={{ position: 'relative' }}>
                <input id="reg-password" type={showPass ? 'text' : 'password'} className={`form-input${errors.password ? ' error' : ''}`} placeholder="••••••" style={{ paddingRight: 44 }} {...register('password')} />
                <button type="button" onClick={() => setShowPass(!showPass)} style={{ position: 'absolute', right: 12, top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-outline)', display: 'flex' }} aria-label="Toggle">
                  {showPass ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
              {errors.password && <span className="form-error">{errors.password.message}</span>}
            </div>
            <div className="form-group">
              <label htmlFor="reg-confirm" className="form-label">Confirmar</label>
              <input id="reg-confirm" type="password" className={`form-input${errors.confirmarPassword ? ' error' : ''}`} placeholder="••••••" {...register('confirmarPassword')} />
              {errors.confirmarPassword && <span className="form-error">{errors.confirmarPassword.message}</span>}
            </div>
          </div>

          <button id="register-submit" type="submit" className="btn btn-primary w-full" disabled={loading} style={{ marginTop: 'var(--space-sm)' }}>
            {loading ? <span className="spinner" style={{ width: 20, height: 20, borderWidth: 2 }} /> : null}
            {loading ? 'Creando cuenta...' : 'Crear Cuenta'}
          </button>
        </form>

        <div className="divider" />
        <p className="text-center" style={{ fontSize: '0.875rem', color: 'var(--color-on-surface-variant)' }}>
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" id="go-to-login" style={{ color: 'var(--color-primary)', fontWeight: 600 }}>Inicia sesión</Link>
        </p>
      </div>
    </div>
  );
}
