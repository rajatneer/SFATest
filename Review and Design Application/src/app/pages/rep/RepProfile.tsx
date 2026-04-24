import React from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../../context/AppContext';
import { LogOut, User, Phone, Mail, Shield } from 'lucide-react';

export default function RepProfile() {
  const { currentUser, logout, daySession } = useApp();
  const navigate = useNavigate();

  if (!currentUser) return null;

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="p-4 pb-6 space-y-4">
      <div className="pt-2">
        <h2 className="text-gray-900" style={{ fontWeight: 700, fontSize: '1.1rem' }}>Profile</h2>
      </div>

      {/* Avatar card */}
      <div className="bg-gradient-to-br from-blue-600 to-blue-700 rounded-2xl p-5 text-white">
        <div className="flex items-center gap-4">
          <div className="w-16 h-16 bg-white/20 rounded-full flex items-center justify-center" style={{ fontSize: '1.5rem', fontWeight: 700 }}>
            {currentUser.full_name.split(' ').map(n => n[0]).join('').slice(0, 2)}
          </div>
          <div>
            <p className="text-white" style={{ fontWeight: 700, fontSize: '1.1rem' }}>{currentUser.full_name}</p>
            <p className="text-blue-200" style={{ fontSize: '0.82rem' }}>{currentUser.role_name}</p>
            <span className="bg-white/20 text-white px-2 py-0.5 rounded-full mt-1 inline-block" style={{ fontSize: '0.7rem' }}>
              @{currentUser.username}
            </span>
          </div>
        </div>
      </div>

      {/* Info */}
      <div className="bg-white rounded-2xl border border-gray-200 p-4 shadow-sm space-y-3">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 bg-gray-100 rounded-lg flex items-center justify-center">
            <Phone className="w-4 h-4 text-gray-500" />
          </div>
          <div>
            <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>Mobile</p>
            <p className="text-gray-800" style={{ fontSize: '0.875rem', fontWeight: 500 }}>{currentUser.mobile_number}</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 bg-gray-100 rounded-lg flex items-center justify-center">
            <Mail className="w-4 h-4 text-gray-500" />
          </div>
          <div>
            <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>Email</p>
            <p className="text-gray-800" style={{ fontSize: '0.875rem', fontWeight: 500 }}>{currentUser.email}</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 bg-gray-100 rounded-lg flex items-center justify-center">
            <Shield className="w-4 h-4 text-gray-500" />
          </div>
          <div>
            <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>Role</p>
            <p className="text-gray-800" style={{ fontSize: '0.875rem', fontWeight: 500 }}>{currentUser.role_name}</p>
          </div>
        </div>
      </div>

      {/* Day session status */}
      {daySession && (
        <div className={`rounded-xl border p-3 flex items-center gap-2 ${daySession.status === 'started' ? 'bg-green-50 border-green-200' : 'bg-gray-50 border-gray-200'}`}>
          <div className={`w-2 h-2 rounded-full ${daySession.status === 'started' ? 'bg-green-500 animate-pulse' : 'bg-gray-400'}`} />
          <div>
            <p className="text-gray-700" style={{ fontSize: '0.82rem', fontWeight: 600 }}>
              Day {daySession.status === 'started' ? 'Active' : 'Ended'}
            </p>
            <p className="text-gray-500" style={{ fontSize: '0.72rem' }}>
              Started: {new Date(daySession.start_day_timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              {daySession.end_day_timestamp && ` · Ended: ${new Date(daySession.end_day_timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`}
            </p>
          </div>
        </div>
      )}

      {/* Logout */}
      <button onClick={handleLogout}
        className="w-full flex items-center justify-center gap-2 bg-red-50 border border-red-200 text-red-600 py-3 rounded-xl hover:bg-red-100 transition"
        style={{ fontWeight: 600 }}>
        <LogOut className="w-4 h-4" />
        Sign Out
      </button>
    </div>
  );
}
