export type UserRole = 'ADMIN' | 'TSI' | 'SALES_REP' | 'DISTRIBUTOR';

export interface User {
  user_id: number;
  username: string;
  password: string;
  full_name: string;
  mobile_number: string;
  email: string;
  role: UserRole;
  role_name: string;
  manager_id?: number;
  distributor_id?: number;
  is_active: boolean;
}

export interface Territory {
  territory_id: number;
  territory_code: string;
  territory_name: string;
  tsi_user_id: number;
  is_active: boolean;
}

export interface Distributor {
  distributor_id: number;
  distributor_code: string;
  distributor_name: string;
  contact_person: string;
  mobile_number: string;
  address: string;
  is_active: boolean;
}

export interface Route {
  route_id: number;
  route_code: string;
  route_name: string;
  territory_id: number;
  distributor_id: number;
  is_active: boolean;
}

export interface RouteAssignment {
  route_assignment_id: number;
  route_id: number;
  rep_user_id: number;
  start_date: string;
  end_date?: string;
  is_active: boolean;
}

export interface Customer {
  customer_id: number;
  customer_code: string;
  customer_name: string;
  contact_person: string;
  mobile_number: string;
  address_line_1: string;
  locality: string;
  city: string;
  state: string;
  pincode: string;
  route_id: number;
  distributor_id: number;
  outlet_type: string;
  gst_number?: string;
  latitude?: number;
  longitude?: number;
  coordinate_capture_source?: string;
  is_active: boolean;
}

export interface Product {
  product_id: number;
  product_code: string;
  product_name: string;
  uom: string;
  selling_price: number;
  mrp: number;
  category: string;
  is_active: boolean;
}

export interface OrderItem {
  order_item_id: number;
  product_id: number;
  quantity: number;
  unit_price: number;
  line_total: number;
}

export type OrderStatus = 'Created' | 'Synced' | 'Accepted' | 'Dispatched' | 'Delivered' | 'Cancelled';

export interface Order {
  order_id: number;
  order_number: string;
  order_date: string;
  rep_user_id: number;
  route_id: number;
  customer_id: number;
  distributor_id: number;
  visit_id?: number;
  status: OrderStatus;
  gross_amount: number;
  net_amount: number;
  items: OrderItem[];
  sync_status: 'synced' | 'pending' | 'failed';
  created_at: string;
  distributor_remarks?: string;
}

export type VisitStatus = 'completed' | 'skipped' | 'active';

export interface Visit {
  visit_id: number;
  rep_user_id: number;
  day_session_id: number;
  route_id: number;
  customer_id: number;
  checkin_timestamp: string;
  checkout_timestamp?: string;
  visit_status: VisitStatus;
  visit_notes?: string;
  within_tolerance_flag?: boolean;
  geo_distance_meters?: number;
  coordinate_captured_during_visit: boolean;
  has_order: boolean;
  order_id?: number;
  sync_status: 'synced' | 'pending';
}

export interface DaySession {
  day_session_id: number;
  rep_user_id: number;
  business_date: string;
  start_day_timestamp: string;
  start_day_lat: number;
  start_day_long: number;
  end_day_timestamp?: string;
  end_day_lat?: number;
  end_day_long?: number;
  selected_route_id?: number;
  status: 'started' | 'ended';
  sync_status: 'synced' | 'pending';
}

// ========================
// MASTER DATA
// ========================

export const territories: Territory[] = [
  { territory_id: 1, territory_code: 'TER001', territory_name: 'Mumbai North', tsi_user_id: 2, is_active: true },
  { territory_id: 2, territory_code: 'TER002', territory_name: 'Mumbai South', tsi_user_id: 3, is_active: true },
  { territory_id: 3, territory_code: 'TER003', territory_name: 'Thane Region', tsi_user_id: 2, is_active: true },
];

export const distributors: Distributor[] = [
  { distributor_id: 1, distributor_code: 'DIST001', distributor_name: 'Mumbai Central Distributors', contact_person: 'Anand Mehta', mobile_number: '9820001111', address: '45 Linking Road, Bandra, Mumbai – 400050', is_active: true },
  { distributor_id: 2, distributor_code: 'DIST002', distributor_name: 'Western Region Supplies', contact_person: 'Venkat Rao', mobile_number: '9820002222', address: '12 SV Road, Andheri West, Mumbai – 400058', is_active: true },
  { distributor_id: 3, distributor_code: 'DIST003', distributor_name: 'Thane Distributors Pvt Ltd', contact_person: 'Sunil Joshi', mobile_number: '9820003333', address: '8 Gokhale Road, Thane West – 400601', is_active: true },
];

