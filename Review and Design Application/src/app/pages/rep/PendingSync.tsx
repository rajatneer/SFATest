import React, { useState } from 'react';
import { useApp } from '../../context/AppContext';
import { RefreshCw, CheckCircle, AlertCircle, Clock, Wifi } from 'lucide-react';

const entityIcons: Record<string, string> = {
  day_session: '🕐',
  visit: '📍',
  order: '📦',
  coordinate: '🗺️',
};

const statusColors = {
  pending: 'bg-orange-100 text-orange-700 border-orange-200',
  syncing: 'bg-blue-100 text-blue-700 border-blue-200',
  synced: 'bg-green-100 text-green-700 border-green-200',
  failed: 'bg-red-100 text-red-700 border-red-200',
};

export default function PendingSync() {
  const { syncQueue, syncPending, orders } = useApp();
  const [syncing, setSyncing] = useState(false);

  const pendingCount = syncQueue.filter(i => i.status === 'pending').length;
  const failedCount = syncQueue.filter(i => i.status === 'failed').length;
  const syncedCount = syncQueue.filter(i => i.status === 'synced').length;
  const pendingOrders = orders.filter(o => o.sync_status === 'pending').length;

  const handleSync = async () => {
    setSyncing(true);
    await syncPending();
    setSyncing(false);
  };

  return (
    <div className="p-4 pb-6 space-y-4">
      <div className="pt-2">
        <h2 className="text-gray-900" style={{ fontWeight: 700, fontSize: '1.1rem' }}>Sync Queue</h2>
        <p className="text-gray-500" style={{ fontSize: '0.8rem' }}>Offline data waiting to be uploaded</p>
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-3 gap-3">
        <div className="bg-white rounded-xl border border-orange-200 p-3 text-center">
          <p className="text-orange-600" style={{ fontSize: '1.4rem', fontWeight: 700 }}>{pendingCount}</p>
          <p className="text-gray-500" style={{ fontSize: '0.7rem' }}>Pending</p>
        </div>
        <div className="bg-white rounded-xl border border-red-200 p-3 text-center">
          <p className="text-red-600" style={{ fontSize: '1.4rem', fontWeight: 700 }}>{failedCount}</p>
          <p className="text-gray-500" style={{ fontSize: '0.7rem' }}>Failed</p>
        </div>
        <div className="bg-white rounded-xl border border-green-200 p-3 text-center">
          <p className="text-green-600" style={{ fontSize: '1.4rem', fontWeight: 700 }}>{syncedCount}</p>
          <p className="text-gray-500" style={{ fontSize: '0.7rem' }}>Synced</p>
        </div>
      </div>

      {/* Sync button */}
      {(pendingCount > 0 || failedCount > 0) && (
        <button
          onClick={handleSync}
          disabled={syncing}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3.5 rounded-xl flex items-center justify-center gap-2 transition disabled:opacity-60"
          style={{ fontWeight: 600 }}
        >
          <RefreshCw className={`w-5 h-5 ${syncing ? 'animate-spin' : ''}`} />
          {syncing ? 'Syncing…' : `Sync ${pendingCount + failedCount} Items`}
        </button>
      )}

      {pendingOrders > 0 && (
        <div className="bg-orange-50 border border-orange-200 rounded-xl p-3 flex items-center gap-2">
          <AlertCircle className="w-4 h-4 text-orange-500 flex-shrink-0" />
          <p className="text-orange-700" style={{ fontSize: '0.82rem' }}>{pendingOrders} order(s) pending server confirmation.</p>
        </div>
      )}

      {/* Queue list */}
      {syncQueue.length === 0 ? (
        <div className="bg-white rounded-2xl border border-gray-200 p-8 text-center shadow-sm">
          <div className="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-3">
            <Wifi className="w-6 h-6 text-green-600" />
          </div>
          <p className="text-gray-700" style={{ fontWeight: 600 }}>All Synced</p>
          <p className="text-gray-400 mt-1" style={{ fontSize: '0.85rem' }}>No pending items in the sync queue.</p>
        </div>
      ) : (
        <div className="space-y-2">
          {syncQueue.slice().reverse().map(item => (
            <div key={item.id} className="bg-white rounded-xl border border-gray-200 p-3.5 shadow-sm">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <span className="text-xl">{entityIcons[item.entity_type] || '📄'}</span>
                  <div>
                    <p className="text-gray-800" style={{ fontSize: '0.85rem', fontWeight: 600 }}>{item.description}</p>
                    <p className="text-gray-400" style={{ fontSize: '0.72rem' }}>
                      {item.entity_ref} · {new Date(item.created_at).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </p>
                  </div>
                </div>
                <span className={`px-2 py-0.5 rounded-full border ${statusColors[item.status]}`} style={{ fontSize: '0.68rem', fontWeight: 600 }}>
                  {item.status === 'syncing' ? '⏳ Syncing' : item.status === 'synced' ? '✓ Done' : item.status === 'failed' ? '✗ Failed' : '● Pending'}
                </span>
              </div>
              {item.error && (
                <p className="mt-1.5 text-red-500" style={{ fontSize: '0.75rem' }}>{item.error}</p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
