import React, { useState } from 'react';
import {
  Users, MapPin, Route, Package, Truck, Upload, BarChart2,
  Plus, Search, Edit2, ToggleLeft, ToggleRight, X, Check,
  AlertCircle, CheckCircle, Clock, Download
} from 'lucide-react';
import {
  users as initialUsers, territories, routes, distributors, customers, products
} from '../../data/mockData';
import { useApp } from '../../context/AppContext';

type AdminTab = 'users' | 'territories' | 'routes' | 'customers' | 'products' | 'uploads' | 'reports';

interface UserFormState {
  full_name: string;
  username: string;
  email: string;
  mobile_number: string;
  role: string;
}

const uploadJobs = [
  { id: 1, type: 'Customers', file: 'customers_april_2026.csv', uploaded_by: 'Rajesh Kumar', uploaded_at: '2026-04-20 10:30', status: 'Completed', records: 24, errors: 0 },
  { id: 2, type: 'Products', file: 'product_catalog_v3.csv', uploaded_by: 'Rajesh Kumar', uploaded_at: '2026-04-18 14:15', status: 'Completed', records: 15, errors: 0 },
  { id: 3, type: 'Users', file: 'field_reps_march.csv', uploaded_by: 'Rajesh Kumar', uploaded_at: '2026-04-01 09:00', status: 'Completed with errors', records: 12, errors: 2 },
  { id: 4, type: 'Route Assignments', file: 'route_assignments_q2.csv', uploaded_by: 'Rajesh Kumar', uploaded_at: '2026-03-31 16:45', status: 'Completed', records: 8, errors: 0 },
];

