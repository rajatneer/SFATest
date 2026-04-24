import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import { useApp } from '../../context/AppContext';
import { customers, products } from '../../data/mockData';
import {
  ArrowLeft, MapPin, Phone, CheckCircle, XCircle, Package,
  Plus, Minus, Trash2, ShoppingCart, Navigation, AlertTriangle, Search
} from 'lucide-react';

type Step = 'checkin' | 'visit' | 'order' | 'done';

interface CartItem {
  product_id: number;
  quantity: number;
}

export default function CustomerDetail() {
  const { customerId } = useParams<{ customerId: string }>();
  const navigate = useNavigate();
  const { currentUser, daySession, selectedRoute, visits, addVisit, checkoutVisit, addOrder } = useApp();

  const customer = customers.find(c => c.customer_id === Number(customerId));
  const existingVisit = visits.find(v => v.customer_id === Number(customerId) && v.rep_user_id === currentUser?.user_id);

  const [step, setStep] = useState<Step>(() => {
    if (!existingVisit) return 'checkin';
    if (existingVisit.visit_status === 'active') return 'visit';
    return 'done';
  });

  const [checkingIn, setCheckingIn] = useState(false);
  const [geoResult, setGeoResult] = useState<{ distance: number; within: boolean } | null>(null);
  const [captureCoords, setCaptureCoords] = useState(false);
  const [visitNotes, setVisitNotes] = useState('');
  const [visitStatus, setVisitStatus] = useState<'completed' | 'skipped'>('completed');
  const [showOrder, setShowOrder] = useState(false);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [productSearch, setProductSearch] = useState('');
  const [currentVisitId, setCurrentVisitId] = useState<number | undefined>(existingVisit?.visit_id);
  const [savingOrder, setSavingOrder] = useState(false);
  const [orderSaved, setOrderSaved] = useState(false);
  const [savedOrder, setSavedOrder] = useState<any>(null);

  if (!customer) return <div className="p-4 text-gray-500">Customer not found</div>;

  const hasCoords = !!(customer.latitude && customer.longitude);

  const handleCheckin = async () => {
    setCheckingIn(true);
    await new Promise(r => setTimeout(r, 1200));
    if (hasCoords) {
      const dist = Math.random() * 120;
      setGeoResult({ distance: Math.round(dist), within: dist <= 100 });
    } else {
      setCaptureCoords(true);
    }
    const visit = addVisit(customer.customer_id);
    setCurrentVisitId(visit.visit_id);
    setCheckingIn(false);
    if (hasCoords || !captureCoords) {
      setTimeout(() => setStep('visit'), 500);
    }
  };

  const handleCaptureAndCheckin = async () => {
    setCheckingIn(true);
    await new Promise(r => setTimeout(r, 800));
    setGeoResult({ distance: 0, within: true });
    const visit = addVisit(customer.customer_id);
    setCurrentVisitId(visit.visit_id);
    setCheckingIn(false);
    setStep('visit');
  };

  const handleCheckout = () => {
    if (currentVisitId) {
      checkoutVisit(currentVisitId, visitNotes, visitStatus);
    }
    setStep('done');
  };

  const handleSaveOrder = async () => {
    if (cart.length === 0) return;
    setSavingOrder(true);
    await new Promise(r => setTimeout(r, 800));
    const order = addOrder(customer.customer_id, currentVisitId, cart);
    setSavedOrder(order);
    setOrderSaved(true);
    setSavingOrder(false);
    setShowOrder(false);
    setCart([]);
  };

  const activeProducts = products.filter(p => p.is_active);
  const filteredProducts = activeProducts.filter(p =>
    p.product_name.toLowerCase().includes(productSearch.toLowerCase()) ||
    p.product_code.toLowerCase().includes(productSearch.toLowerCase())
  );

  const cartTotal = cart.reduce((sum, item) => {
    const product = products.find(p => p.product_id === item.product_id);
    return sum + (product?.selling_price || 0) * item.quantity;
  }, 0);

  const adjustQty = (productId: number, delta: number) => {
    setCart(prev => {
      const existing = prev.find(i => i.product_id === productId);
      if (!existing) {
        if (delta > 0) return [...prev, { product_id: productId, quantity: delta }];
        return prev;
      }
      const newQty = existing.quantity + delta;
      if (newQty <= 0) return prev.filter(i => i.product_id !== productId);
      return prev.map(i => i.product_id === productId ? { ...i, quantity: newQty } : i);
    });
  };

  const getCartQty = (productId: number) =>
    cart.find(i => i.product_id === productId)?.quantity || 0;

  const outletBadgeColor = customer.outlet_type === 'Modern Trade' ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700';

  return (
    <div className="pb-6">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-4 pt-4 pb-4">
        <button onClick={() => navigate('/rep/customers')} className="flex items-center gap-2 text-gray-500 mb-3 hover:text-gray-700">
          <ArrowLeft className="w-4 h-4" />
          <span style={{ fontSize: '0.85rem' }}>Customer List</span>
        </button>

        <div className="flex items-start justify-between">
          <div className="flex-1 min-w-0">
            <h2 className="text-gray-900" style={{ fontWeight: 700, fontSize: '1.05rem' }}>{customer.customer_name}</h2>
            <p className="text-gray-500 mt-0.5" style={{ fontSize: '0.8rem' }}>{customer.customer_code}</p>
          </div>
          <span className={`px-2.5 py-1 rounded-full ${outletBadgeColor} flex-shrink-0`} style={{ fontSize: '0.72rem', fontWeight: 600 }}>
            {customer.outlet_type}
          </span>
        </div>

        <div className="mt-3 space-y-1.5">
          <div className="flex items-center gap-2">
            <MapPin className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
            <span className="text-gray-600" style={{ fontSize: '0.8rem' }}>{customer.address_line_1}, {customer.locality}, {customer.city}</span>
          </div>
          <div className="flex items-center gap-2">
            <Phone className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
            <span className="text-gray-600" style={{ fontSize: '0.8rem' }}>{customer.contact_person} · {customer.mobile_number}</span>
          </div>
        </div>

        {!hasCoords && (
          <div className="mt-2.5 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 flex items-center gap-2">
            <AlertTriangle className="w-4 h-4 text-amber-500 flex-shrink-0" />
            <span className="text-amber-700" style={{ fontSize: '0.78rem' }}>GPS coordinates not available for this customer.</span>
          </div>
        )}
      </div>

      <div className="p-4 space-y-4">
        {/* Step: Check In */}
        {step === 'checkin' && (
          <div className="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center">
                <Navigation className="w-5 h-5 text-blue-600" />
              </div>
              <div>
                <p className="text-gray-900" style={{ fontWeight: 600 }}>Check In</p>
                <p className="text-gray-500" style={{ fontSize: '0.8rem' }}>Capture GPS to verify your location</p>
              </div>
            </div>

            {geoResult && (
              <div className={`rounded-xl p-3 mb-3 ${geoResult.within ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'}`}>
                <div className="flex items-center gap-2">
                  {geoResult.within ? <CheckCircle className="w-4 h-4 text-green-600" /> : <XCircle className="w-4 h-4 text-red-500" />}
                  <span className={geoResult.within ? 'text-green-700' : 'text-red-700'} style={{ fontSize: '0.85rem', fontWeight: 600 }}>
                    {geoResult.within ? 'Within Tolerance' : 'Outside Tolerance'}
                  </span>
                </div>
                <p className="text-gray-500 mt-0.5 ml-6" style={{ fontSize: '0.78rem' }}>
                  Distance: {geoResult.distance}m (tolerance: 100m)
                </p>
              </div>
            )}

            {captureCoords && !geoResult ? (
              <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 mb-4">
                <p className="text-blue-800 mb-2" style={{ fontWeight: 600, fontSize: '0.9rem' }}>Capture Coordinates</p>
                <p className="text-blue-600 mb-3" style={{ fontSize: '0.82rem' }}>
                  No coordinates found for this customer. Your current GPS location will be saved as the customer's reference coordinates.
                </p>
                <button onClick={handleCaptureAndCheckin} disabled={checkingIn}
                  className="w-full bg-blue-600 text-white py-3 rounded-xl flex items-center justify-center gap-2 disabled:opacity-60"
                  style={{ fontWeight: 600 }}>
                  <Navigation className="w-4 h-4" />
                  {checkingIn ? 'Capturing GPS…' : 'Capture & Check In'}
                </button>
              </div>
            ) : (
              !geoResult && (
                <button onClick={handleCheckin} disabled={checkingIn || !daySession}
                  className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3.5 rounded-xl flex items-center justify-center gap-2 transition disabled:opacity-60"
                  style={{ fontWeight: 600 }}>
                  <Navigation className="w-5 h-5" />
                  {checkingIn ? 'Fetching GPS…' : 'Check In'}
                </button>
              )
            )}

            {!daySession && (
              <p className="text-center text-red-500 mt-2" style={{ fontSize: '0.8rem' }}>Start your day first to check in.</p>
            )}
          </div>
        )}

        {/* Step: Visit */}
        {step === 'visit' && (
          <>
            {/* Geo result banner */}
            {geoResult && (
              <div className={`rounded-xl p-3 flex items-center gap-2 ${geoResult.within ? 'bg-green-50 border border-green-200' : 'bg-amber-50 border border-amber-200'}`}>
                {geoResult.within ? <CheckCircle className="w-4 h-4 text-green-600" /> : <AlertTriangle className="w-4 h-4 text-amber-600" />}
                <span className={geoResult.within ? 'text-green-700' : 'text-amber-700'} style={{ fontSize: '0.82rem', fontWeight: 600 }}>
                  Checked in · {geoResult.distance}m from customer location
                </span>
              </div>
            )}

            {/* Visit Notes */}
            <div className="bg-white rounded-2xl border border-gray-200 p-4 shadow-sm">
              <p className="text-gray-700 mb-3" style={{ fontWeight: 600 }}>Visit Notes</p>
              <textarea
                placeholder="Add notes about your visit (discussions, observations, feedback…)"
                value={visitNotes}
                onChange={e => setVisitNotes(e.target.value)}
                rows={3}
                className="w-full px-3 py-2.5 bg-gray-50 border border-gray-200 rounded-xl resize-none focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
                style={{ fontSize: '0.875rem' }}
              />
            </div>

            {/* Order Section */}
            {!orderSaved ? (
              <div className="bg-white rounded-2xl border border-gray-200 p-4 shadow-sm">
                <div className="flex items-center justify-between mb-3">
                  <p className="text-gray-700" style={{ fontWeight: 600 }}>Order</p>
                  {!showOrder ? (
                    <button onClick={() => setShowOrder(true)}
                      className="flex items-center gap-1.5 bg-blue-50 text-blue-700 px-3 py-1.5 rounded-lg hover:bg-blue-100 transition"
                      style={{ fontSize: '0.8rem', fontWeight: 600 }}>
                      <Plus className="w-4 h-4" />
                      Add Order
                    </button>
                  ) : (
                    <button onClick={() => { setShowOrder(false); setCart([]); }}
                      className="text-gray-400 hover:text-gray-600" style={{ fontSize: '0.8rem' }}>
                      Cancel
                    </button>
                  )}
                </div>

                {showOrder && (
                  <>
                    {/* Product search */}
                    <div className="relative mb-3">
                      <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" />
                      <input type="text" placeholder="Search products…" value={productSearch}
                        onChange={e => setProductSearch(e.target.value)}
                        className="w-full pl-8 pr-3 py-2 bg-gray-50 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
                        style={{ fontSize: '0.82rem' }} />
                    </div>

                    {/* Product list */}
                    <div className="max-h-48 overflow-y-auto space-y-2 mb-3">
                      {filteredProducts.map(product => {
                        const qty = getCartQty(product.product_id);
                        return (
                          <div key={product.product_id} className={`flex items-center gap-2 p-2 rounded-lg border ${qty > 0 ? 'border-blue-200 bg-blue-50' : 'border-gray-100 bg-gray-50'}`}>
                            <div className="flex-1 min-w-0">
                              <p className="text-gray-800 truncate" style={{ fontSize: '0.8rem', fontWeight: 500 }}>{product.product_name}</p>
                              <p className="text-gray-500" style={{ fontSize: '0.72rem' }}>₹{product.selling_price} / {product.uom}</p>
                            </div>
                            <div className="flex items-center gap-1.5 flex-shrink-0">
                              {qty > 0 && (
                                <>
                                  <button onClick={() => adjustQty(product.product_id, -1)}
                                    className="w-6 h-6 bg-white border border-gray-200 rounded-md flex items-center justify-center hover:bg-gray-100">
                                    <Minus className="w-3 h-3 text-gray-600" />
                                  </button>
                                  <span className="w-6 text-center text-gray-900" style={{ fontSize: '0.82rem', fontWeight: 600 }}>{qty}</span>
                                </>
                              )}
                              <button onClick={() => adjustQty(product.product_id, 1)}
                                className="w-6 h-6 bg-blue-600 rounded-md flex items-center justify-center hover:bg-blue-700">
                                <Plus className="w-3 h-3 text-white" />
                              </button>
                            </div>
                          </div>
                        );
                      })}
                    </div>

                    {/* Cart summary */}
                    {cart.length > 0 && (
                      <div className="border-t border-gray-100 pt-3">
                        <div className="space-y-1 mb-3">
                          {cart.map(item => {
                            const p = products.find(pr => pr.product_id === item.product_id)!;
                            return (
                              <div key={item.product_id} className="flex items-center justify-between">
                                <span className="text-gray-700 truncate flex-1" style={{ fontSize: '0.8rem' }}>{p.product_name} × {item.quantity}</span>
                                <div className="flex items-center gap-2">
                                  <span className="text-gray-900" style={{ fontSize: '0.8rem', fontWeight: 600 }}>₹{(p.selling_price * item.quantity).toFixed(0)}</span>
                                  <button onClick={() => setCart(prev => prev.filter(i => i.product_id !== item.product_id))}>
                                    <Trash2 className="w-3.5 h-3.5 text-red-400" />
                                  </button>
                                </div>
                              </div>
                            );
                          })}
                        </div>
                        <div className="flex items-center justify-between bg-blue-50 rounded-lg px-3 py-2 mb-3">
                          <span className="text-blue-700" style={{ fontWeight: 600, fontSize: '0.9rem' }}>Total</span>
                          <span className="text-blue-700" style={{ fontWeight: 700, fontSize: '1rem' }}>₹{cartTotal.toFixed(2)}</span>
                        </div>
                        <button onClick={handleSaveOrder} disabled={savingOrder}
                          className="w-full bg-green-600 hover:bg-green-700 text-white py-2.5 rounded-xl flex items-center justify-center gap-2 transition disabled:opacity-60"
                          style={{ fontWeight: 600 }}>
                          <ShoppingCart className="w-4 h-4" />
                          {savingOrder ? 'Saving…' : 'Save Order'}
                        </button>
                      </div>
                    )}
                  </>
                )}
              </div>
            ) : (
              <div className="bg-green-50 border border-green-200 rounded-xl p-3 flex items-center gap-2">
                <CheckCircle className="w-5 h-5 text-green-600" />
                <div>
                  <p className="text-green-800" style={{ fontWeight: 600, fontSize: '0.85rem' }}>Order Saved</p>
                  <p className="text-green-600" style={{ fontSize: '0.78rem' }}>{savedOrder?.order_number} · ₹{savedOrder?.net_amount?.toFixed(2)}</p>
                </div>
              </div>
            )}

            {/* Checkout */}
            <div className="bg-white rounded-2xl border border-gray-200 p-4 shadow-sm">
              <p className="text-gray-700 mb-3" style={{ fontWeight: 600 }}>Complete Visit</p>
              <div className="flex gap-2 mb-3">
                <button onClick={() => setVisitStatus('completed')}
                  className={`flex-1 py-2.5 rounded-xl border flex items-center justify-center gap-2 transition ${visitStatus === 'completed' ? 'bg-green-600 border-green-600 text-white' : 'bg-white border-gray-200 text-gray-600'}`}
                  style={{ fontWeight: 500, fontSize: '0.875rem' }}>
                  <CheckCircle className="w-4 h-4" />
                  Completed
                </button>
                <button onClick={() => setVisitStatus('skipped')}
                  className={`flex-1 py-2.5 rounded-xl border flex items-center justify-center gap-2 transition ${visitStatus === 'skipped' ? 'bg-gray-600 border-gray-600 text-white' : 'bg-white border-gray-200 text-gray-600'}`}
                  style={{ fontWeight: 500, fontSize: '0.875rem' }}>
                  <XCircle className="w-4 h-4" />
                  Skipped
                </button>
              </div>
              <button onClick={handleCheckout}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 rounded-xl transition"
                style={{ fontWeight: 600 }}>
                Checkout & Continue
              </button>
            </div>
          </>
        )}

        {/* Step: Done */}
        {step === 'done' && (
          <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-sm text-center">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <CheckCircle className="w-8 h-8 text-green-600" />
            </div>
            <p className="text-gray-900 mb-1" style={{ fontWeight: 700, fontSize: '1.1rem' }}>Visit Complete</p>
            <p className="text-gray-500 mb-4" style={{ fontSize: '0.85rem' }}>
              {existingVisit?.visit_status === 'completed' || visitNotes ? 'Visit recorded and synced.' : 'Visit marked as skipped.'}
            </p>
            {orderSaved && savedOrder && (
              <div className="bg-green-50 rounded-xl p-3 mb-4 text-left">
                <p className="text-green-800" style={{ fontWeight: 600, fontSize: '0.85rem' }}>📦 {savedOrder.order_number}</p>
                <p className="text-green-600" style={{ fontSize: '0.78rem' }}>₹{savedOrder.net_amount?.toFixed(2)} · Pending sync</p>
              </div>
            )}
            <button onClick={() => navigate('/rep/customers')}
              className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 rounded-xl transition"
              style={{ fontWeight: 600 }}>
              Next Customer
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
