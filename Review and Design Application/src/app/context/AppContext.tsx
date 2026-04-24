import React, { createContext, useContext, useState, useEffect } from 'react';
import {
  User, DaySession, Visit, Order, Route, OrderStatus,
  users, routes, routeAssignments, customers, products,
  initialOrders, initialVisits, initialDaySessions
} from '../data/mockData';

export interface SyncQueueItem {
  id: string;
  entity_type: 'day_session' | 'visit' | 'order' | 'coordinate';
  entity_ref: string;
  description: string;
  created_at: string;
  status: 'pending' | 'syncing' | 'synced' | 'failed';
  error?: string;
  retry_count: number;
}

interface AppContextType {
  currentUser: User | null;
  login: (username: string, password: string) => { success: boolean; error?: string };
  logout: () => void;

  daySession: DaySession | null;
  startDay: () => void;
  endDay: () => void;

  selectedRoute: Route | null;
  selectRoute: (route: Route) => void;

  visits: Visit[];
  addVisit: (customerId: number) => Visit;
  checkoutVisit: (visitId: number, notes: string, status: 'completed' | 'skipped') => void;

  orders: Order[];
  addOrder: (customerId: number, visitId: number | undefined, items: { product_id: number; quantity: number }[]) => Order;
  updateOrderStatus: (orderId: number, status: OrderStatus, remarks?: string) => void;

  syncQueue: SyncQueueItem[];
  syncPending: () => Promise<void>;

  getAssignedRoutes: (userId: number) => Route[];
  getRouteCustomers: (routeId: number) => typeof customers;
  getTodayVisits: (userId: number) => Visit[];
  getTodayOrders: (userId: number) => Order[];
}

const AppContext = createContext<AppContextType | null>(null);

let visitIdCounter = 100;
let orderIdCounter = 10000;
let sessionIdCounter = 10;
let syncItemIdCounter = 1;

