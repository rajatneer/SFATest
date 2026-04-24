import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { Eye, EyeOff, ShoppingBag, TrendingUp, Users, Truck } from 'lucide-react';

const demoLogins = [
  { label: 'Admin', username: 'admin', password: 'admin123', icon: Users, color: 'bg-violet-100 text-violet-700 border-violet-200' },
  { label: 'TSI', username: 'tsi01', password: 'tsi123', icon: TrendingUp, color: 'bg-blue-100 text-blue-700 border-blue-200' },
  { label: 'Sales Rep', username: 'rep01', password: 'rep123', icon: ShoppingBag, color: 'bg-green-100 text-green-700 border-green-200' },
  { label: 'Distributor', username: 'dist01', password: 'dist123', icon: Truck, color: 'bg-orange-100 text-orange-700 border-orange-200' },
];

export default function Login() {
  const { login } = useApp();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    await new Promise(r => setTimeout(r, 600));
    const result = login(username, password);
    setLoading(false);
    if (!result.success) {
      setError(result.error || 'Login failed');
      return;
    }
    navigate('/');
  };

  const handleDemoLogin = async (uname: string, pwd: string) => {
    setUsername(uname);
    setPassword(pwd);
    setError('');
    setLoading(true);
    await new Promise(r => setTimeout(r, 400));
    login(uname, pwd);
    setLoading(false);
    navigate('/');
  };

  return (
    <div className="min-h-screen flex">
      {/* Left panel */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-blue-900 via-blue-800 to-blue-700 flex-col justify-between p-12 relative overflow-hidden">
        <div className="absolute inset-0 opacity-10">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="absolute rounded-full border border-white/30"
              style={{ width: `${(i + 1) * 120}px`, height: `${(i + 1) * 120}px`, top: '50%', left: '50%', transform: 'translate(-50%, -50%)' }} />
          ))}
        </div>
        <div className="relative z-10">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center">
              <ShoppingBag className="w-5 h-5 text-white" />
            </div>
            <span className="text-white text-xl" style={{ fontWeight: 600 }}>FieldForce SFA</span>
          </div>
          <p className="text-blue-200 text-sm">FMCG Sales Force Automation</p>
        </div>
        <div className="relative z-10">
          <h1 className="text-white mb-6" style={{ fontSize: '2.25rem', lineHeight: '1.2' }}>
            Empower your field<br />sales team
          </h1>
          <div className="space-y-4">
            {[
              { icon: '📍', title: 'GPS-Verified Check-ins', desc: 'Geo-validated customer visits with tolerance control' },
              { icon: '📦', title: 'Offline-First Orders', desc: 'Capture orders even without network. Sync when online.' },
              { icon: '📊', title: 'Real-Time Analytics', desc: 'Route coverage, strike rates, and distributor tracking' },
            ].map(f => (
              <div key={f.title} className="flex gap-3">
                <span className="text-2xl">{f.icon}</span>
                <div>
                  <p className="text-white text-sm" style={{ fontWeight: 600 }}>{f.title}</p>
                  <p className="text-blue-200" style={{ fontSize: '0.8rem' }}>{f.desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
        <div className="relative z-10 text-blue-300" style={{ fontSize: '0.75rem' }}>
          © 2026 FieldForce SFA. All rights reserved.
        </div>
      </div>

      {/* Right panel */}
      <div className="flex-1 flex items-center justify-center p-6 bg-gray-50">
        <div className="w-full max-w-md">
          {/* Mobile logo */}
          <div className="lg:hidden flex items-center gap-3 mb-8 justify-center">
            <div className="w-10 h-10 bg-blue-600 rounded-xl flex items-center justify-center">
              <ShoppingBag className="w-5 h-5 text-white" />
            </div>
            <span className="text-gray-900 text-xl" style={{ fontWeight: 700 }}>FieldForce SFA</span>
          </div>

          <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-8">
            <h2 className="text-gray-900 mb-1" style={{ fontSize: '1.5rem' }}>Welcome back</h2>
            <p className="text-gray-500 mb-6" style={{ fontSize: '0.875rem' }}>Sign in to your account</p>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: '0.875rem' }}>Username</label>
                <input
                  type="text"
                  value={username}
                  onChange={e => setUsername(e.target.value)}
                  placeholder="Enter your username"
                  className="w-full px-4 py-3 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
                  style={{ fontSize: '0.875rem' }}
                />
              </div>
              <div>
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: '0.875rem' }}>Password</label>
                <div className="relative">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    placeholder="Enter your password"
                    className="w-full px-4 py-3 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition pr-12"
                    style={{ fontSize: '0.875rem' }}
                  />
                  <button type="button" onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                    {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                  </button>
                </div>
              </div>

              {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3" style={{ fontSize: '0.875rem' }}>
                  {error}
                </div>
              )}

              <button type="submit" disabled={loading}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 rounded-xl transition disabled:opacity-60 disabled:cursor-not-allowed"
                style={{ fontWeight: 600 }}>
                {loading ? 'Signing in…' : 'Sign In'}
              </button>

              <p className="text-center text-gray-400" style={{ fontSize: '0.8rem' }}>
                Forgot password? Contact your administrator.
              </p>
            </form>

            <div className="mt-6">
              <div className="flex items-center gap-3 mb-3">
                <div className="flex-1 h-px bg-gray-100" />
                <span className="text-gray-400" style={{ fontSize: '0.75rem' }}>Quick demo login</span>
                <div className="flex-1 h-px bg-gray-100" />
              </div>
              <div className="grid grid-cols-2 gap-2">
                {demoLogins.map(d => {
                  const Icon = d.icon;
                  return (
                    <button key={d.label} onClick={() => handleDemoLogin(d.username, d.password)}
                      className={`flex items-center gap-2 px-3 py-2.5 rounded-xl border transition hover:scale-[1.02] active:scale-100 ${d.color}`}
                      style={{ fontSize: '0.8rem', fontWeight: 500 }}>
                      <Icon className="w-4 h-4" />
                      {d.label}
                    </button>
                  );
                })}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}