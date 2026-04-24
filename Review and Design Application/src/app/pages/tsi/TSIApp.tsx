import React, { useState } from 'react';
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import {
  LayoutDashboard, Users, Route, Package, TrendingUp,
  CheckCircle, AlertCircle, Clock, ArrowUp, ArrowDown
} from 'lucide-react';
import { repPerformanceData, weeklyOrderStats, routes, territories, initialOrders, customers } from '../../data/mockData';

type Tab = 'dashboard' | 'reps' | 'coverage' | 'orders';

const PIE_COLORS = ['#22c55e', '#f97316', '#94a3b8'];

export default function TSIApp() {
  const [tab, setTab] = useState<Tab>('dashboard');

  const totalReps = repPerformanceData.length;
  const activeReps = repPerformanceData.filter(r => r.day_started).length;
  const avgCoverage = Math.round(repPerformanceData.reduce((s, r) => s + r.coverage_pct, 0) / totalReps);
  const todayOrders = repPerformanceData.reduce((s, r) => s + r.orders_today, 0);
  const strikeRate = Math.round(
    (repPerformanceData.reduce((s, r) => s + r.orders_today, 0) /
      Math.max(repPerformanceData.reduce((s, r) => s + r.visited_today, 0), 1)) * 100
  );

  const visitStatusData = [
    { name: 'Completed', value: repPerformanceData.reduce((s, r) => s + r.visited_today, 0) },
    { name: 'Pending', value: repPerformanceData.reduce((s, r) => s + Math.max(r.customers_total - r.visited_today, 0), 0) },
    { name: 'Skipped', value: 1 },
  ];

  const tabs: { id: Tab; label: string; icon: React.ReactNode }[] = [
    { id: 'dashboard', label: 'Dashboard', icon: <LayoutDashboard className="w-4 h-4" /> },
    { id: 'reps', label: 'Rep Performance', icon: <Users className="w-4 h-4" /> },
    { id: 'coverage', label: 'Route Coverage', icon: <Route className="w-4 h-4" /> },
    { id: 'orders', label: 'Orders', icon: <Package className="w-4 h-4" /> },
  ];

  return (
    <div className="p-6">
      {/* Tab bar */}
      <div className="flex gap-1 mb-6 bg-gray-100 rounded-xl p-1 w-fit">
        {tabs.map(t => (
          <button key={t.id} onClick={() => setTab(t.id)}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg transition ${tab === t.id ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
            style={{ fontSize: '0.85rem', fontWeight: 500 }}>
            {t.icon}
            {t.label}
          </button>
        ))}
      </div>

      {/* Dashboard */}
      {tab === 'dashboard' && (
        <div className="space-y-6">
          {/* KPI cards */}
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            {[
              { label: 'Active Reps Today', value: `${activeReps}/${totalReps}`, sub: `${totalReps - activeReps} not started`, icon: <Users className="w-5 h-5 text-blue-600" />, color: 'bg-blue-50 border-blue-100', trend: null },
              { label: 'Avg Coverage', value: `${avgCoverage}%`, sub: 'Of route customers', icon: <Route className="w-5 h-5 text-green-600" />, color: 'bg-green-50 border-green-100', trend: '+8%' },
              { label: "Today's Orders", value: todayOrders.toString(), sub: 'Across all reps', icon: <Package className="w-5 h-5 text-orange-600" />, color: 'bg-orange-50 border-orange-100', trend: '+3' },
              { label: 'Strike Rate', value: `${strikeRate}%`, sub: 'Orders / Visits', icon: <TrendingUp className="w-5 h-5 text-purple-600" />, color: 'bg-purple-50 border-purple-100', trend: null },
            ].map(kpi => (
              <div key={kpi.label} className={`bg-white rounded-xl border ${kpi.color.split(' ')[1]} p-4 shadow-sm`}>
                <div className="flex items-center justify-between mb-2">
                  <div className={`w-9 h-9 rounded-lg flex items-center justify-center ${kpi.color}`}>
                    {kpi.icon}
                  </div>
                  {kpi.trend && (
                    <span className="text-green-600 flex items-center gap-0.5" style={{ fontSize: '0.75rem', fontWeight: 600 }}>
                      <ArrowUp className="w-3 h-3" />{kpi.trend}
                    </span>
                  )}
                </div>
                <p className="text-gray-900" style={{ fontSize: '1.6rem', fontWeight: 700, lineHeight: 1 }}>{kpi.value}</p>
                <p className="text-gray-500 mt-1" style={{ fontSize: '0.78rem' }}>{kpi.label}</p>
                <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>{kpi.sub}</p>
              </div>
            ))}
          </div>

          {/* Charts row */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
            {/* Weekly orders line chart */}
            <div className="lg:col-span-2 bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
              <p className="text-gray-700 mb-4" style={{ fontWeight: 600 }}>Weekly Orders Trend</p>
              <ResponsiveContainer width="100%" height={200}>
                <LineChart data={weeklyOrderStats}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip formatter={(v: any) => [v, 'Orders']} />
                  <Line type="monotone" dataKey="orders" stroke="#3b82f6" strokeWidth={2} dot={{ fill: '#3b82f6', r: 3 }} activeDot={{ r: 5 }} />
                </LineChart>
              </ResponsiveContainer>
            </div>

            {/* Visit status pie */}
            <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
              <p className="text-gray-700 mb-4" style={{ fontWeight: 600 }}>Today's Visit Status</p>
              <ResponsiveContainer width="100%" height={140}>
                <PieChart>
                  <Pie data={visitStatusData} cx="50%" cy="50%" innerRadius={35} outerRadius={60} dataKey="value">
                    {visitStatusData.map((_, i) => (
                      <Cell key={i} fill={PIE_COLORS[i]} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
              <div className="space-y-1.5 mt-2">
                {visitStatusData.map((item, i) => (
                  <div key={item.name} className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <div className="w-2.5 h-2.5 rounded-full" style={{ backgroundColor: PIE_COLORS[i] }} />
                      <span className="text-gray-600" style={{ fontSize: '0.78rem' }}>{item.name}</span>
                    </div>
                    <span className="text-gray-900" style={{ fontSize: '0.78rem', fontWeight: 600 }}>{item.value}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Rep coverage bar chart */}
          <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
            <p className="text-gray-700 mb-4" style={{ fontWeight: 600 }}>Rep-wise Route Coverage (%)</p>
            <ResponsiveContainer width="100%" height={180}>
              <BarChart data={repPerformanceData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="rep_name" tick={{ fontSize: 11 }} />
                <YAxis domain={[0, 100]} unit="%" tick={{ fontSize: 11 }} />
                <Tooltip formatter={(v: any) => [`${v}%`, 'Coverage']} />
                <Bar dataKey="coverage_pct" fill="#3b82f6" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Missed customers alert */}
          <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <p className="text-gray-700" style={{ fontWeight: 600 }}>Missed / At-Risk Customers</p>
              <span className="bg-red-100 text-red-700 px-2 py-0.5 rounded-full" style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                {repPerformanceData.reduce((s, r) => s + Math.max(r.customers_total - r.visited_today, 0), 0)} customers
              </span>
            </div>
            <div className="space-y-2">
              {repPerformanceData.filter(r => r.customers_total - r.visited_today > 0).map(rep => (
                <div key={rep.rep_id} className="flex items-center justify-between py-2 border-b border-gray-50 last:border-0">
                  <div className="flex items-center gap-2">
                    <div className="w-7 h-7 bg-gray-100 rounded-full flex items-center justify-center" style={{ fontSize: '0.72rem', fontWeight: 700 }}>
                      {rep.rep_name.split(' ')[0][0]}{rep.rep_name.split(' ')[1]?.[0] || ''}
                    </div>
                    <div>
                      <p className="text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>{rep.rep_name}</p>
                      <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>{rep.routes}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-red-600" style={{ fontSize: '0.85rem', fontWeight: 600 }}>{rep.customers_total - rep.visited_today} pending</p>
                    {!rep.day_started && (
                      <span className="text-amber-600" style={{ fontSize: '0.7rem' }}>Not started</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Rep Performance */}
      {tab === 'reps' && (
        <div className="space-y-4">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100">
              <p className="text-gray-700" style={{ fontWeight: 600 }}>Rep Performance – April 24, 2026</p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="bg-gray-50">
                    {['Rep', 'Routes', 'Customers', 'Visited', 'Orders', 'Coverage', 'Strike Rate', 'Day Status'].map(h => (
                      <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600, whiteSpace: 'nowrap' }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {repPerformanceData.map(rep => (
                    <tr key={rep.rep_id} className="border-t border-gray-50 hover:bg-gray-50">
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <div className="w-7 h-7 bg-blue-100 rounded-full flex items-center justify-center text-blue-700" style={{ fontSize: '0.7rem', fontWeight: 700 }}>
                            {rep.rep_name.split(' ').map(n => n[0]).join('')}
                          </div>
                          <span className="text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>{rep.rep_name}</span>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{rep.routes}</td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{rep.customers_total}</td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{rep.visited_today}</td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{rep.orders_today}</td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <div className="flex-1 h-1.5 bg-gray-100 rounded-full w-16">
                            <div className="h-full bg-blue-500 rounded-full" style={{ width: `${rep.coverage_pct}%` }} />
                          </div>
                          <span style={{ fontSize: '0.82rem', fontWeight: 600, color: rep.coverage_pct >= 80 ? '#16a34a' : rep.coverage_pct >= 50 ? '#ca8a04' : '#dc2626' }}>
                            {rep.coverage_pct}%
                          </span>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <span style={{ fontSize: '0.82rem', fontWeight: 600, color: rep.strike_rate >= 60 ? '#16a34a' : '#ca8a04' }}>
                          {rep.strike_rate}%
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <span className={`px-2 py-0.5 rounded-full ${rep.day_started ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                          {rep.day_started ? '🟢 Active' : '⚪ Not Started'}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pending end-day */}
          <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 flex items-start gap-3">
            <Clock className="w-5 h-5 text-amber-500 flex-shrink-0 mt-0.5" />
            <div>
              <p className="text-amber-800" style={{ fontWeight: 600, fontSize: '0.9rem' }}>Pending Day End</p>
              <p className="text-amber-700 mt-0.5" style={{ fontSize: '0.82rem' }}>
                {repPerformanceData.filter(r => r.day_started).length} rep(s) have active sessions and haven't ended their day.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Route Coverage */}
      {tab === 'coverage' && (
        <div className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {routes.map(route => {
              const territory = territories.find(t => t.territory_id === route.territory_id);
              const rep = repPerformanceData.find(r => r.routes.includes(route.route_code));
              const routeCustomers = customers.filter(c => c.route_id === route.route_id);
              const missingCoords = routeCustomers.filter(c => !c.latitude).length;
              const coverage = rep?.coverage_pct || 0;

              return (
                <div key={route.route_id} className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
                  <div className="flex items-start justify-between mb-3">
                    <div>
                      <p className="text-gray-900" style={{ fontWeight: 600, fontSize: '0.95rem' }}>{route.route_name}</p>
                      <p className="text-gray-400" style={{ fontSize: '0.75rem' }}>{route.route_code} · {territory?.territory_name}</p>
                    </div>
                    <span className={`px-2 py-0.5 rounded-full ${coverage >= 80 ? 'bg-green-100 text-green-700' : coverage >= 40 ? 'bg-amber-100 text-amber-700' : 'bg-red-100 text-red-700'}`} style={{ fontSize: '0.72rem', fontWeight: 700 }}>
                      {coverage}%
                    </span>
                  </div>
                  <div className="h-2 bg-gray-100 rounded-full mb-3">
                    <div className="h-full rounded-full transition-all" style={{ width: `${coverage}%`, backgroundColor: coverage >= 80 ? '#22c55e' : coverage >= 40 ? '#f59e0b' : '#ef4444' }} />
                  </div>
                  <div className="flex gap-3 text-sm">
                    <div>
                      <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>Customers</p>
                      <p className="text-gray-700" style={{ fontWeight: 600, fontSize: '0.85rem' }}>{routeCustomers.length}</p>
                    </div>
                    <div>
                      <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>Visited</p>
                      <p className="text-gray-700" style={{ fontWeight: 600, fontSize: '0.85rem' }}>{rep?.visited_today || 0}</p>
                    </div>
                    {missingCoords > 0 && (
                      <div>
                        <p className="text-amber-500" style={{ fontSize: '0.72rem' }}>No GPS</p>
                        <p className="text-amber-600" style={{ fontWeight: 600, fontSize: '0.85rem' }}>{missingCoords}</p>
                      </div>
                    )}
                    <div>
                      <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>Assigned Rep</p>
                      <p className="text-gray-700" style={{ fontWeight: 600, fontSize: '0.85rem' }}>{rep?.rep_name || '—'}</p>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Orders */}
      {tab === 'orders' && (
        <div className="space-y-4">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-2">
            {[
              { label: 'Today', value: '3', color: 'text-blue-600' },
              { label: 'Synced', value: '2', color: 'text-green-600' },
              { label: 'Accepted', value: '1', color: 'text-purple-600' },
              { label: 'Total Value', value: '₹5,576', color: 'text-orange-600' },
            ].map(s => (
              <div key={s.label} className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
                <p className={s.color} style={{ fontSize: '1.4rem', fontWeight: 700 }}>{s.value}</p>
                <p className="text-gray-500" style={{ fontSize: '0.78rem' }}>{s.label}</p>
              </div>
            ))}
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100">
              <p className="text-gray-700" style={{ fontWeight: 600 }}>Recent Orders</p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="bg-gray-50">
                    {['Order No.', 'Date', 'Customer', 'Route', 'Amount', 'Status'].map(h => (
                      <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {initialOrders.slice(0, 8).map(order => {
                    const customer = customers.find(c => c.customer_id === order.customer_id);
                    const route = routes.find(r => r.route_id === order.route_id);
                    const statusColors: Record<string, string> = {
                      Created: 'bg-blue-100 text-blue-700',
                      Synced: 'bg-indigo-100 text-indigo-700',
                      Accepted: 'bg-purple-100 text-purple-700',
                      Dispatched: 'bg-amber-100 text-amber-700',
                      Delivered: 'bg-green-100 text-green-700',
                      Cancelled: 'bg-red-100 text-red-700',
                    };
                    return (
                      <tr key={order.order_id} className="border-t border-gray-50 hover:bg-gray-50">
                        <td className="px-4 py-3 text-blue-600" style={{ fontSize: '0.82rem', fontWeight: 500 }}>{order.order_number}</td>
                        <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{order.order_date}</td>
                        <td className="px-4 py-3 text-gray-700" style={{ fontSize: '0.82rem' }}>{customer?.customer_name || '—'}</td>
                        <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{route?.route_code || '—'}</td>
                        <td className="px-4 py-3 text-gray-900" style={{ fontSize: '0.82rem', fontWeight: 600 }}>₹{order.net_amount.toLocaleString()}</td>
                        <td className="px-4 py-3">
                          <span className={`px-2 py-0.5 rounded-full ${statusColors[order.status] || 'bg-gray-100 text-gray-600'}`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                            {order.status}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