export const routes: Route[] = [
  { route_id: 1, route_code: 'RT001', route_name: 'Andheri East – A', territory_id: 1, distributor_id: 1, is_active: true },
  { route_id: 2, route_code: 'RT002', route_name: 'Andheri West – B', territory_id: 1, distributor_id: 2, is_active: true },
  { route_id: 3, route_code: 'RT003', route_name: 'Bandra Market Circuit', territory_id: 1, distributor_id: 2, is_active: true },
  { route_id: 4, route_code: 'RT004', route_name: 'Dadar Central', territory_id: 2, distributor_id: 1, is_active: true },
  { route_id: 5, route_code: 'RT005', route_name: 'Thane West Loop', territory_id: 3, distributor_id: 3, is_active: true },
  { route_id: 6, route_code: 'RT006', route_name: 'Thane East Circuit', territory_id: 3, distributor_id: 3, is_active: true },
];

export const routeAssignments: RouteAssignment[] = [
  { route_assignment_id: 1, route_id: 1, rep_user_id: 4, start_date: '2026-01-01', is_active: true },
  { route_assignment_id: 2, route_id: 2, rep_user_id: 4, start_date: '2026-01-01', is_active: true },
  { route_assignment_id: 3, route_id: 3, rep_user_id: 5, start_date: '2026-01-01', is_active: true },
  { route_assignment_id: 4, route_id: 4, rep_user_id: 6, start_date: '2026-01-01', is_active: true },
  { route_assignment_id: 5, route_id: 5, rep_user_id: 7, start_date: '2026-01-01', is_active: true },
  { route_assignment_id: 6, route_id: 6, rep_user_id: 8, start_date: '2026-01-01', is_active: true },
];

export const users: User[] = [
  { user_id: 1, username: 'admin', password: 'admin123', full_name: 'Rajesh Kumar', mobile_number: '9900001111', email: 'admin@sfaapp.com', role: 'ADMIN', role_name: 'Admin', is_active: true },
  { user_id: 2, username: 'tsi01', password: 'tsi123', full_name: 'Priya Sharma', mobile_number: '9900002222', email: 'priya.sharma@sfaapp.com', role: 'TSI', role_name: 'Territory Sales Incharge', is_active: true },
  { user_id: 3, username: 'tsi02', password: 'tsi123', full_name: 'Amit Patel', mobile_number: '9900003333', email: 'amit.patel@sfaapp.com', role: 'TSI', role_name: 'Territory Sales Incharge', is_active: true },
  { user_id: 4, username: 'rep01', password: 'rep123', full_name: 'Suresh Nair', mobile_number: '9900004444', email: 'suresh.nair@sfaapp.com', role: 'SALES_REP', role_name: 'Sales Representative', manager_id: 2, is_active: true },
  { user_id: 5, username: 'rep02', password: 'rep123', full_name: 'Deepa Menon', mobile_number: '9900005555', email: 'deepa.menon@sfaapp.com', role: 'SALES_REP', role_name: 'Sales Representative', manager_id: 2, is_active: true },
  { user_id: 6, username: 'rep03', password: 'rep123', full_name: 'Karan Singh', mobile_number: '9900006666', email: 'karan.singh@sfaapp.com', role: 'SALES_REP', role_name: 'Sales Representative', manager_id: 3, is_active: true },
  { user_id: 7, username: 'rep04', password: 'rep123', full_name: 'Ravi Kumar', mobile_number: '9900007777', email: 'ravi.kumar@sfaapp.com', role: 'SALES_REP', role_name: 'Sales Representative', manager_id: 2, is_active: true },
  { user_id: 8, username: 'rep05', password: 'rep123', full_name: 'Sneha Patil', mobile_number: '9900008888', email: 'sneha.patil@sfaapp.com', role: 'SALES_REP', role_name: 'Sales Representative', manager_id: 3, is_active: true },
  { user_id: 9, username: 'dist01', password: 'dist123', full_name: 'Anand Mehta', mobile_number: '9820001111', email: 'dist1@sfaapp.com', role: 'DISTRIBUTOR', role_name: 'Distributor User', distributor_id: 1, is_active: true },
  { user_id: 10, username: 'dist02', password: 'dist123', full_name: 'Venkat Rao', mobile_number: '9820002222', email: 'dist2@sfaapp.com', role: 'DISTRIBUTOR', role_name: 'Distributor User', distributor_id: 2, is_active: true },
  { user_id: 11, username: 'dist03', password: 'dist123', full_name: 'Sunil Joshi', mobile_number: '9820003333', email: 'dist3@sfaapp.com', role: 'DISTRIBUTOR', role_name: 'Distributor User', distributor_id: 3, is_active: true },
];

