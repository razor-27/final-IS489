import { Routes, Route, Navigate } from 'react-router-dom';
import ProtectedRoute from './components/ProtectedRoute';
import { NotFoundPage, ForbiddenPage } from './components/ErrorPages';

// Auth
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';

// Paciente
import InicioPage from './pages/paciente/InicioPage';
import BuscarPage from './pages/paciente/BuscarPage';
import DisponibilidadPage from './pages/paciente/DisponibilidadPage';
import AgendarPage from './pages/paciente/AgendarPage';
import CitasPage from './pages/paciente/CitasPage';
import CitaDetallePage from './pages/paciente/CitaDetallePage';
import PerfilPage from './pages/paciente/PerfilPage';

// Medico
import AgendaPage from './pages/medico/AgendaPage';
import HorarioPage from './pages/medico/HorarioPage';

// Admin
import DashboardPage from './pages/admin/DashboardPage';
import MedicosAdminPage from './pages/admin/MedicosAdminPage';
import EspecialidadesAdminPage from './pages/admin/EspecialidadesAdminPage';
import ConfiguracionClinicaPage from './pages/admin/ConfiguracionClinicaPage';

import { useAuth } from './context/AuthContext';

function HomeRedirect() {
  const { role, isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div className="loading-state" style={{ minHeight: '100vh' }}>
        <div className="spinner spinner-lg" />
        <p>Cargando WariSalud...</p>
      </div>
    );
  }

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role === 'Paciente') return <Navigate to="/paciente/inicio" replace />;
  if (role === 'Medico') return <Navigate to="/medico/agenda" replace />;
  if (role === 'Admin') return <Navigate to="/admin/dashboard" replace />;
  return <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <Routes>
      {/* Public */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/registro" element={<RegisterPage />} />

      {/* Root redirect */}
      <Route path="/" element={<HomeRedirect />} />

      {/* Paciente */}
      <Route element={<ProtectedRoute allowedRoles={['Paciente']} />}>
        <Route path="/paciente/inicio" element={<InicioPage />} />
        <Route path="/paciente/buscar" element={<BuscarPage />} />
        <Route path="/paciente/medicos/:id/disponibilidad" element={<DisponibilidadPage />} />
        <Route path="/paciente/agendar" element={<AgendarPage />} />
        <Route path="/paciente/citas" element={<CitasPage />} />
        <Route path="/paciente/citas/:id" element={<CitaDetallePage />} />
        <Route path="/paciente/perfil" element={<PerfilPage />} />
      </Route>

      {/* Medico */}
      <Route element={<ProtectedRoute allowedRoles={['Medico']} />}>
        <Route path="/medico/agenda" element={<AgendaPage />} />
        <Route path="/medico/horario" element={<HorarioPage />} />
      </Route>

      {/* Admin */}
      <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
        <Route path="/admin/dashboard" element={<DashboardPage />} />
        <Route path="/admin/medicos" element={<MedicosAdminPage />} />
        <Route path="/admin/especialidades" element={<EspecialidadesAdminPage />} />
        <Route path="/admin/configuracion-clinica" element={<ConfiguracionClinicaPage />} />
      </Route>

      {/* Error pages */}
      <Route path="/403" element={<ForbiddenPage />} />
      <Route path="/404" element={<NotFoundPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
