import React, { useState } from 'react';
import { Search, Package, ChevronRight, X, Check, Truck, CheckCircle } from 'lucide-react';
import { useApp } from '../../context/AppContext';
import { customers, routes } from '../../data/mockData';
import { Order, OrderStatus } from '../../data/mockData';

const statusColors: Record<string, { bg: string; text: string; border: string }> = {
  Created: { bg: 'bg-blue-50', text: 'text-blue-700', border: 'border-blue-200' },
  Synced: { bg: 'bg-indigo-50', text: 'text-indigo-700', border: 'border-indigo-200' },
  Accepted: { bg: 'bg-purple-50', text: 'text-purple-700', border: 'border-purple-200' },
  Dispatched: { bg: 'bg-amber-50', text: 'text-amber-700', border: 'border-amber-200' },
  Delivered: { bg: 'bg-green-50', text: 'text-green-700', border: 'border-green-200' },
  Cancelled: { bg: 'bg-red-50', text: 'text-red-700', border: 'border-red-200' },
};

const STATUS_FLOW: OrderStatus[] = ['Synced', 'Accepted', 'Dispatched', 'Delivered'];

export default function DistributorApp() {
  const { currentUser, orders, updateOrderStatus } = useApp();
  const [search, setSearch] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | OrderStatus>('all');
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [newStatus, setNewStatus] = useState<OrderStatus>('Accepted');
  const [remarks, setRemarks] = useState('');
  const [updating, setUpdating] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);

  if (!currentUser) return null;

  // Filter orders for this distributor
  const distributorId = currentUser.distributor_id;
  const myOrders = orders.filter(o => o.distributor_id === distributorId);

  const filteredOrders = myOrders.filter(o => {
    const customer = customers.find(c => c.customer_id === o.customer_id);
    const matchSearch =
      o.order_number.toLowerCase().includes(search.toLowerCase()) ||
      customer?.customer_name.toLowerCase().includes(search.toLowerCase()) || false;
    const matchStatus = filterStatus === 'all' || o.status === filterStatus;
    return matchSearch && matchStatus;
  });

  const statusCounts = myOrders.reduce((acc, o) => {
    acc[o.status] = (acc[o.status] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  const pendingAction = myOrders.filter(o => ['Created', 'Synced', 'Accepted', 'Dispatched'].includes(o.status)).length;

  const getNextStatus = (current: OrderStatus): OrderStatus[] => {
    const idx = STATUS_FLOW.indexOf(current);
    return STATUS_FLOW.slice(idx + 1);
  };

  const handleUpdateStatus = async () => {
    if (!selectedOrder) return;
    setUpdating(true);
    await new Promise(r => setTimeout(r, 800));
    updateOrderStatus(selectedOrder.order_id, newStatus, remarks);
    setUpdating(false);
    setShowSuccess(true);
    setTimeout(() => {
      setShowSuccess(false);
      setSelectedOrder(null);
      setRemarks('');
    }, 1200);
  };

  return (
    <div className="p-6 relative">
      {/* Summary row */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-6">
        {[
          { label: 'Total Orders', value: myOrders.length, color: 'text-gray-900' },
          { label: 'Pending Action', value: pendingAction, color: 'text-orange-600' },
          { label: 'Dispatched', value: statusCounts['Dispatched'] || 0, color: 'text-amber-600' },
          { label: 'Delivered', value: statusCounts['Delivered'] || 0, color: 'text-green-600' },
        ].map(s => (
          <div key={s.label} className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
            <p className={s.color} style={{ fontSize: '1.6rem', fontWeight: 700 }}>{s.value}</p>
            <p className="text-gray-500" style={{ fontSize: '0.78rem' }}>{s.label}</p>
          </div>
        ))}
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3 mb-4 flex-wrap">
        <div className="relative flex-1 min-w-[200px] max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input type="text" placeholder="Search orders or customers…" value={search}
            onChange={e => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500"
            style={{ fontSize: '0.875rem' }} />
        </div>
        <div className="flex gap-1.5 flex-wrap">
          <button onClick={() => setFilterStatus('all')}
            className={`px-3 py-2 rounded-lg transition ${filterStatus === 'all' ? 'bg-slate-900 text-white' : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-50'}`}
            style={{ fontSize: '0.78rem', fontWeight: 500 }}>
            All ({myOrders.length})
          </button>
          {(['Synced', 'Accepted', 'Dispatched', 'Delivered'] as OrderStatus[]).map(s => (
            <button key={s} onClick={() => setFilterStatus(s)}
              className={`px-3 py-2 rounded-lg transition ${filterStatus === s ? `${statusColors[s].bg} ${statusColors[s].text} border ${statusColors[s].border}` : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-50'}`}
              style={{ fontSize: '0.78rem', fontWeight: 500 }}>
              {s} {statusCounts[s] ? `(${statusCounts[s]})` : ''}
            </button>
          ))}
        </div>
      </div>

      {/* Orders table */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="bg-gray-50">
              {['Order No.', 'Date', 'Customer', 'Route', 'Items', 'Amount', 'Status', 'Action'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-gray-500" style={{ fontSize: '0.75rem', fontWeight: 600 }}>{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filteredOrders.length === 0 ? (
              <tr><td colSpan={8} className="px-4 py-8 text-center text-gray-400" style={{ fontSize: '0.875rem' }}>No orders found</td></tr>
            ) : filteredOrders.map(order => {
              const customer = customers.find(c => c.customer_id === order.customer_id);
              const route = routes.find(r => r.route_id === order.route_id);
              const sc = statusColors[order.status] || { bg: 'bg-gray-50', text: 'text-gray-600', border: 'border-gray-200' };
              const canUpdate = ['Created', 'Synced', 'Accepted', 'Dispatched'].includes(order.status);
              return (
                <tr key={order.order_id} className="border-t border-gray-50 hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <span className="text-blue-600" style={{ fontSize: '0.82rem', fontWeight: 500 }}>{order.order_number}</span>
                  </td>
                  <td className="px-4 py-3 text-gray-500" style={{ fontSize: '0.82rem' }}>{order.order_date}</td>
                  <td className="px-4 py-3 text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 500 }}>
                    {customer?.customer_name || '—'}
                  </td>
                  <td className="px-4 py-3 text-gray-500" style={{ fontSize: '0.78rem' }}>{route?.route_code || '—'}</td>
                  <td className="px-4 py-3 text-gray-600" style={{ fontSize: '0.82rem' }}>{order.items.length} items</td>
                  <td className="px-4 py-3 text-gray-900" style={{ fontSize: '0.85rem', fontWeight: 600 }}>₹{order.net_amount.toLocaleString()}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded-full border ${sc.bg} ${sc.text} ${sc.border}`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                      {order.status}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    {canUpdate && (
                      <button
                        onClick={() => {
                          setSelectedOrder(order);
                          setNewStatus(getNextStatus(order.status)[0] || 'Delivered');
                          setRemarks('');
                        }}
                        className="flex items-center gap-1.5 text-blue-600 hover:text-blue-800 hover:bg-blue-50 px-2.5 py-1.5 rounded-lg transition"
                        style={{ fontSize: '0.78rem', fontWeight: 500 }}>
                        <Truck className="w-3.5 h-3.5" />
                        Update
                      </button>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Status update modal */}
      {selectedOrder && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4" onClick={e => e.target === e.currentTarget && setSelectedOrder(null)}>
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6 relative">
            {showSuccess ? (
              <div className="text-center py-6">
                <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-3">
                  <CheckCircle className="w-8 h-8 text-green-600" />
                </div>
                <p className="text-gray-900" style={{ fontWeight: 700, fontSize: '1.1rem' }}>Status Updated!</p>
                <p className="text-gray-500 mt-1" style={{ fontSize: '0.875rem' }}>Order is now <strong>{newStatus}</strong></p>
              </div>
            ) : (
              <>
                <div className="flex items-center justify-between mb-4">
                  <div>
                    <p className="text-gray-900" style={{ fontWeight: 700, fontSize: '1rem' }}>Update Order Status</p>
                    <p className="text-gray-400" style={{ fontSize: '0.82rem' }}>{selectedOrder.order_number}</p>
                  </div>
                  <button onClick={() => setSelectedOrder(null)} className="text-gray-400 hover:text-gray-600">
                    <X className="w-5 h-5" />
                  </button>
                </div>

                {/* Order summary */}
                <div className="bg-gray-50 rounded-xl p-3 mb-4">
                  <div className="flex justify-between mb-1.5">
                    <span className="text-gray-500" style={{ fontSize: '0.82rem' }}>Customer</span>
                    <span className="text-gray-800" style={{ fontSize: '0.82rem', fontWeight: 600 }}>
                      {customers.find(c => c.customer_id === selectedOrder.customer_id)?.customer_name}
                    </span>
                  </div>
                  <div className="flex justify-between mb-1.5">
                    <span className="text-gray-500" style={{ fontSize: '0.82rem' }}>Amount</span>
                    <span className="text-gray-800" style={{ fontSize: '0.82rem', fontWeight: 600 }}>₹{selectedOrder.net_amount.toLocaleString()}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500" style={{ fontSize: '0.82rem' }}>Current Status</span>
                    <span className={`px-2 py-0.5 rounded-full ${statusColors[selectedOrder.status]?.bg} ${statusColors[selectedOrder.status]?.text}`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
                      {selectedOrder.status}
                    </span>
                  </div>
                </div>

                <div className="mb-4">
                  <label className="block text-gray-600 mb-2" style={{ fontSize: '0.85rem', fontWeight: 500 }}>New Status</label>
                  <div className="flex gap-2 flex-wrap">
                    {getNextStatus(selectedOrder.status).map(s => (
                      <button key={s} onClick={() => setNewStatus(s)}
                        className={`px-3 py-2 rounded-lg border transition ${newStatus === s ? `${statusColors[s].bg} ${statusColors[s].text} ${statusColors[s].border} border` : 'border-gray-200 text-gray-600 hover:bg-gray-50'}`}
                        style={{ fontSize: '0.82rem', fontWeight: 500 }}>
                        {s}
                      </button>
                    ))}
                    <button onClick={() => setNewStatus('Cancelled')}
                      className={`px-3 py-2 rounded-lg border transition ${newStatus === 'Cancelled' ? 'bg-red-50 text-red-700 border-red-200' : 'border-gray-200 text-gray-400 hover:bg-gray-50'}`}
                      style={{ fontSize: '0.82rem', fontWeight: 500 }}>
                      Cancel Order
                    </button>
                  </div>
                </div>

                <div className="mb-4">
                  <label className="block text-gray-600 mb-2" style={{ fontSize: '0.85rem', fontWeight: 500 }}>Remarks (optional)</label>
                  <textarea
                    placeholder="Add a note (e.g. Out for delivery, Partial delivery…)"
                    value={remarks}
                    onChange={e => setRemarks(e.target.value)}
                    rows={2}
                    className="w-full px-3 py-2.5 border border-gray-200 rounded-xl resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
                    style={{ fontSize: '0.875rem' }}
                  />
                </div>

                <div className="flex gap-2">
                  <button onClick={() => setSelectedOrder(null)}
                    className="flex-1 border border-gray-200 text-gray-600 py-2.5 rounded-xl hover:bg-gray-50 transition"
                    style={{ fontWeight: 500 }}>
                    Cancel
                  </button>
                  <button onClick={handleUpdateStatus} disabled={updating}
                    className="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-xl transition disabled:opacity-60"
                    style={{ fontWeight: 600 }}>
                    {updating ? 'Updating…' : `Mark as ${newStatus}`}
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