export const customers: Customer[] = [
  // RT001 – Andheri East A (rep01, DIST001)
  { customer_id: 101, customer_code: 'C101', customer_name: 'Sharma General Store', contact_person: 'Ramesh Sharma', mobile_number: '9821001001', address_line_1: '12 Marol Naka', locality: 'Marol Naka', city: 'Mumbai', state: 'Maharashtra', pincode: '400059', route_id: 1, distributor_id: 1, outlet_type: 'General Trade', latitude: 19.1113, longitude: 72.8701, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 102, customer_code: 'C102', customer_name: 'Krishna Kirana Traders', contact_person: 'Mukesh Krishna', mobile_number: '9821001002', address_line_1: '45 MIDC Road', locality: 'MIDC', city: 'Mumbai', state: 'Maharashtra', pincode: '400093', route_id: 1, distributor_id: 1, outlet_type: 'General Trade', latitude: 19.1162, longitude: 72.8756, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 103, customer_code: 'C103', customer_name: 'Patel Kirana Center', contact_person: 'Dilip Patel', mobile_number: '9821001003', address_line_1: '8 Chakala', locality: 'Chakala', city: 'Mumbai', state: 'Maharashtra', pincode: '400099', route_id: 1, distributor_id: 1, outlet_type: 'Modern Trade', is_active: true },
  { customer_id: 104, customer_code: 'C104', customer_name: 'Andheri Supermart', contact_person: 'Vijay Anand', mobile_number: '9821001004', address_line_1: '22 JB Nagar', locality: 'JB Nagar', city: 'Mumbai', state: 'Maharashtra', pincode: '400059', route_id: 1, distributor_id: 1, outlet_type: 'Modern Trade', latitude: 19.1197, longitude: 72.8684, coordinate_capture_source: 'rep_capture', is_active: true },
  { customer_id: 105, customer_code: 'C105', customer_name: 'Lucky General Store', contact_person: 'Rajesh Lucky', mobile_number: '9821001005', address_line_1: '67 Saki Vihar Rd', locality: 'Saki Naka', city: 'Mumbai', state: 'Maharashtra', pincode: '400072', route_id: 1, distributor_id: 1, outlet_type: 'General Trade', latitude: 19.1089, longitude: 72.8899, coordinate_capture_source: 'admin_upload', is_active: true },
  // RT002 – Andheri West B (rep01, DIST002)
  { customer_id: 201, customer_code: 'C201', customer_name: 'West Side Provisions', contact_person: 'Santosh Kumar', mobile_number: '9821002001', address_line_1: '23 SV Road', locality: 'SV Road', city: 'Mumbai', state: 'Maharashtra', pincode: '400058', route_id: 2, distributor_id: 2, outlet_type: 'General Trade', latitude: 19.1197, longitude: 72.8468, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 202, customer_code: 'C202', customer_name: 'Andheri Foodmart', contact_person: 'Geeta Devi', mobile_number: '9821002002', address_line_1: '5 Lokhandwala Complex', locality: 'Lokhandwala', city: 'Mumbai', state: 'Maharashtra', pincode: '400053', route_id: 2, distributor_id: 2, outlet_type: 'Modern Trade', latitude: 19.1361, longitude: 72.8346, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 203, customer_code: 'C203', customer_name: 'Four Roads Corner Shop', contact_person: 'Pramod Jain', mobile_number: '9821002003', address_line_1: '4 Road Junction, Versova', locality: 'Versova', city: 'Mumbai', state: 'Maharashtra', pincode: '400053', route_id: 2, distributor_id: 2, outlet_type: 'General Trade', is_active: true },
  { customer_id: 204, customer_code: 'C204', customer_name: 'Juhu Bazaar Store', contact_person: 'Manish Shah', mobile_number: '9821002004', address_line_1: '18 Juhu Circle', locality: 'Juhu', city: 'Mumbai', state: 'Maharashtra', pincode: '400049', route_id: 2, distributor_id: 2, outlet_type: 'General Trade', latitude: 19.1075, longitude: 72.8264, coordinate_capture_source: 'admin_upload', is_active: true },
  // RT003 – Bandra Market (rep02, DIST002)
  { customer_id: 301, customer_code: 'C301', customer_name: 'Bandra Fresh & Best', contact_person: 'Cyrus Mistry', mobile_number: '9821003001', address_line_1: '10 Hill Road', locality: 'Hill Road', city: 'Mumbai', state: 'Maharashtra', pincode: '400050', route_id: 3, distributor_id: 2, outlet_type: 'Modern Trade', latitude: 19.0596, longitude: 72.8295, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 302, customer_code: 'C302', customer_name: 'Market Lane Provisions', contact_person: "Francis D'Souza", mobile_number: '9821003002', address_line_1: '7 Linking Road', locality: 'Linking Road', city: 'Mumbai', state: 'Maharashtra', pincode: '400050', route_id: 3, distributor_id: 2, outlet_type: 'General Trade', latitude: 19.0624, longitude: 72.8362, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 303, customer_code: 'C303', customer_name: 'Carter Road Stores', contact_person: 'Anita Pereira', mobile_number: '9821003003', address_line_1: '2 Carter Road', locality: 'Carter Road', city: 'Mumbai', state: 'Maharashtra', pincode: '400050', route_id: 3, distributor_id: 2, outlet_type: 'General Trade', is_active: true },
  // RT004 – Dadar Central (rep03, DIST001)
  { customer_id: 401, customer_code: 'C401', customer_name: 'Dadar Market Traders', contact_person: 'Sanjay Jadhav', mobile_number: '9821004001', address_line_1: '15 Dadar TT Circle', locality: 'Dadar TT', city: 'Mumbai', state: 'Maharashtra', pincode: '400014', route_id: 4, distributor_id: 1, outlet_type: 'General Trade', latitude: 19.0196, longitude: 72.8419, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 402, customer_code: 'C402', customer_name: 'Shivaji Park Kirana', contact_person: 'Prakash Rane', mobile_number: '9821004002', address_line_1: '3 Shivaji Park Rd', locality: 'Shivaji Park', city: 'Mumbai', state: 'Maharashtra', pincode: '400028', route_id: 4, distributor_id: 1, outlet_type: 'General Trade', latitude: 19.0277, longitude: 72.8372, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 403, customer_code: 'C403', customer_name: 'Dadar West Provisions', contact_person: 'Monica Fernandez', mobile_number: '9821004003', address_line_1: '25 Dadar West', locality: 'Dadar West', city: 'Mumbai', state: 'Maharashtra', pincode: '400028', route_id: 4, distributor_id: 1, outlet_type: 'Modern Trade', latitude: 19.0166, longitude: 72.8322, coordinate_capture_source: 'rep_capture', is_active: true },
  // RT005 – Thane West Loop (rep04, DIST003)
  { customer_id: 501, customer_code: 'C501', customer_name: 'Thane West Superstore', contact_person: 'Rahul Bhat', mobile_number: '9821005001', address_line_1: '12 Gokhale Road', locality: 'Gokhale Rd', city: 'Thane', state: 'Maharashtra', pincode: '400601', route_id: 5, distributor_id: 3, outlet_type: 'Modern Trade', latitude: 19.2131, longitude: 72.9785, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 502, customer_code: 'C502', customer_name: 'Vasant Stores', contact_person: 'Vasant Desai', mobile_number: '9821005002', address_line_1: '7 Station Road', locality: 'Station Road', city: 'Thane', state: 'Maharashtra', pincode: '400601', route_id: 5, distributor_id: 3, outlet_type: 'General Trade', latitude: 19.1989, longitude: 72.9671, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 503, customer_code: 'C503', customer_name: 'Hiranandani Provision', contact_person: 'Nilesh Hiranandani', mobile_number: '9821005003', address_line_1: '4 Hiranandani Estate', locality: 'Hiranandani', city: 'Thane', state: 'Maharashtra', pincode: '400607', route_id: 5, distributor_id: 3, outlet_type: 'Modern Trade', latitude: 19.2572, longitude: 73.0127, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 504, customer_code: 'C504', customer_name: 'Naupada Kirana', contact_person: 'Hemant Naik', mobile_number: '9821005004', address_line_1: '9 Naupada, Thane', locality: 'Naupada', city: 'Thane', state: 'Maharashtra', pincode: '400602', route_id: 5, distributor_id: 3, outlet_type: 'General Trade', is_active: true },
  // RT006 – Thane East Circuit (rep05, DIST003)
  { customer_id: 601, customer_code: 'C601', customer_name: 'Kopri Provisions', contact_person: 'Anil Kopkar', mobile_number: '9821006001', address_line_1: '2 Kopri Colony', locality: 'Kopri', city: 'Thane', state: 'Maharashtra', pincode: '400603', route_id: 6, distributor_id: 3, outlet_type: 'General Trade', latitude: 19.2022, longitude: 73.0023, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 602, customer_code: 'C602', customer_name: 'Rabodi Market Store', contact_person: 'Suresh Gawde', mobile_number: '9821006002', address_line_1: '11 Rabodi', locality: 'Rabodi', city: 'Thane', state: 'Maharashtra', pincode: '400601', route_id: 6, distributor_id: 3, outlet_type: 'General Trade', latitude: 19.2047, longitude: 72.9824, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 603, customer_code: 'C603', customer_name: 'Majiwada Fresh Stores', contact_person: 'Kishore Patkar', mobile_number: '9821006003', address_line_1: '5 Majiwada Junction', locality: 'Majiwada', city: 'Thane', state: 'Maharashtra', pincode: '400601', route_id: 6, distributor_id: 3, outlet_type: 'Modern Trade', latitude: 19.2391, longitude: 72.9873, coordinate_capture_source: 'admin_upload', is_active: true },
  { customer_id: 604, customer_code: 'C604', customer_name: 'Wagle Estate Kirana', contact_person: 'Yogesh Wagle', mobile_number: '9821006004', address_line_1: '34 Wagle Industrial Estate', locality: 'Wagle Estate', city: 'Thane', state: 'Maharashtra', pincode: '400604', route_id: 6, distributor_id: 3, outlet_type: 'General Trade', is_active: true },
];

