import React from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router';
import { Home, Users, RefreshCw, User, ShoppingBag, Wifi, WifiOff } from 'lucide-react';
import { useApp } from '../context/AppContext';

export default function MobileLayout() {
  const { currentUser, syncQueue, daySession } = useApp();
  const pendingCount = syncQueue.filter(i => i.status === 'pending').length;

  if (!currentUser) return null;

  const navItems = [
    { to: '/rep', label: 'Home', icon: Home, end: true },
    { to: '/rep/customers', label: 'Customers', icon: Users },
    { to: '/rep/sync', label: 'Sync', icon: RefreshCw, badge: pendingCount },
    { to: '/rep/profile', label: 'Profile', icon: User },
  ];

  return (
    <div className="flex flex-col h-screen bg-gray-50 max-w-md mx-auto relative">
      {/* Top Header */}
      <header className="bg-blue-700 text-white px-4 py-3 flex items-center justify-between flex-shrink-0">
        <div className="flex items-center gap-2">
          <ShoppingBag className="w-5 h-5" />
          <span style={{ fontWeight: 700, fontSize: '1rem' }}>FieldForce</span>
        </div>
        <div className="flex items-center gap-3">
          {pendingCount > 0 ? (
            <div className="flex items-center gap-1 bg-orange-400 text-white px-2 py-0.5 rounded-full" style={{ fontSize: '0.7rem', fontWeight: 600 }}>
              <WifiOff className="w-3 h-3" />
              {pendingCount} pending
            </div>
          ) : (
            <div className="flex items-center gap-1 bg-green-500/80 text-white px-2 py-0.5 rounded-full" style={{ fontSize: '0.7rem', fontWeight: 600 }}>
              <Wifi className="w-3 h-3" />
              Synced
            </div>
          )}
          <div className="w-8 h-8 bg-white/20 rounded-full flex items-center justify-center" style={{ fontSize: '0.75rem', fontWeight: 700 }}>
            {currentUser.full_name.split(' ').map(n => n[0]).join('').slice(0, 2)}
          </div>
        </div>
      </header>

      {/* Day session banner */}
      {daySession && daySession.status === 'started' && (
        <div className="bg-green-600 text-white px-4 py-1.5 flex items-center justify-between flex-shrink-0">
          <span style={{ fontSize: '0.75rem' }}>🟢 Day Active — Fri, Apr 24, 2026</span>
          <span className="bg-white/20 px-2 py-0.5 rounded-full" style={{ fontSize: '0.7rem' }}>
            {new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
          </span>
        </div>
      )}

      {/* Main content */}
      <main className="flex-1 overflow-y-auto">
        <Outlet />
      </main>

      {/* Bottom Navigation */}
      <nav className="bg-white border-t border-gray-200 flex-shrink-0">
        <div className="flex">
          {navItems.map(item => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                className={({ isActive }) =>
                  `flex-1 flex flex-col items-center py-2.5 gap-1 relative transition-colors ${isActive ? 'text-blue-600' : 'text-gray-500'}`
                }
              >
                <div className="relative">
                  <Icon className="w-5 h-5" />
                  {item.badge ? (
                    <span className="absolute -top-1.5 -right-1.5 w-4 h-4 bg-orange-500 text-white rounded-full flex items-center justify-center" style={{ fontSize: '0.6rem', fontWeight: 700 }}>
                      {item.badge > 9 ? '9+' : item.badge}
                    </span>
                  ) : null}
                </div>
                <span style={{ fontSize: '0.65rem', fontWeight: 500 }}>{item.label}</span>
              </NavLink>
            );
          })}
        </div>
      </nav>
    </div>
  );
}