export default function AdminApp() {
  const [tab, setTab] = useState<AdminTab>('users');
  const [userSearch, setUserSearch] = useState('');
  const [customerSearch, setCustomerSearch] = useState('');
  const [productSearch, setProductSearch] = useState('');
  const [showAddUser, setShowAddUser] = useState(false);
  const [userForm, setUserForm] = useState<UserFormState>({ full_name: '', username: '', email: '', mobile_number: '', role: 'SALES_REP' });
  const [localUsers, setLocalUsers] = useState(initialUsers);

  const tabs: { id: AdminTab; label: string; icon: React.ReactNode }[] = [
    { id: 'users', label: 'Users', icon: <Users className="w-4 h-4" /> },
    { id: 'territories', label: 'Territories', icon: <MapPin className="w-4 h-4" /> },
    { id: 'routes', label: 'Routes', icon: <Route className="w-4 h-4" /> },
    { id: 'customers', label: 'Customers', icon: <Users className="w-4 h-4" /> },
    { id: 'products', label: 'Products', icon: <Package className="w-4 h-4" /> },
    { id: 'uploads', label: 'Uploads', icon: <Upload className="w-4 h-4" /> },
    { id: 'reports', label: 'Reports', icon: <BarChart2 className="w-4 h-4" /> },
  ];

  const filteredUsers = localUsers.filter(u =>
    u.full_name.toLowerCase().includes(userSearch.toLowerCase()) ||
    u.username.toLowerCase().includes(userSearch.toLowerCase()) ||
    u.role.toLowerCase().includes(userSearch.toLowerCase())
  );

  const filteredCustomers = customers.filter(c =>
    c.customer_name.toLowerCase().includes(customerSearch.toLowerCase()) ||
    c.customer_code.toLowerCase().includes(customerSearch.toLowerCase())
  );

  const filteredProducts = products.filter(p =>
    p.product_name.toLowerCase().includes(productSearch.toLowerCase()) ||
    p.product_code.toLowerCase().includes(productSearch.toLowerCase())
  );

  const toggleUser = (userId: number) => {
    setLocalUsers(prev => prev.map(u => u.user_id === userId ? { ...u, is_active: !u.is_active } : u));
  };

  const handleAddUser = () => {
    if (!userForm.full_name || !userForm.username) return;
    const newUser = {
      ...userForm,
      user_id: 100 + localUsers.length,
      password: 'temp123',
      role: userForm.role as any,
      role_name: userForm.role === 'SALES_REP' ? 'Sales Representative' : userForm.role === 'TSI' ? 'Territory Sales Incharge' : userForm.role,
      is_active: true,
    };
    setLocalUsers(prev => [...prev, newUser]);
    setShowAddUser(false);
    setUserForm({ full_name: '', username: '', email: '', mobile_number: '', role: 'SALES_REP' });
  };

  const roleColors: Record<string, string> = {
    ADMIN: 'bg-violet-100 text-violet-700',
    TSI: 'bg-blue-100 text-blue-700',
    SALES_REP: 'bg-green-100 text-green-700',
    DISTRIBUTOR: 'bg-orange-100 text-orange-700',
  };

  const statusColor = (s: string) =>
    s === 'Completed' ? 'bg-green-100 text-green-700' :
      s === 'Completed with errors' ? 'bg-amber-100 text-amber-700' :
        'bg-blue-100 text-blue-700';

  const missingCoordCustomers = customers.filter(c => !c.latitude || !c.longitude);

  return (
    <div className="p-6">
      {/* Tab bar */}
      <div className="flex gap-0.5 mb-6 bg-gray-100 rounded-xl p-1 flex-wrap">
        {tabs.map(t => (
          <button key={t.id} onClick={() => setTab(t.id)}
            className={`flex items-center gap-1.5 px-3.5 py-2 rounded-lg transition ${tab === t.id ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
            style={{ fontSize: '0.82rem', fontWeight: 500 }}>
            {t.icon}
            {t.label}
          </button>
        ))}
      </div>

      {/* Users */}
      {tab === 'users' && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input type="text" placeholder="Search users…" value={userSearch}
                onChange={e => setUserSearch(e.target.value)}
                className="w-full pl-9 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500"
                style={{ fontSize: '0.875rem' }} />
            </div>
            <button onClick={() => setShowAddUser(true)}
              className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2.5 rounded-xl transition ml-3"
              style={{ fontSize: '0.875rem', fontWeight: 600 }}>
              <Plus className="w-4 h-4" />
              Add User
            </button>
          </div>

          {/* Add user modal */}
          {showAddUser && (
            <div className="bg-white border border-blue-200 rounded-xl p-4 shadow-sm">
              <div className="flex items-center justify-between mb-3">
                <p className="text-gray-800" style={{ fontWeight: 600 }}>New User</p>
                <button onClick={() => setShowAddUser(false)}><X className="w-4 h-4 text-gray-400" /></button>
              </div>
              <div className="grid grid-cols-2 gap-3">
                {[
                  { key: 'full_name', label: 'Full Name', placeholder: 'Enter full name' },
                  { key: 'username', label: 'Username', placeholder: 'Enter username' },
                  { key: 'email', label: 'Email', placeholder: 'Enter email' },
                  { key: 'mobile_number', label: 'Mobile', placeholder: 'Enter mobile' },
                ].map(f => (
                  <div key={f.key}>
                    <label className="block text-gray-600 mb-1" style={{ fontSize: '0.78rem' }}>{f.label}</label>
                    <input type="text" placeholder={f.placeholder}
                      value={(userForm as any)[f.key]}
                      onChange={e => setUserForm(prev => ({ ...prev, [f.key]: e.target.value }))}
                      className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      style={{ fontSize: '0.82rem' }} />
                  </div>
                ))}
                <div>
                  <label className="block text-gray-600 mb-1" style={{ fontSize: '0.78rem' }}>Role</label>
                  <select value={userForm.role} onChange={e => setUserForm(prev => ({ ...prev, role: e.target.value }))}
                    className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    style={{ fontSize: '0.82rem' }}>
                    <option value="SALES_REP">Sales Representative</option>
                    <option value="TSI">Territory Sales Incharge</option>
                    <option value="DISTRIBUTOR">Distributor User</option>
                    <option value="ADMIN">Admin</option>
                  </select>
                </div>
              </div>
              <div className="flex gap-2 mt-3">
                <button onClick={() => setShowAddUser(false)}
                  className="flex-1 border border-gray-200 text-gray-600 py-2 rounded-lg hover:bg-gray-50 transition" style={{ fontSize: '0.875rem' }}>
                  Cancel
                </button>
                <button onClick={handleAddUser}
                  className="flex-1 bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition" style={{ fontSize: '0.875rem', fontWeight: 600 }}>
                  Create User
                </button>
              </div>
            </div>
          )}

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="bg-gray-50">
                  {['User', 'Username', 'Role', 'Mobile', 'Email', 'Status', 'Actions'].map(h => (
                    <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {filteredUsers.map(user => (
                  <tr key={user.user_id} className={`border-t border-gray-50 hover:bg-gray-50 ${!user.is_active ? 'opacity-50' : ''}`}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <div className="w-7 h-7 bg-blue-100 rounded-full flex items-center justify-center text-blue-700 flex-shrink-0" style={{ fontSize: '0.7rem', fontWeight: 700 }}>
                          {user.full_name.split(' ').map(n => n[0]).join('').slice(0, 2)}
                        </div>
                        <span className="text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>{user.full_name}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-gray-500" style={{ fontSize: '0.82rem' }}>{user.username}</td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-0.5 rounded-full ${roleColors[user.role] || 'bg-gray-100 text-gray-600'}`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                        {user.role}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{user.mobile_number}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{user.email}</td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-0.5 rounded-full ${user.is_active ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                        {user.is_active ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <button onClick={() => toggleUser(user.user_id)}
                        className="text-gray-400 hover:text-blue-600 transition">
                        {user.is_active
                          ? <ToggleRight className="w-5 h-5 text-green-500" />
                          : <ToggleLeft className="w-5 h-5 text-gray-400" />}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Territories */}
      {tab === 'territories' && (
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
            <p className="text-gray-700" style={{ fontWeight: 600 }}>Territories ({territories.length})</p>
            <button className="flex items-center gap-1.5 bg-blue-600 text-white px-3 py-1.5 rounded-lg" style={{ fontSize: '0.8rem', fontWeight: 600 }}>
              <Plus className="w-3.5 h-3.5" />Add
            </button>
          </div>
          <table className="w-full">
            <thead><tr className="bg-gray-50">
              {['Code', 'Name', 'TSI Assigned', 'Routes', 'Status'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
              ))}
            </tr></thead>
            <tbody>
              {territories.map(t => {
                const tsi = initialUsers.find(u => u.user_id === t.tsi_user_id);
                const routeCount = routes.filter(r => r.territory_id === t.territory_id).length;
                return (
                  <tr key={t.territory_id} className="border-t border-gray-50 hover:bg-gray-50">
                    <td className="px-4 py-3 text-gray-500 font-mono" style={{ fontSize: '0.82rem' }}>{t.territory_code}</td>
                    <td className="px-4 py-3 text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>{t.territory_name}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{tsi?.full_name || '—'}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{routeCount}</td>
                    <td className="px-4 py-3"><span className="bg-green-100 text-green-700 px-2 py-0.5 rounded-full" style={{ fontSize: '0.72rem', fontWeight: 600 }}>Active</span></td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* Routes */}
      {tab === 'routes' && (
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
            <p className="text-gray-700" style={{ fontWeight: 600 }}>Routes ({routes.length})</p>
            <button className="flex items-center gap-1.5 bg-blue-600 text-white px-3 py-1.5 rounded-lg" style={{ fontSize: '0.8rem', fontWeight: 600 }}>
              <Plus className="w-3.5 h-3.5" />Add
            </button>
          </div>
          <table className="w-full">
            <thead><tr className="bg-gray-50">
              {['Code', 'Name', 'Territory', 'Distributor', 'Customers', 'Assigned Rep', 'Status'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
              ))}
            </tr></thead>
            <tbody>
              {routes.map(route => {
                const territory = territories.find(t => t.territory_id === route.territory_id);
                const distributor = distributors.find(d => d.distributor_id === route.distributor_id);
                const customerCount = customers.filter(c => c.route_id === route.route_id).length;
                const repPerf = [
                  { route_code: 'RT001', rep: 'Suresh N.' }, { route_code: 'RT002', rep: 'Suresh N.' },
                  { route_code: 'RT003', rep: 'Deepa M.' }, { route_code: 'RT004', rep: 'Karan S.' },
                  { route_code: 'RT005', rep: 'Ravi K.' }, { route_code: 'RT006', rep: 'Sneha P.' },
                ].find(r => r.route_code === route.route_code);
                return (
                  <tr key={route.route_id} className="border-t border-gray-50 hover:bg-gray-50">
                    <td className="px-4 py-3 text-gray-500 font-mono" style={{ fontSize: '0.82rem' }}>{route.route_code}</td>
                    <td className="px-4 py-3 text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>{route.route_name}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{territory?.territory_name}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{distributor?.distributor_name.split(' ').slice(0, 2).join(' ')}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{customerCount}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{repPerf?.rep || '—'}</td>
                    <td className="px-4 py-3"><span className="bg-green-100 text-green-700 px-2 py-0.5 rounded-full" style={{ fontSize: '0.72rem', fontWeight: 600 }}>Active</span></td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* Customers */}
      {tab === 'customers' && (
        <div className="space-y-3">
          <div className="flex items-center gap-3">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input type="text" placeholder="Search customers…" value={customerSearch}
                onChange={e => setCustomerSearch(e.target.value)}
                className="w-full pl-9 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500"
                style={{ fontSize: '0.875rem' }} />
            </div>
            <div className="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 flex items-center gap-2">
              <AlertCircle className="w-4 h-4 text-amber-500" />
              <span className="text-amber-700" style={{ fontSize: '0.78rem', fontWeight: 600 }}>{missingCoordCustomers.length} missing GPS</span>
            </div>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <table className="w-full">
              <thead><tr className="bg-gray-50">
                {['Code', 'Name', 'Route', 'City', 'Outlet', 'GPS', 'Status'].map(h => (
                  <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
                ))}
              </tr></thead>
              <tbody>
                {filteredCustomers.map(c => {
                  const route = routes.find(r => r.route_id === c.route_id);
                  const hasGps = !!(c.latitude && c.longitude);
                  return (
                    <tr key={c.customer_id} className="border-t border-gray-50 hover:bg-gray-50">
                      <td className="px-4 py-3 text-gray-500 font-mono" style={{ fontSize: '0.78rem' }}>{c.customer_code}</td>
                      <td className="px-4 py-3 text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>{c.customer_name}</td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{route?.route_code || '—'}</td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{c.city}</td>
                      <td className="px-4 py-3">
                        <span className={`px-2 py-0.5 rounded-full ${c.outlet_type === 'Modern Trade' ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700'}`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                          {c.outlet_type}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        {hasGps ? (
                          <span className="flex items-center gap-1 text-green-600" style={{ fontSize: '0.78rem' }}>
                            <CheckCircle className="w-3.5 h-3.5" />
                            {c.coordinate_capture_source?.replace('_', ' ')}
                          </span>
                        ) : (
                          <span className="flex items-center gap-1 text-amber-600" style={{ fontSize: '0.78rem' }}>
                            <AlertCircle className="w-3.5 h-3.5" />
                            Missing
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        <span className="bg-green-100 text-green-700 px-2 py-0.5 rounded-full" style={{ fontSize: '0.72rem', fontWeight: 600 }}>Active</span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Products */}
      {tab === 'products' && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input type="text" placeholder="Search products…" value={productSearch}
                onChange={e => setProductSearch(e.target.value)}
                className="w-full pl-9 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500"
                style={{ fontSize: '0.875rem' }} />
            </div>
            <button className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2.5 rounded-xl ml-3" style={{ fontSize: '0.875rem', fontWeight: 600 }}>
              <Plus className="w-4 h-4" />Add Product
            </button>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <table className="w-full">
              <thead><tr className="bg-gray-50">
                {['Code', 'Name', 'Category', 'UOM', 'Selling Price', 'MRP', 'Status'].map(h => (
                  <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
                ))}
              </tr></thead>
              <tbody>
                {filteredProducts.map(p => (
                  <tr key={p.product_id} className="border-t border-gray-50 hover:bg-gray-50">
                    <td className="px-4 py-3 text-gray-500 font-mono" style={{ fontSize: '0.78rem' }}>{p.product_code}</td>
                    <td className="px-4 py-3 text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>{p.product_name}</td>
                    <td className="px-4 py-3"><span className="bg-blue-50 text-blue-700 px-2 py-0.5 rounded-full" style={{ fontSize: '0.72rem' }}>{p.category}</span></td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{p.uom}</td>
                    <td className="px-4 py-3 text-gray-900" style={{ fontSize: '0.85rem', fontWeight: 600 }}>₹{p.selling_price.toFixed(2)}</td>
                    <td className="px-4 py-3 text-gray-500" style={{ fontSize: '0.82rem' }}>₹{p.mrp.toFixed(2)}</td>
                    <td className="px-4 py-3"><span className="bg-green-100 text-green-700 px-2 py-0.5 rounded-full" style={{ fontSize: '0.72rem', fontWeight: 600 }}>Active</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Uploads */}
      {tab === 'uploads' && (
        <div className="space-y-4">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            {['Customers', 'Products', 'Users', 'Route Assignments'].map(type => (
              <button key={type} className="bg-white border border-dashed border-gray-300 rounded-xl p-4 hover:border-blue-400 hover:bg-blue-50 transition text-center group">
                <Upload className="w-6 h-6 text-gray-300 group-hover:text-blue-400 mx-auto mb-2 transition" />
                <p className="text-gray-600 group-hover:text-blue-600" style={{ fontSize: '0.82rem', fontWeight: 500 }}>Upload {type}</p>
                <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>CSV format</p>
              </button>
            ))}
          </div>
          <div className="flex items-center gap-3 mb-1">
            <p className="text-gray-700" style={{ fontWeight: 600 }}>Upload History</p>
            <span className="bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full" style={{ fontSize: '0.72rem' }}>{uploadJobs.length} jobs</span>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <table className="w-full">
              <thead><tr className="bg-gray-50">
                {['Type', 'File', 'Uploaded By', 'Date', 'Records', 'Errors', 'Status'].map(h => (
                  <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
                ))}
              </tr></thead>
              <tbody>
                {uploadJobs.map(job => (
                  <tr key={job.id} className="border-t border-gray-50 hover:bg-gray-50">
                    <td className="px-4 py-3"><span className="bg-blue-50 text-blue-700 px-2 py-0.5 rounded-full" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{job.type}</span></td>
                    <td className="px-4 py-3 text-gray-700" style={{ fontSize: '0.82rem' }}>{job.file}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{job.uploaded_by}</td>
                    <td className="px-4 py-3 text-gray-500" style={{ fontSize: '0.82rem' }}>{job.uploaded_at}</td>
                    <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{job.records}</td>
                    <td className="px-4 py-3">
                      <span className={job.errors > 0 ? 'text-red-600' : 'text-green-600'} style={{ fontSize: '0.82rem', fontWeight: 600 }}>{job.errors}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-0.5 rounded-full ${statusColor(job.status)}`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                        {job.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Reports */}
      {tab === 'reports' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {[
            { title: 'Master Data Completeness', icon: '📋', items: [{ label: 'Total Customers', value: customers.length, ok: true }, { label: 'With GPS Coordinates', value: customers.filter(c => c.latitude).length, ok: true }, { label: 'Missing Coordinates', value: missingCoordCustomers.length, ok: false }] },
            { title: 'System Overview', icon: '🗂️', items: [{ label: 'Total Users', value: initialUsers.length, ok: true }, { label: 'Active Routes', value: routes.filter(r => r.is_active).length, ok: true }, { label: 'Distributors', value: distributors.length, ok: true }] },
          ].map(card => (
            <div key={card.title} className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
              <p className="text-gray-700 mb-3" style={{ fontWeight: 600 }}>{card.icon} {card.title}</p>
              <div className="space-y-2.5">
                {card.items.map(item => (
                  <div key={item.label} className="flex items-center justify-between">
                    <span className="text-gray-600" style={{ fontSize: '0.875rem' }}>{item.label}</span>
                    <div className="flex items-center gap-2">
                      <span className="text-gray-900" style={{ fontWeight: 700 }}>{item.value}</span>
                      {!item.ok && item.value > 0 && (
                        <AlertCircle className="w-4 h-4 text-amber-500" />
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}

          {/* Missing coordinates report */}
          <div className="md:col-span-2 bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
              <p className="text-gray-700" style={{ fontWeight: 600 }}>⚠️ Customers Without Coordinates ({missingCoordCustomers.length})</p>
              <button className="flex items-center gap-1.5 text-blue-600 hover:text-blue-700" style={{ fontSize: '0.8rem', fontWeight: 500 }}>
                <Download className="w-3.5 h-3.5" />Download Report
              </button>
            </div>
            <table className="w-full">
              <thead><tr className="bg-gray-50">
                {['Code', 'Customer Name', 'Route', 'City', 'Contact'].map(h => (
                  <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
                ))}
              </tr></thead>
              <tbody>
                {missingCoordCustomers.map(c => {
                  const route = routes.find(r => r.route_id === c.route_id);
                  return (
                    <tr key={c.customer_id} className="border-t border-gray-50 hover:bg-gray-50">
                      <td className="px-4 py-3 text-gray-500 font-mono" style={{ fontSize: '0.78rem' }}>{c.customer_code}</td>
                      <td className="px-4 py-3 text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>{c.customer_name}</td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{route?.route_name || '—'}</td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{c.city}</td>
                      <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{c.contact_person}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