export const products: Product[] = [
  { product_id: 1001, product_code: 'PRD001', product_name: 'Shampoo 200ml', uom: 'Bottle', selling_price: 145.00, mrp: 165.00, category: 'Hair Care', is_active: true },
  { product_id: 1002, product_code: 'PRD002', product_name: 'Hair Conditioner 180ml', uom: 'Bottle', selling_price: 125.00, mrp: 145.00, category: 'Hair Care', is_active: true },
  { product_id: 1003, product_code: 'PRD003', product_name: 'Bathing Soap Pack (4s)', uom: 'Pack', selling_price: 78.00, mrp: 92.00, category: 'Personal Care', is_active: true },
  { product_id: 1004, product_code: 'PRD004', product_name: 'Toothpaste 150g', uom: 'Tube', selling_price: 68.00, mrp: 80.00, category: 'Oral Care', is_active: true },
  { product_id: 1005, product_code: 'PRD005', product_name: 'Dishwash Liquid 500ml', uom: 'Bottle', selling_price: 95.00, mrp: 110.00, category: 'Home Care', is_active: true },
  { product_id: 1006, product_code: 'PRD006', product_name: 'Floor Cleaner 1L', uom: 'Bottle', selling_price: 115.00, mrp: 135.00, category: 'Home Care', is_active: true },
  { product_id: 1007, product_code: 'PRD007', product_name: 'Hand Wash 250ml', uom: 'Bottle', selling_price: 85.00, mrp: 99.00, category: 'Personal Care', is_active: true },
  { product_id: 1008, product_code: 'PRD008', product_name: 'Face Wash 100ml', uom: 'Tube', selling_price: 110.00, mrp: 130.00, category: 'Skin Care', is_active: true },
  { product_id: 1009, product_code: 'PRD009', product_name: 'Body Lotion 200ml', uom: 'Bottle', selling_price: 155.00, mrp: 180.00, category: 'Skin Care', is_active: true },
  { product_id: 1010, product_code: 'PRD010', product_name: 'Talcum Powder 300g', uom: 'Pack', selling_price: 88.00, mrp: 105.00, category: 'Personal Care', is_active: true },
  { product_id: 1011, product_code: 'PRD011', product_name: 'Shaving Foam 200ml', uom: 'Can', selling_price: 135.00, mrp: 155.00, category: 'Personal Care', is_active: true },
  { product_id: 1012, product_code: 'PRD012', product_name: 'Detergent Powder 1kg', uom: 'Pack', selling_price: 175.00, mrp: 195.00, category: 'Home Care', is_active: true },
  { product_id: 1013, product_code: 'PRD013', product_name: 'Room Freshener 300ml', uom: 'Can', selling_price: 145.00, mrp: 168.00, category: 'Home Care', is_active: true },
  { product_id: 1014, product_code: 'PRD014', product_name: 'Mosquito Repellent 45ml', uom: 'Bottle', selling_price: 58.00, mrp: 70.00, category: 'Home Care', is_active: true },
  { product_id: 1015, product_code: 'PRD015', product_name: 'Coconut Oil 500ml', uom: 'Bottle', selling_price: 128.00, mrp: 148.00, category: 'Hair Care', is_active: true },
];

