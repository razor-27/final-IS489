import { useState, useRef, useEffect } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import {
  Heart, Calendar, Search, User, LayoutDashboard,
  Stethoscope, Settings, ChevronDown, LogOut, Menu, X,
  Clock, BookOpen,
} from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { getInitials } from '../lib/utils';

interface NavItem {
  to: string;
  label: string;
  icon: React.ReactNode;
}

function getPacienteNav(): NavItem[] {
  return [
    { to: '/paciente/inicio', label: 'Inicio', icon: <LayoutDashboard size={18} /> },
    { to: '/paciente/buscar', label: 'Buscar Médico', icon: <Search size={18} /> },
    { to: '/paciente/citas', label: 'Mis Citas', icon: <Calendar size={18} /> },
    { to: '/paciente/perfil', label: 'Mi Perfil', icon: <User size={18} /> },
  ];
}

function getMedicoNav(): NavItem[] {
  return [
    { to: '/medico/agenda', label: 'Mi Agenda', icon: <Calendar size={18} /> },
    { to: '/medico/horario', label: 'Mi Horario', icon: <Clock size={18} /> },
  ];
}

function getAdminNav(): NavItem[] {
  return [
    { to: '/admin/dashboard', label: 'Dashboard', icon: <LayoutDashboard size={18} /> },
    { to: '/admin/medicos', label: 'Médicos', icon: <Stethoscope size={18} /> },
    { to: '/admin/especialidades', label: 'Especialidades', icon: <BookOpen size={18} /> },
    { to: '/admin/configuracion-clinica', label: 'Configuración', icon: <Settings size={18} /> },
  ];
}

export default function AppLayout({ children }: { children?: React.ReactNode }) {
  const { user, role, logout } = useAuth();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const navItems = role === 'Paciente' ? getPacienteNav()
    : role === 'Medico' ? getMedicoNav()
    : getAdminNav();

  const displayName = user?.nombre ?? user?.email ?? 'Usuario';

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  function handleLogout() {
    logout();
    navigate('/login', { replace: true, state: null });
  }

  return (
    <>
      <nav className="navbar" id="main-navbar">
        <div className="navbar-inner">
          {/* Brand */}
          <Link to="/" className="navbar-brand">
            <div className="brand-icon">
              <Heart size={20} fill="currentColor" />
            </div>
            WariSalud
          </Link>

          {/* Desktop nav */}
          <div className="navbar-nav" id="desktop-nav">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
              >
                {item.icon}
                {item.label}
              </NavLink>
            ))}
          </div>

          {/* Actions */}
          <div className="navbar-actions">
            {/* Mobile menu toggle */}
            <button
              className="btn btn-ghost btn-sm"
              onClick={() => setMobileNavOpen(!mobileNavOpen)}
              style={{ display: 'none' }}
              id="mobile-menu-btn"
              aria-label="Menú de navegación"
            >
              {mobileNavOpen ? <X size={20} /> : <Menu size={20} />}
            </button>

            {/* User menu */}
            <div className="user-menu" ref={menuRef}>
              <button
                id="user-menu-btn"
                className="btn btn-ghost"
                onClick={() => setMenuOpen(!menuOpen)}
                style={{ gap: 8, padding: '0 12px', height: 40 }}
                aria-haspopup="true"
                aria-expanded={menuOpen}
              >
                <div className="avatar" style={{ width: 32, height: 32, fontSize: '0.75rem' }}>
                  {getInitials(displayName)}
                </div>
                <span style={{ fontSize: '0.875rem', fontWeight: 500, maxWidth: 120, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                  {displayName}
                </span>
                <ChevronDown size={14} style={{ transition: 'transform 200ms', transform: menuOpen ? 'rotate(180deg)' : '' }} />
              </button>

              {menuOpen && (
                <div className="user-menu-dropdown" role="menu">
                  <div style={{ padding: '8px 12px 8px', borderBottom: '1px solid var(--color-outline-variant)', marginBottom: 4 }}>
                    <div style={{ fontSize: '0.875rem', fontWeight: 600, color: 'var(--color-on-surface)' }}>{displayName}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--color-on-surface-variant)' }}>{user?.email}</div>
                    <div className="chip chip-primary" style={{ marginTop: 4 }}>{role}</div>
                  </div>
                  <button className="user-menu-item" role="menuitem" onClick={() => { setMenuOpen(false); navigate(role === 'Paciente' ? '/paciente/perfil' : '/'); }}>
                    <User size={16} /> Mi Perfil
                  </button>
                  <button className="user-menu-item danger" role="menuitem" onClick={handleLogout}>
                    <LogOut size={16} /> Cerrar Sesión
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      </nav>

      {/* Mobile nav drawer */}
      {mobileNavOpen && (
        <div style={{ background: 'var(--color-surface-container-lowest)', borderBottom: '1px solid var(--color-outline-variant)', padding: '8px 16px 16px' }}>
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}
              onClick={() => setMobileNavOpen(false)}
            >
              {item.icon}
              {item.label}
            </NavLink>
          ))}
        </div>
      )}

      {/* Page content */}
      <div className="app-layout">
        {/* Sidebar (desktop) */}
        <aside className="sidebar" id="sidebar">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}
            >
              {item.icon}
              {item.label}
            </NavLink>
          ))}
        </aside>

        <main className="main-content" id="main-content">
          {children}
        </main>
      </div>
    </>
  );
}
