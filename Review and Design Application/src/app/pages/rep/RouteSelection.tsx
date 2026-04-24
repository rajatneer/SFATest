import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../../context/AppContext';
import { Search, MapPin, Package, Users, ChevronRight, ArrowLeft } from 'lucide-react';
import { territories, distributors, customers } from '../../data/mockData';

export default function RouteSelection() {
  const { currentUser, daySession, selectedRoute, selectRoute, getAssignedRoutes } = useApp();
  const navigate = useNavigate();
  const [search, setSearch] = useState('');

  if (!currentUser) return null;

  const assignedRoutes = getAssignedRoutes(currentUser.user_id);
  const filtered = assignedRoutes.filter(r =>
    r.route_name.toLowerCase().includes(search.toLowerCase()) ||
    r.route_code.toLowerCase().includes(search.toLowerCase())
  );

  const handleSelect = (route: typeof assignedRoutes[0]) => {
    selectRoute(route);
    navigate('/rep/customers');
  };

  return (
    <div className="p-4 pb-6">
      {/* Header */}
      <div className="flex items-center gap-3 mb-4">
        <button onClick={() => navigate(-1)} className="w-8 h-8 bg-white rounded-xl border border-gray-200 flex items-center justify-center">
          <ArrowLeft className="w-4 h-4 text-gray-600" />
        </button>
        <div>
          <h2 className="text-gray-900" style={{ fontWeight: 700, fontSize: '1.1rem' }}>Select Route</h2>
          <p className="text-gray-500" style={{ fontSize: '0.78rem' }}>{assignedRoutes.length} routes assigned to you</p>
        </div>
      </div>

      {!daySession && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-3 mb-4 flex items-start gap-2">
          <span className="text-amber-500 mt-0.5">⚠️</span>
          <p className="text-amber-700" style={{ fontSize: '0.8rem' }}>You must start your day before selecting a route for field activities.</p>
        </div>
      )}

      {/* Search */}
      <div className="relative mb-4">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
        <input
          type="text"
          placeholder="Search route name or code…"
          value={search}
          onChange={e => setSearch(e.target.value)}
          className="w-full pl-9 pr-4 py-3 bg-white border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
          style={{ fontSize: '0.875rem' }}
        />
      </div>

      {/* Route list */}
      <div className="space-y-3">
        {filtered.map(route => {
          const territory = territories.find(t => t.territory_id === route.territory_id);
          const distributor = distributors.find(d => d.distributor_id === route.distributor_id);
          const customerCount = customers.filter(c => c.route_id === route.route_id && c.is_active).length;
          const isActive = selectedRoute?.route_id === route.route_id;

          return (
            <button
              key={route.route_id}
              onClick={() => handleSelect(route)}
              className={`w-full text-left bg-white rounded-2xl border p-4 shadow-sm transition active:scale-98 ${isActive ? 'border-blue-400 ring-2 ring-blue-100' : 'border-gray-200 hover:border-blue-200'}`}
            >
              <div className="flex items-start justify-between mb-3">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-0.5">
                    <span className="text-gray-900" style={{ fontWeight: 700, fontSize: '0.95rem' }}>{route.route_name}</span>
                    {isActive && (
                      <span className="bg-blue-600 text-white px-2 py-0.5 rounded-full" style={{ fontSize: '0.65rem', fontWeight: 600 }}>
                        Active
                      </span>
                    )}
                  </div>
                  <span className="bg-gray-100 text-gray-500 px-2 py-0.5 rounded-md" style={{ fontSize: '0.72rem', fontWeight: 500 }}>
                    {route.route_code}
                  </span>
                </div>
                <ChevronRight className="w-5 h-5 text-gray-400 mt-0.5" />
              </div>

              <div className="grid grid-cols-3 gap-2">
                <div className="flex items-center gap-1.5">
                  <MapPin className="w-3.5 h-3.5 text-blue-500 flex-shrink-0" />
                  <span className="text-gray-600 truncate" style={{ fontSize: '0.75rem' }}>{territory?.territory_name || '—'}</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <Package className="w-3.5 h-3.5 text-orange-500 flex-shrink-0" />
                  <span className="text-gray-600 truncate" style={{ fontSize: '0.75rem' }}>{distributor?.distributor_name.split(' ')[0] || '—'}</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <Users className="w-3.5 h-3.5 text-green-500 flex-shrink-0" />
                  <span className="text-gray-600" style={{ fontSize: '0.75rem' }}>{customerCount} customers</span>
                </div>
              </div>
            </button>
          );
        })}

        {filtered.length === 0 && (
          <div className="text-center py-12">
            <p className="text-gray-400" style={{ fontSize: '0.9rem' }}>No routes found</p>
          </div>
        )}
      </div>
    </div>
  );
}