export const initialOrders: Order[] = [
  { order_id: 9001, order_number: 'ORD-20260424-9001', order_date: '2026-04-24', rep_user_id: 4, route_id: 1, customer_id: 101, distributor_id: 1, status: 'Synced', gross_amount: 1450.00, net_amount: 1450.00, items: [{ order_item_id: 1, product_id: 1001, quantity: 10, unit_price: 145, line_total: 1450 }], sync_status: 'synced', created_at: '2026-04-24T09:30:00Z' },
  { order_id: 9002, order_number: 'ORD-20260424-9002', order_date: '2026-04-24', rep_user_id: 4, route_id: 1, customer_id: 102, distributor_id: 1, status: 'Synced', gross_amount: 2336.00, net_amount: 2336.00, items: [{ order_item_id: 2, product_id: 1003, quantity: 12, unit_price: 78, line_total: 936 }, { order_item_id: 3, product_id: 1012, quantity: 8, unit_price: 175, line_total: 1400 }], sync_status: 'synced', created_at: '2026-04-24T10:15:00Z' },
  { order_id: 9003, order_number: 'ORD-20260424-9003', order_date: '2026-04-24', rep_user_id: 5, route_id: 3, customer_id: 301, distributor_id: 2, status: 'Accepted', gross_amount: 1790.00, net_amount: 1790.00, items: [{ order_item_id: 4, product_id: 1005, quantity: 8, unit_price: 95, line_total: 760 }, { order_item_id: 5, product_id: 1006, quantity: 6, unit_price: 115, line_total: 690 }, { order_item_id: 6, product_id: 1007, quantity: 4, unit_price: 85, line_total: 340 }], sync_status: 'synced', created_at: '2026-04-24T09:45:00Z' },
  { order_id: 8001, order_number: 'ORD-20260423-8001', order_date: '2026-04-23', rep_user_id: 4, route_id: 1, customer_id: 103, distributor_id: 1, status: 'Dispatched', gross_amount: 1765.00, net_amount: 1765.00, items: [{ order_item_id: 7, product_id: 1009, quantity: 5, unit_price: 155, line_total: 775 }, { order_item_id: 8, product_id: 1008, quantity: 5, unit_price: 110, line_total: 550 }, { order_item_id: 9, product_id: 1010, quantity: 5, unit_price: 88, line_total: 440 }], sync_status: 'synced', created_at: '2026-04-23T10:20:00Z' },
  { order_id: 8002, order_number: 'ORD-20260423-8002', order_date: '2026-04-23', rep_user_id: 6, route_id: 4, customer_id: 401, distributor_id: 1, status: 'Delivered', gross_amount: 2452.00, net_amount: 2452.00, items: [{ order_item_id: 10, product_id: 1012, quantity: 10, unit_price: 175, line_total: 1750 }, { order_item_id: 11, product_id: 1003, quantity: 9, unit_price: 78, line_total: 702 }], sync_status: 'synced', created_at: '2026-04-23T11:30:00Z' },
  { order_id: 8003, order_number: 'ORD-20260423-8003', order_date: '2026-04-23', rep_user_id: 7, route_id: 5, customer_id: 501, distributor_id: 3, status: 'Delivered', gross_amount: 3188.00, net_amount: 3188.00, items: [{ order_item_id: 12, product_id: 1001, quantity: 12, unit_price: 145, line_total: 1740 }, { order_item_id: 13, product_id: 1015, quantity: 6, unit_price: 128, line_total: 768 }, { order_item_id: 14, product_id: 1007, quantity: 8, unit_price: 85, line_total: 680 }], sync_status: 'synced', created_at: '2026-04-23T10:00:00Z' },
  { order_id: 8004, order_number: 'ORD-20260423-8004', order_date: '2026-04-23', rep_user_id: 8, route_id: 6, customer_id: 601, distributor_id: 3, status: 'Accepted', gross_amount: 1396.00, net_amount: 1396.00, items: [{ order_item_id: 15, product_id: 1004, quantity: 12, unit_price: 68, line_total: 816 }, { order_item_id: 16, product_id: 1013, quantity: 4, unit_price: 145, line_total: 580 }], sync_status: 'synced', created_at: '2026-04-23T12:00:00Z' },
  { order_id: 7001, order_number: 'ORD-20260422-7001', order_date: '2026-04-22', rep_user_id: 4, route_id: 2, customer_id: 201, distributor_id: 2, status: 'Delivered', gross_amount: 2080.00, net_amount: 2080.00, items: [{ order_item_id: 17, product_id: 1002, quantity: 8, unit_price: 125, line_total: 1000 }, { order_item_id: 18, product_id: 1011, quantity: 8, unit_price: 135, line_total: 1080 }], sync_status: 'synced', created_at: '2026-04-22T09:30:00Z' },
  { order_id: 7002, order_number: 'ORD-20260422-7002', order_date: '2026-04-22', rep_user_id: 5, route_id: 3, customer_id: 302, distributor_id: 2, status: 'Delivered', gross_amount: 1890.00, net_amount: 1890.00, items: [{ order_item_id: 19, product_id: 1006, quantity: 6, unit_price: 115, line_total: 690 }, { order_item_id: 20, product_id: 1005, quantity: 6, unit_price: 95, line_total: 570 }, { order_item_id: 21, product_id: 1014, quantity: 9, unit_price: 58, line_total: 522 }], sync_status: 'synced', created_at: '2026-04-22T10:45:00Z' },
  { order_id: 7003, order_number: 'ORD-20260422-7003', order_date: '2026-04-22', rep_user_id: 6, route_id: 4, customer_id: 402, distributor_id: 1, status: 'Delivered', gross_amount: 1510.00, net_amount: 1510.00, items: [{ order_item_id: 22, product_id: 1001, quantity: 6, unit_price: 145, line_total: 870 }, { order_item_id: 23, product_id: 1015, quantity: 5, unit_price: 128, line_total: 640 }], sync_status: 'synced', created_at: '2026-04-22T11:30:00Z' },
  { order_id: 7004, order_number: 'ORD-20260422-7004', order_date: '2026-04-22', rep_user_id: 7, route_id: 5, customer_id: 502, distributor_id: 3, status: 'Delivered', gross_amount: 2881.00, net_amount: 2881.00, items: [{ order_item_id: 24, product_id: 1012, quantity: 8, unit_price: 175, line_total: 1400 }, { order_item_id: 25, product_id: 1003, quantity: 7, unit_price: 78, line_total: 546 }, { order_item_id: 26, product_id: 1007, quantity: 11, unit_price: 85, line_total: 935 }], sync_status: 'synced', created_at: '2026-04-22T09:00:00Z' },
  { order_id: 7005, order_number: 'ORD-20260421-7005', order_date: '2026-04-21', rep_user_id: 4, route_id: 1, customer_id: 104, distributor_id: 1, status: 'Delivered', gross_amount: 1340.00, net_amount: 1340.00, items: [{ order_item_id: 27, product_id: 1007, quantity: 8, unit_price: 85, line_total: 680 }, { order_item_id: 28, product_id: 1014, quantity: 10, unit_price: 58, line_total: 580 }, { order_item_id: 29, product_id: 1004, quantity: 1, unit_price: 68, line_total: 68 }], sync_status: 'synced', created_at: '2026-04-21T10:00:00Z' },
  { order_id: 7006, order_number: 'ORD-20260421-7006', order_date: '2026-04-21', rep_user_id: 8, route_id: 6, customer_id: 602, distributor_id: 3, status: 'Dispatched', gross_amount: 1620.00, net_amount: 1620.00, items: [{ order_item_id: 30, product_id: 1001, quantity: 6, unit_price: 145, line_total: 870 }, { order_item_id: 31, product_id: 1002, quantity: 6, unit_price: 125, line_total: 750 }], sync_status: 'synced', created_at: '2026-04-21T11:00:00Z' },
];