export function AppProvider({ children }: { children: React.ReactNode }) {
  const [currentUser, setCurrentUser] = useState<User | null>(() => {
    try {
      const stored = localStorage.getItem('sfa_user');
      return stored ? JSON.parse(stored) : null;
    } catch { return null; }
  });

  const [daySession, setDaySession] = useState<DaySession | null>(() => {
    try {
      const stored = localStorage.getItem('sfa_day_session');
      if (!stored) return null;
      const s = JSON.parse(stored) as DaySession;
      // Only restore if today
      if (s.business_date === '2026-04-24') return s;
      return null;
    } catch { return null; }
  });

  const [selectedRoute, setSelectedRoute] = useState<Route | null>(() => {
    try {
      const stored = localStorage.getItem('sfa_selected_route');
      return stored ? JSON.parse(stored) : null;
    } catch { return null; }
  });

  const [visits, setVisits] = useState<Visit[]>(() => {
    try {
      const stored = localStorage.getItem('sfa_visits');
      return stored ? JSON.parse(stored) : initialVisits;
    } catch { return initialVisits; }
  });

  const [orders, setOrders] = useState<Order[]>(() => {
    try {
      const stored = localStorage.getItem('sfa_orders');
      return stored ? JSON.parse(stored) : initialOrders;
    } catch { return initialOrders; }
  });

  const [syncQueue, setSyncQueue] = useState<SyncQueueItem[]>([]);

  // Persist to localStorage
  useEffect(() => {
    if (currentUser) localStorage.setItem('sfa_user', JSON.stringify(currentUser));
    else localStorage.removeItem('sfa_user');
  }, [currentUser]);

  useEffect(() => {
    if (daySession) localStorage.setItem('sfa_day_session', JSON.stringify(daySession));
    else localStorage.removeItem('sfa_day_session');
  }, [daySession]);

  useEffect(() => {
    if (selectedRoute) localStorage.setItem('sfa_selected_route', JSON.stringify(selectedRoute));
    else localStorage.removeItem('sfa_selected_route');
  }, [selectedRoute]);

  useEffect(() => {
    localStorage.setItem('sfa_visits', JSON.stringify(visits));
  }, [visits]);

  useEffect(() => {
    localStorage.setItem('sfa_orders', JSON.stringify(orders));
  }, [orders]);

  const login = (username: string, password: string) => {
    const user = users.find(u => u.username === username && u.password === password && u.is_active);
    if (!user) return { success: false, error: 'Invalid username or password' };
    setCurrentUser(user);

    // Restore day session for rep
    if (user.role === 'SALES_REP') {
      const existing = initialDaySessions.find(s => s.rep_user_id === user.user_id && s.status === 'started');
      if (existing) {
        setDaySession(existing);
        const assignedRoute = routes.find(r => r.route_id === existing.selected_route_id) || null;
        setSelectedRoute(assignedRoute);
      }
    }
    return { success: true };
  };

  const logout = () => {
    setCurrentUser(null);
    setDaySession(null);
    setSelectedRoute(null);
    localStorage.removeItem('sfa_user');
    localStorage.removeItem('sfa_day_session');
    localStorage.removeItem('sfa_selected_route');
  };

  const startDay = () => {
    if (!currentUser) return;
    const session: DaySession = {
      day_session_id: ++sessionIdCounter,
      rep_user_id: currentUser.user_id,
      business_date: '2026-04-24',
      start_day_timestamp: new Date().toISOString(),
      start_day_lat: 19.1113,
      start_day_long: 72.8701,
      status: 'started',
      sync_status: 'pending',
    };
    setDaySession(session);
    addToSyncQueue('day_session', `DS-${session.day_session_id}`, 'Start Day session');
  };

  const endDay = () => {
    if (!daySession) return;
    const updated: DaySession = {
      ...daySession,
      end_day_timestamp: new Date().toISOString(),
      end_day_lat: 19.1200,
      end_day_long: 72.8750,
      status: 'ended',
    };
    setDaySession(updated);
    addToSyncQueue('day_session', `DS-end-${daySession.day_session_id}`, 'End Day session');
  };

  const selectRoute = (route: Route) => {
    setSelectedRoute(route);
    if (daySession) {
      setDaySession({ ...daySession, selected_route_id: route.route_id });
    }
  };

  const addVisit = (customerId: number): Visit => {
    const id = ++visitIdCounter;
    const visit: Visit = {
      visit_id: id,
      rep_user_id: currentUser!.user_id,
      day_session_id: daySession!.day_session_id,
      route_id: selectedRoute!.route_id,
      customer_id: customerId,
      checkin_timestamp: new Date().toISOString(),
      visit_status: 'active',
      coordinate_captured_during_visit: false,
      has_order: false,
      sync_status: 'pending',
    };
    setVisits(prev => [...prev, visit]);
    addToSyncQueue('visit', `V-${id}`, `Check-in visit #${id}`);
    return visit;
  };

  const checkoutVisit = (visitId: number, notes: string, status: 'completed' | 'skipped') => {
    setVisits(prev => prev.map(v =>
      v.visit_id === visitId
        ? { ...v, visit_status: status, visit_notes: notes, checkout_timestamp: new Date().toISOString() }
        : v
    ));
    addToSyncQueue('visit', `V-checkout-${visitId}`, `Checkout visit #${visitId}`);
  };

  const addOrder = (
    customerId: number,
    visitId: number | undefined,
    itemInputs: { product_id: number; quantity: number }[]
  ): Order => {
    const id = ++orderIdCounter;
    const items = itemInputs.map((item, idx) => {
      const product = products.find(p => p.product_id === item.product_id)!;
      return {
        order_item_id: idx + 1,
        product_id: item.product_id,
        quantity: item.quantity,
        unit_price: product.selling_price,
        line_total: product.selling_price * item.quantity,
      };
    });
    const gross = items.reduce((sum, i) => sum + i.line_total, 0);
    const customer = customers.find(c => c.customer_id === customerId)!;
    const order: Order = {
      order_id: id,
      order_number: `ORD-20260424-${id}`,
      order_date: '2026-04-24',
      rep_user_id: currentUser!.user_id,
      route_id: selectedRoute!.route_id,
      customer_id: customerId,
      distributor_id: customer.distributor_id,
      visit_id: visitId,
      status: 'Created',
      gross_amount: gross,
      net_amount: gross,
      items,
      sync_status: 'pending',
      created_at: new Date().toISOString(),
    };
    setOrders(prev => [...prev, order]);
    if (visitId) {
      setVisits(prev => prev.map(v => v.visit_id === visitId ? { ...v, has_order: true, order_id: id } : v));
    }
    addToSyncQueue('order', `O-${id}`, `Order ${order.order_number}`);
    return order;
  };

  const updateOrderStatus = (orderId: number, status: OrderStatus, remarks?: string) => {
    setOrders(prev => prev.map(o =>
      o.order_id === orderId ? { ...o, status, distributor_remarks: remarks } : o
    ));
  };

  const addToSyncQueue = (type: SyncQueueItem['entity_type'], ref: string, desc: string) => {
    const item: SyncQueueItem = {
      id: `sq-${++syncItemIdCounter}`,
      entity_type: type,
      entity_ref: ref,
      description: desc,
      created_at: new Date().toISOString(),
      status: 'pending',
      retry_count: 0,
    };
    setSyncQueue(prev => [...prev, item]);
  };

  const syncPending = async () => {
    setSyncQueue(prev => prev.map(i => i.status === 'pending' ? { ...i, status: 'syncing' } : i));
    await new Promise(r => setTimeout(r, 1500));
    setSyncQueue(prev => prev.map(i => i.status === 'syncing' ? { ...i, status: 'synced' } : i));
    setOrders(prev => prev.map(o => o.sync_status === 'pending' ? { ...o, sync_status: 'synced', status: 'Synced' as OrderStatus } : o));
    setVisits(prev => prev.map(v => v.sync_status === 'pending' ? { ...v, sync_status: 'synced' } : v));
  };

  const getAssignedRoutes = (userId: number): Route[] => {
    const assignedIds = routeAssignments
      .filter(a => a.rep_user_id === userId && a.is_active)
      .map(a => a.route_id);
    return routes.filter(r => assignedIds.includes(r.route_id) && r.is_active);
  };

  const getRouteCustomers = (routeId: number) =>
    customers.filter(c => c.route_id === routeId && c.is_active);

  const getTodayVisits = (userId: number): Visit[] =>
    visits.filter(v => v.rep_user_id === userId);

  const getTodayOrders = (userId: number): Order[] =>
    orders.filter(o => o.rep_user_id === userId && o.order_date === '2026-04-24');

  return (
    <AppContext.Provider value={{
      currentUser, login, logout,
      daySession, startDay, endDay,
      selectedRoute, selectRoute,
      visits, addVisit, checkoutVisit,
      orders, addOrder, updateOrderStatus,
      syncQueue, syncPending,
      getAssignedRoutes, getRouteCustomers, getTodayVisits, getTodayOrders,
    }}>
      {children}
    </AppContext.Provider>
  );
}

export function useApp() {
  const ctx = useContext(AppContext);
  if (!ctx) throw new Error('useApp must be used within AppProvider');
  return ctx;
}
