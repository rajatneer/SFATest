import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../../context/AppContext';
import { Play, Square, MapPin, CheckCircle, Package, Route, ChevronRight, Clock, AlertCircle, Users } from 'lucide-react';
import { routes, territories, distributors } from '../../data/mockData';

export default function RepHome() {
  const { currentUser, daySession, startDay, endDay, selectedRoute, getAssignedRoutes, getTodayVisits, getTodayOrders } = useApp();
  const navigate = useNavigate();
  const [starting, setStarting] = useState(false);
  const [ending, setEnding] = useState(false);
  const [showEndConfirm, setShowEndConfirm] = useState(false);

  if (!currentUser) return null;

  const assignedRoutes = getAssignedRoutes(currentUser.user_id);
  const todayVisits = getTodayVisits(currentUser.user_id);
  const todayOrders = getTodayOrders(currentUser.user_id);
  const completedVisits = todayVisits.filter(v => v.visit_status === 'completed').length;
  const skippedVisits = todayVisits.filter(v => v.visit_status === 'skipped').length;
  const ordersTotal = todayOrders.reduce((sum, o) => sum + o.net_amount, 0);

  const handleStartDay = async () => {
    setStarting(true);
    await new Promise(r => setTimeout(r, 1000));
    startDay();
    setStarting(false);
  };

  const handleEndDay = async () => {
    setEnding(true);
    await new Promise(r => setTimeout(r, 800));
    endDay();
    setEnding(false);
    setShowEndConfirm(false);
  };

  const territory = selectedRoute ? territories.find(t => t.territory_id === selectedRoute.territory_id) : null;
  const distributor = selectedRoute ? distributors.find(d => d.distributor_id === selectedRoute.distributor_id) : null;

  return (
    <div className="p-4 space-y-4 pb-6">
      {/* Greeting */}
      <div className="pt-2">
        <p className="text-gray-500" style={{ fontSize: '0.85rem' }}>Good morning 👋</p>
        <h2 className="text-gray-900" style={{ fontSize: '1.25rem', fontWeight: 700 }}>
          {currentUser.full_name.split(' ')[0]}
        </h2>
      </div>

      {/* Day Session Card */}
      {!daySession ? (
        <div className="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center">
              <Clock className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <p className="text-gray-900" style={{ fontWeight: 600 }}>Start Your Day</p>
              <p className="text-gray-500" style={{ fontSize: '0.8rem' }}>Fri, April 24, 2026</p>
            </div>
          </div>
          <p className="text-gray-500 mb-4" style={{ fontSize: '0.85rem' }}>
            Tap Start Day to begin field activities. Your location will be captured.
          </p>
          <button onClick={handleStartDay} disabled={starting}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3.5 rounded-xl flex items-center justify-center gap-2 transition active:scale-95 disabled:opacity-60"
            style={{ fontWeight: 600 }}>
            <Play className="w-5 h-5" fill="white" />
            {starting ? 'Capturing location…' : 'Start Day'}
          </button>
        </div>
      ) : daySession.status === 'started' ? (
        <>
          {/* Stats */}
          <div className="grid grid-cols-3 gap-3">
            <div className="bg-white rounded-xl border border-gray-200 p-3 text-center shadow-sm">
              <p className="text-blue-600" style={{ fontSize: '1.5rem', fontWeight: 700 }}>{completedVisits}</p>
              <p className="text-gray-500" style={{ fontSize: '0.7rem' }}>Visited</p>
            </div>
            <div className="bg-white rounded-xl border border-gray-200 p-3 text-center shadow-sm">
              <p className="text-green-600" style={{ fontSize: '1.5rem', fontWeight: 700 }}>{todayOrders.length}</p>
              <p className="text-gray-500" style={{ fontSize: '0.7rem' }}>Orders</p>
            </div>
            <div className="bg-white rounded-xl border border-gray-200 p-3 text-center shadow-sm">
              <p className="text-orange-600" style={{ fontSize: '1.25rem', fontWeight: 700 }}>
                ₹{ordersTotal >= 1000 ? `${(ordersTotal / 1000).toFixed(1)}k` : ordersTotal}
              </p>
              <p className="text-gray-500" style={{ fontSize: '0.7rem' }}>Value</p>
            </div>
          </div>

          {/* Active Route */}
          {selectedRoute ? (
            <div className="bg-gradient-to-br from-blue-600 to-blue-700 rounded-2xl p-4 text-white shadow-sm">
              <div className="flex items-center justify-between mb-3">
                <p className="text-blue-200" style={{ fontSize: '0.75rem', fontWeight: 500 }}>ACTIVE ROUTE</p>
                <button onClick={() => navigate('/rep/route-select')}
                  className="text-blue-200 hover:text-white transition" style={{ fontSize: '0.75rem' }}>
                  Change
                </button>
              </div>
              <p className="text-white mb-1" style={{ fontWeight: 700, fontSize: '1rem' }}>{selectedRoute.route_name}</p>
              <p className="text-blue-200" style={{ fontSize: '0.8rem' }}>{selectedRoute.route_code}</p>
              <div className="mt-3 flex gap-3">
                {territory && (
                  <div className="bg-white/15 rounded-lg px-2.5 py-1.5 flex items-center gap-1.5">
                    <MapPin className="w-3 h-3" />
                    <span style={{ fontSize: '0.75rem' }}>{territory.territory_name}</span>
                  </div>
                )}
                {distributor && (
                  <div className="bg-white/15 rounded-lg px-2.5 py-1.5 flex items-center gap-1.5">
                    <Package className="w-3 h-3" />
                    <span style={{ fontSize: '0.75rem' }} className="truncate max-w-[100px]">{distributor.distributor_name.split(' ').slice(0, 2).join(' ')}</span>
                  </div>
                )}
              </div>
              <button onClick={() => navigate('/rep/customers')}
                className="mt-3 w-full bg-white/20 hover:bg-white/30 text-white py-2.5 rounded-xl flex items-center justify-center gap-2 transition"
                style={{ fontWeight: 600, fontSize: '0.9rem' }}>
                <Users className="w-4 h-4" />
                View Customers
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          ) : (
            <div className="bg-white rounded-2xl border border-amber-200 p-4 shadow-sm">
              <div className="flex items-center gap-2 mb-2">
                <AlertCircle className="w-5 h-5 text-amber-500" />
                <p className="text-gray-900" style={{ fontWeight: 600 }}>No Route Selected</p>
              </div>
              <p className="text-gray-500 mb-3" style={{ fontSize: '0.85rem' }}>Select a route to start visiting customers.</p>
              <button onClick={() => navigate('/rep/route-select')}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 rounded-xl flex items-center justify-center gap-2 transition"
                style={{ fontWeight: 600 }}>
                <Route className="w-4 h-4" />
                Select Route
              </button>
            </div>
          )}

          {/* Today's Activity */}
          {todayVisits.length > 0 && (
            <div className="bg-white rounded-2xl border border-gray-200 p-4 shadow-sm">
              <p className="text-gray-700 mb-3" style={{ fontWeight: 600 }}>Today's Activity</p>
              <div className="space-y-2">
                {todayVisits.slice(0, 3).map(visit => {
                  const isCompleted = visit.visit_status === 'completed';
                  const isSkipped = visit.visit_status === 'skipped';
                  return (
                    <div key={visit.visit_id} className="flex items-center gap-3">
                      <div className={`w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 ${isCompleted ? 'bg-green-100' : 'bg-gray-100'}`}>
                        <CheckCircle className={`w-4 h-4 ${isCompleted ? 'text-green-600' : 'text-gray-400'}`} />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-gray-800 truncate" style={{ fontSize: '0.85rem', fontWeight: 500 }}>
                          Customer #{visit.customer_id}
                        </p>
                        <p className="text-gray-400" style={{ fontSize: '0.75rem' }}>
                          {isCompleted ? 'Completed' : isSkipped ? 'Skipped' : 'Active'} · {visit.has_order ? '📦 Order placed' : 'No order'}
                        </p>
                      </div>
                      <span className={`px-2 py-0.5 rounded-full text-white flex-shrink-0 ${isCompleted ? 'bg-green-500' : isSkipped ? 'bg-gray-400' : 'bg-blue-500'}`}
                        style={{ fontSize: '0.65rem', fontWeight: 600 }}>
                        {isCompleted ? 'Done' : isSkipped ? 'Skip' : 'Active'}
                      </span>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* End Day */}
          {!showEndConfirm ? (
            <button onClick={() => setShowEndConfirm(true)}
              className="w-full bg-white border border-red-200 text-red-600 hover:bg-red-50 py-3 rounded-xl flex items-center justify-center gap-2 transition"
              style={{ fontWeight: 600 }}>
              <Square className="w-4 h-4" />
              End Day
            </button>
          ) : (
            <div className="bg-white rounded-2xl border border-red-200 p-4 shadow-sm">
              <p className="text-gray-900 mb-1" style={{ fontWeight: 600 }}>End Day Confirmation</p>
              <p className="text-gray-500 mb-4" style={{ fontSize: '0.85rem' }}>
                You've visited {completedVisits} customers and created {todayOrders.length} orders. Are you sure?
              </p>
              <div className="flex gap-2">
                <button onClick={() => setShowEndConfirm(false)}
                  className="flex-1 bg-gray-100 hover:bg-gray-200 text-gray-700 py-2.5 rounded-xl transition" style={{ fontWeight: 500 }}>
                  Cancel
                </button>
                <button onClick={handleEndDay} disabled={ending}
                  className="flex-1 bg-red-600 hover:bg-red-700 text-white py-2.5 rounded-xl transition disabled:opacity-60" style={{ fontWeight: 600 }}>
                  {ending ? 'Ending…' : 'Confirm End Day'}
                </button>
              </div>
            </div>
          )}
        </>
      ) : (
        <div className="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-3">
            <CheckCircle className="w-6 h-6 text-gray-400" />
          </div>
          <p className="text-gray-900 mb-1" style={{ fontWeight: 600 }}>Day Ended</p>
          <p className="text-gray-500" style={{ fontSize: '0.85rem' }}>
            You completed {completedVisits} visits and {todayOrders.length} orders today.
          </p>
        </div>
      )}

      {/* Assigned routes (when no day session) */}
      {!daySession && (
        <div className="bg-white rounded-2xl border border-gray-200 p-4 shadow-sm">
          <p className="text-gray-700 mb-3" style={{ fontWeight: 600 }}>Your Routes</p>
          {assignedRoutes.map(r => (
            <div key={r.route_id} className="flex items-center gap-3 py-2.5 border-b border-gray-50 last:border-0">
              <div className="w-8 h-8 bg-blue-50 rounded-lg flex items-center justify-center">
                <Route className="w-4 h-4 text-blue-600" />
              </div>
              <div className="flex-1">
                <p className="text-gray-800" style={{ fontSize: '0.875rem', fontWeight: 500 }}>{r.route_name}</p>
                <p className="text-gray-400" style={{ fontSize: '0.75rem' }}>{r.route_code}</p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}