export const initialVisits: Visit[] = [
  { visit_id: 1, rep_user_id: 4, day_session_id: 1, route_id: 1, customer_id: 101, checkin_timestamp: '2026-04-24T09:15:00Z', checkout_timestamp: '2026-04-24T09:38:00Z', visit_status: 'completed', visit_notes: 'Good response. Placed order for Shampoo.', within_tolerance_flag: true, geo_distance_meters: 18.5, coordinate_captured_during_visit: false, has_order: true, order_id: 9001, sync_status: 'synced' },
  { visit_id: 2, rep_user_id: 4, day_session_id: 1, route_id: 1, customer_id: 102, checkin_timestamp: '2026-04-24T10:00:00Z', checkout_timestamp: '2026-04-24T10:22:00Z', visit_status: 'completed', visit_notes: 'Owner discussed secondary display placement.', within_tolerance_flag: true, geo_distance_meters: 45.2, coordinate_captured_during_visit: false, has_order: true, order_id: 9002, sync_status: 'synced' },
  { visit_id: 3, rep_user_id: 4, day_session_id: 1, route_id: 1, customer_id: 104, checkin_timestamp: '2026-04-24T11:30:00Z', checkout_timestamp: '2026-04-24T11:48:00Z', visit_status: 'skipped', visit_notes: 'Shop closed. Will revisit tomorrow.', coordinate_captured_during_visit: false, has_order: false, sync_status: 'synced' },
  { visit_id: 4, rep_user_id: 5, day_session_id: 2, route_id: 3, customer_id: 301, checkin_timestamp: '2026-04-24T09:30:00Z', checkout_timestamp: '2026-04-24T09:52:00Z', visit_status: 'completed', visit_notes: 'Good meeting. Interested in display scheme.', within_tolerance_flag: true, geo_distance_meters: 12.3, coordinate_captured_during_visit: false, has_order: true, order_id: 9003, sync_status: 'synced' },
  { visit_id: 5, rep_user_id: 5, day_session_id: 2, route_id: 3, customer_id: 302, checkin_timestamp: '2026-04-24T10:45:00Z', checkout_timestamp: '2026-04-24T11:05:00Z', visit_status: 'completed', visit_notes: 'Discussed upcoming festival season stock.', within_tolerance_flag: true, geo_distance_meters: 28.7, coordinate_captured_during_visit: false, has_order: false, sync_status: 'synced' },
];

