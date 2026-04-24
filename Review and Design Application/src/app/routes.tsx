import React from 'react';
import { createBrowserRouter, Navigate } from 'react-router';
import {
  LayoutDashboard, Users, Route, Package, Truck,
  BarChart2, TrendingUp, MapPin, Upload, ShoppingBag
} from 'lucide-react';

import { useApp } from './context/AppContext';
import Login from './pages/Login';
import MobileLayout from './components/MobileLayout';
import DesktopLayout from './components/DesktopLayout';
import RepHome from './pages/rep/RepHome';
import RouteSelection from './pages/rep/RouteSelection';
import CustomerList from './pages/rep/CustomerList';
import CustomerDetail from './pages/rep/CustomerDetail';
import PendingSync from './pages/rep/PendingSync';
import RepProfile from './pages/rep/RepProfile';
import TSIApp from './pages/tsi/TSIApp';
import AdminApp from './pages/admin/AdminApp';
import DistributorApp from './pages/distributor/DistributorApp';

// Root redirect based on role
function RootRedirect() {
  const { currentUser } = useApp();
  if (!currentUser) return <Navigate to="/login" replace />;
  switch (currentUser.role) {
    case 'SALES_REP': return <Navigate to="/rep" replace />;
    case 'TSI': return <Navigate to="/tsi" replace />;
    case 'ADMIN': return <Navigate to="/admin" replace />;
    case 'DISTRIBUTOR': return <Navigate to="/distributor" replace />;
    default: return <Navigate to="/login" replace />;
  }
}

// TSI layout wrapper
function TSILayout() {
  const navItems = [
    { to: '/tsi', label: 'Dashboard & Reports', icon: <LayoutDashboard className="w-4 h-4" /> },
  ];
  return <DesktopLayout navItems={navItems} title="TSI Portal – Territory Sales Incharge" />;
}

// Admin layout wrapper
function AdminLayout() {
  const navItems = [
    { to: '/admin', label: 'Admin Console', icon: <LayoutDashboard className="w-4 h-4" /> },
  ];
  return <DesktopLayout navItems={navItems} title="Admin Console" />;
}

// Distributor layout wrapper
function DistributorLayout() {
  const navItems = [
    { to: '/distributor', label: 'Order Management', icon: <Package className="w-4 h-4" /> },
  ];
  return <DesktopLayout navItems={navItems} title="Distributor Portal" />;
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <RootRedirect />,
  },
  {
    path: '/login',
    element: <Login />,
  },
  // Sales Rep (mobile)
  {
    path: '/rep',
    element: <MobileLayout />,
    children: [
      { index: true, element: <RepHome /> },
      { path: 'route-select', element: <RouteSelection /> },
      { path: 'customers', element: <CustomerList /> },
      { path: 'customers/:customerId', element: <CustomerDetail /> },
      { path: 'sync', element: <PendingSync /> },
      { path: 'profile', element: <RepProfile /> },
    ],
  },
  // TSI
  {
    path: '/tsi',
    element: <TSILayout />,
    children: [
      { index: true, element: <TSIApp /> },
    ],
  },
  // Admin
  {
    path: '/admin',
    element: <AdminLayout />,
    children: [
      { index: true, element: <AdminApp /> },
    ],
  },
  // Distributor
  {
    path: '/distributor',
    element: <DistributorLayout />,
    children: [
      { index: true, element: <DistributorApp /> },
    ],
  },
]);
