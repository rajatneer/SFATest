import React, { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router';
import { ShoppingBag, LogOut, ChevronDown, Bell } from 'lucide-react';
import { useApp } from '../context/AppContext';

interface NavItem {
  to: string;
  label: string;
  icon: React.ReactNode;
}

interface DesktopLayoutProps {
  navItems: NavItem[];
  title: string;
}

export default function DesktopLayout({ navItems, title }: DesktopLayoutProps) {
  const { currentUser, logout } = useApp();
  const navigate = useNavigate();
  const [showUserMenu, setShowUserMenu] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className="w-60 bg-slate-900 flex flex-col flex-shrink-0">
        {/* Logo */}
        <div className="p-5 border-b border-slate-700">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 bg-blue-500 rounded-lg flex items-center justify-center">
              <ShoppingBag className="w-4 h-4 text-white" />
            </div>
            <div>
              <p className="text-white" style={{ fontWeight: 700, fontSize: '0.9rem', lineHeight: 1.2 }}>FieldForce</p>
              <p className="text-slate-400" style={{ fontSize: '0.7rem' }}>SFA Platform</p>
            </div>
          </div>
        </div>

        {/* Role badge */}
        <div className="px-4 py-3">
          <div className="bg-slate-800 rounded-lg px-3 py-2">
            <p className="text-slate-400" style={{ fontSize: '0.65rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Role</p>
            <p className="text-white" style={{ fontSize: '0.8rem', fontWeight: 600 }}>{currentUser?.role_name}</p>
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-3 py-2 space-y-0.5 overflow-y-auto">
          {navItems.map(item => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors ${isActive
                  ? 'bg-blue-600 text-white'
                  : 'text-slate-400 hover:text-white hover:bg-slate-800'
                }`
              }
            >
              {item.icon}
              <span style={{ fontSize: '0.85rem', fontWeight: 500 }}>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        {/* User section */}
        <div className="p-4 border-t border-slate-700">
          <div className="relative">
            <button onClick={() => setShowUserMenu(!showUserMenu)}
              className="w-full flex items-center gap-3 hover:bg-slate-800 rounded-lg p-2 transition-colors">
              <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white flex-shrink-0" style={{ fontSize: '0.75rem', fontWeight: 700 }}>
                {currentUser?.full_name.split(' ').map(n => n[0]).join('').slice(0, 2)}
              </div>
              <div className="flex-1 text-left overflow-hidden">
                <p className="text-white truncate" style={{ fontSize: '0.8rem', fontWeight: 600 }}>{currentUser?.full_name}</p>
                <p className="text-slate-400 truncate" style={{ fontSize: '0.7rem' }}>{currentUser?.username}</p>
              </div>
              <ChevronDown className="w-4 h-4 text-slate-400 flex-shrink-0" />
            </button>
            {showUserMenu && (
              <div className="absolute bottom-full left-0 right-0 mb-1 bg-slate-800 rounded-lg shadow-lg overflow-hidden border border-slate-700">
                <button onClick={handleLogout}
                  className="w-full flex items-center gap-2 px-3 py-2.5 text-red-400 hover:bg-slate-700 transition-colors"
                  style={{ fontSize: '0.85rem' }}>
                  <LogOut className="w-4 h-4" />
                  Sign Out
                </button>
              </div>
            )}
          </div>
        </div>
      </aside>

      {/* Main content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Top header */}
        <header className="bg-white border-b border-gray-200 px-6 py-3.5 flex items-center justify-between flex-shrink-0">
          <h1 className="text-gray-900" style={{ fontSize: '1rem', fontWeight: 600 }}>{title}</h1>
          <div className="flex items-center gap-3">
            <span className="text-gray-400" style={{ fontSize: '0.8rem' }}>Fri, Apr 24, 2026</span>
            <button className="relative w-8 h-8 flex items-center justify-center text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition">
              <Bell className="w-5 h-5" />
              <span className="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full" />
            </button>
          </div>
        </header>

        {/* Content */}
        <main className="flex-1 overflow-y-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