export const initialDaySessions: DaySession[] = [
  { day_session_id: 1, rep_user_id: 4, business_date: '2026-04-24', start_day_timestamp: '2026-04-24T08:55:00Z', start_day_lat: 19.1113, start_day_long: 72.8701, selected_route_id: 1, status: 'started', sync_status: 'synced' },
  { day_session_id: 2, rep_user_id: 5, business_date: '2026-04-24', start_day_timestamp: '2026-04-24T09:10:00Z', start_day_lat: 19.0596, start_day_long: 72.8295, selected_route_id: 3, status: 'started', sync_status: 'synced' },
];

export const weeklyOrderStats = [
  { date: 'Apr 18', orders: 12, amount: 18450 },
  { date: 'Apr 19', orders: 9, amount: 14200 },
  { date: 'Apr 20', orders: 0, amount: 0 },
  { date: 'Apr 21', orders: 15, amount: 23700 },
  { date: 'Apr 22', orders: 11, amount: 17650 },
  { date: 'Apr 23', orders: 14, amount: 22800 },
  { date: 'Apr 24', orders: 3, amount: 5610 },
];

export const repPerformanceData = [
  { rep_name: 'Suresh N.', rep_id: 4, routes: 'RT001/RT002', customers_total: 9, visited_today: 3, orders_today: 2, coverage_pct: 33, strike_rate: 67, day_started: true },
  { rep_name: 'Deepa M.', rep_id: 5, routes: 'RT003', customers_total: 3, visited_today: 2, orders_today: 1, coverage_pct: 67, strike_rate: 50, day_started: true },
  { rep_name: 'Karan S.', rep_id: 6, routes: 'RT004', customers_total: 3, visited_today: 0, orders_today: 0, coverage_pct: 0, strike_rate: 0, day_started: false },
  { rep_name: 'Ravi K.', rep_id: 7, routes: 'RT005', customers_total: 4, visited_today: 4, orders_today: 3, coverage_pct: 100, strike_rate: 75, day_started: true },
  { rep_name: 'Sneha P.', rep_id: 8, routes: 'RT006', customers_total: 4, visited_today: 1, orders_today: 1, coverage_pct: 25, strike_rate: 100, day_started: true },
];
