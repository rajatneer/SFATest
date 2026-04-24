import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../../context/AppContext';
import { Search, ChevronRight, CheckCircle, XCircle, SkipForward, MapPin, AlertCircle, ArrowLeft, Route } from 'lucide-react';

type FilterTab = 'all' | 'visited' | 'pending' | 'skipped';

export default function CustomerList() {
  const { currentUser, daySession, selectedRoute, getRouteCustomers, getTodayVisits } = useApp();
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<FilterTab>('all');

  if (!currentUser) return null;

  if (!daySession || daySession.status === 'ended') {
    return (
      <div className="p-4 flex flex-col items-center justify-center min-h-[50vh] gap-4">
        <AlertCircle className="w-12 h-12 text-amber-400" />
        <div className="text-center">
          <p className="text-gray-900 mb-1" style={{ fontWeight: 600 }}>Day Not Started</p>
          <p className="text-gray-500" style={{ fontSize: '0.85rem' }}>Please start your day to access customers.</p>
        </div>
        <button onClick={() => navigate('/rep')} className="bg-blue-600 text-white px-6 py-2.5 rounded-xl" style={{ fontWeight: 600 }}>
          Go to Home
        </button>
      </div>
    );
  }

  if (!selectedRoute) {
    return (
      <div className="p-4 flex flex-col items-center justify-center min-h-[50vh] gap-4">
        <Route className="w-12 h-12 text-blue-400" />
        <div className="text-center">
          <p className="text-gray-900 mb-1" style={{ fontWeight: 600 }}>No Route Selected</p>
          <p className="text-gray-500" style={{ fontSize: '0.85rem' }}>Select a route to view customers.</p>
        </div>
        <button onClick={() => navigate('/rep/route-select')} className="bg-blue-600 text-white px-6 py-2.5 rounded-xl" style={{ fontWeight: 600 }}>
          Select Route
        </button>
      </div>
    );
  }

  const routeCustomers = getRouteCustomers(selectedRoute.route_id);
  const todayVisits = getTodayVisits(currentUser.user_id);

  const getVisit = (customerId: number) =>
    todayVisits.find(v => v.customer_id === customerId);

  const getStatus = (customerId: number) => {
    const v = getVisit(customerId);
    if (!v) return 'pending';
    if (v.visit_status === 'completed') return 'visited';
    if (v.visit_status === 'skipped') return 'skipped';
    return 'active';
  };

  const filtered = routeCustomers.filter(c => {
    const status = getStatus(c.customer_id);
    const matchSearch =
      c.customer_name.toLowerCase().includes(search.toLowerCase()) ||
      c.customer_code.toLowerCase().includes(search.toLowerCase()) ||
      c.locality.toLowerCase().includes(search.toLowerCase());
    const matchFilter =
      filter === 'all' ||
      (filter === 'visited' && status === 'visited') ||
      (filter === 'pending' && (status === 'pending' || status === 'active')) ||
      (filter === 'skipped' && status === 'skipped');
    return matchSearch && matchFilter;
  });

  const visitedCount = routeCustomers.filter(c => getStatus(c.customer_id) === 'visited').length;
  const coverage = Math.round((visitedCount / routeCustomers.length) * 100);

  const statusConfig = {
    visited: { label: 'Visited', color: 'bg-green-500', dot: 'bg-green-500', text: 'text-green-700', bg: 'bg-green-50' },
    skipped: { label: 'Skipped', color: 'bg-gray-400', dot: 'bg-gray-400', text: 'text-gray-600', bg: 'bg-gray-50' },
    pending: { label: 'Pending', color: 'bg-orange-400', dot: 'bg-orange-400', text: 'text-orange-700', bg: 'bg-orange-50' },
    active: { label: 'Active', color: 'bg-blue-500', dot: 'bg-blue-500', text: 'text-blue-700', bg: 'bg-blue-50' },
  };

  return (
    <div className="pb-6">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-4 pt-4 pb-3">
        <div className="flex items-center gap-3 mb-3">
          <button onClick={() => navigate('/rep')} className="w-8 h-8 bg-gray-100 rounded-xl flex items-center justify-center">
            <ArrowLeft className="w-4 h-4 text-gray-600" />
          </button>
          <div className="flex-1">
            <h2 className="text-gray-900" style={{ fontWeight: 700, fontSize: '1rem' }}>{selectedRoute.route_name}</h2>
            <p className="text-gray-500" style={{ fontSize: '0.75rem' }}>{routeCustomers.length} customers · {visitedCount} visited</p>
          </div>
          <div className="text-right">
            <p className="text-blue-600" style={{ fontWeight: 700, fontSize: '1.1rem' }}>{coverage}%</p>
            <p className="text-gray-400" style={{ fontSize: '0.65rem' }}>coverage</p>
          </div>
        </div>

        {/* Progress bar */}
        <div className="h-2 bg-gray-100 rounded-full overflow-hidden mb-3">
          <div className="h-full bg-gradient-to-r from-blue-500 to-blue-600 rounded-full transition-all" style={{ width: `${coverage}%` }} />
        </div>

        {/* Search */}
        <div className="relative mb-3">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Search customers…"
            value={search}
            onChange={e => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
            style={{ fontSize: '0.875rem' }}
          />
        </div>

        {/* Filter tabs */}
        <div className="flex gap-1.5">
          {(['all', 'pending', 'visited', 'skipped'] as FilterTab[]).map(tab => (
            <button key={tab} onClick={() => setFilter(tab)}
              className={`px-3 py-1.5 rounded-lg capitalize transition ${filter === tab ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}
              style={{ fontSize: '0.75rem', fontWeight: 500 }}>
              {tab}
            </button>
          ))}
        </div>
      </div>

      {/* Customer list */}
      <div className="px-4 pt-3 space-y-2">
        {filtered.map(customer => {
          const status = getStatus(customer.customer_id);
          const visit = getVisit(customer.customer_id);
          const sc = statusConfig[status as keyof typeof statusConfig];
          const hasMissingCoords = !customer.latitude || !customer.longitude;

          return (
            <button
              key={customer.customer_id}
              onClick={() => navigate(`/rep/customers/${customer.customer_id}`)}
              className="w-full text-left bg-white rounded-2xl border border-gray-200 p-4 shadow-sm hover:border-blue-200 transition active:scale-98"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1 flex-wrap">
                    <span className="text-gray-900 truncate" style={{ fontWeight: 600, fontSize: '0.9rem' }}>{customer.customer_name}</span>
                    <span className={`px-2 py-0.5 rounded-full ${sc.bg} ${sc.text} flex-shrink-0`} style={{ fontSize: '0.65rem', fontWeight: 600 }}>
                      {sc.label}
                    </span>
                    {hasMissingCoords && (
                      <span className="bg-amber-50 text-amber-700 border border-amber-200 px-1.5 py-0.5 rounded-full flex items-center gap-1" style={{ fontSize: '0.62rem', fontWeight: 600 }}>
                        <MapPin className="w-2.5 h-2.5" />
                        No GPS
                      </span>
                    )}
                  </div>
                  <p className="text-gray-500 truncate" style={{ fontSize: '0.78rem' }}>
                    {customer.locality}, {customer.city}
                  </p>
                  <div className="flex items-center gap-3 mt-1.5">
                    <span className="bg-gray-100 text-gray-500 px-2 py-0.5 rounded-md" style={{ fontSize: '0.7rem' }}>{customer.customer_code}</span>
                    <span className="bg-blue-50 text-blue-600 px-2 py-0.5 rounded-md" style={{ fontSize: '0.7rem' }}>{customer.outlet_type}</span>
                    {visit?.has_order && (
                      <span className="bg-green-50 text-green-600 px-2 py-0.5 rounded-md flex items-center gap-1" style={{ fontSize: '0.7rem' }}>
                        📦 Order placed
                      </span>
                    )}
                  </div>
                </div>
                <ChevronRight className="w-5 h-5 text-gray-400 ml-2 mt-0.5 flex-shrink-0" />
              </div>
            </button>
          );
        })}

        {filtered.length === 0 && (
          <div className="text-center py-12">
            <p className="text-gray-400" style={{ fontSize: '0.9rem' }}>No customers match your filter</p>
          </div>
        )}
      </div>
    </div>
  );
}
