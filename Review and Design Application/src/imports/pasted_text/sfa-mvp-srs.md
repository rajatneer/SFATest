Software Requirements Specification (SRS) + Dev Execution Pack
FMCG Sales Force Automation (SFA) – MVP (Standalone, PWA)
________________________________________
1. Introduction
1.1 Purpose
This document defines the detailed software requirements and implementation blueprint for a mobile-first FMCG Sales Force Automation (SFA) system to be delivered as a Progressive Web App (PWA).
The system will be used by Sales Representatives, Territory Sales Incharges (TSIs), Admin users, and Distributor users for route-based execution, visit capture, and order capture.
1.2 Scope
•	Standalone system in MVP
•	No CRM / ERP integrations in MVP
•	Internal ownership of master data and transactional data
•	Route-based execution model
•	PWA-based field usage
•	Offline-first visit and order capture
________________________________________
2. Business Context
2.1 Sales Hierarchy
•	Territory Sales Incharge (TSI)
•	Sales Representatives under TSI
2.2 Customer Coverage Hierarchy
•	Territory → Route → Customer
2.3 Mapping Rules
•	Each Territory has one or more Routes
•	Each Route has one or more Customers
•	Each Route is mapped to exactly one Distributor
•	Each Route is assigned to exactly one active Sales Representative at a time
•	One Sales Representative may be assigned to multiple Routes
•	Each Customer belongs to exactly one Route
2.4 Order Flow
1.	Sales Rep starts day
2.	Sales Rep selects one assigned route
3.	System shows customers of selected route
4.	Sales Rep visits any customer in any order
5.	Sales Rep records visit and optionally creates order
6.	Order is routed to route-mapped distributor
7.	Distributor updates fulfillment status
8.	Sales Rep ends day
________________________________________
3. User Roles
3.1 Roles
•	Admin
•	TSI
•	Sales Rep
•	Distributor User
3.2 Role Responsibilities
Role	Responsibilities
Admin	Master setup, user management, uploads, configuration
TSI	View rep performance, route coverage, order summaries
Sales Rep	Start day, select route, visit customers, capture orders, end day
Distributor User	View routed orders, update order fulfillment status
3.3 Role Master Requirement
The system shall maintain a Role Master instead of relying on free-text role values in the user record.
Role Master Fields
•	role_id
•	role_code
•	role_name
•	role_description
•	is_active
The User table shall reference role_id as a foreign key.
Distributor users shall be treated as standard application users with a role of Distributor User.
________________________________________
4. Actors / User Journeys
4.1 Sales Rep Daily Flow
1.	Login using username and password
2.	Tap Start Day
3.	Select one assigned route
4.	View customers mapped to route
5.	Open customer
6.	Check in to customer
7.	If customer coordinates are missing, capture customer latitude and longitude during visit
8.	Record visit notes / activity
9.	Create order if applicable
10.	Repeat for other customers in route
11.	Tap End Day
4.2 TSI Flow
1.	Login via web/PWA
2.	View route coverage by rep
3.	View strike rate and orders
4.	Monitor missed customers and pending orders
4.3 Distributor Flow
1.	Login via username and password
2.	View orders routed to distributor
3.	Update order status
________________________________________
5. Functional Requirements
5.1 Authentication and Session Management
Requirements
•	System shall support login using username and password
•	System shall support secure logout
•	System shall maintain authenticated user session as per configurable timeout
•	Passwords shall be stored only in hashed form
•	Admin shall be able to reset passwords
Acceptance Criteria
•	User can log in with valid credentials
•	User cannot log in with invalid credentials
•	Password reset flow is available to Admin
________________________________________
5.2 Start Day / End Day
Requirements
•	System shall provide an explicit Start Day action button
•	System shall provide an explicit End Day action button
•	Sales Rep shall not create visits or orders before Start Day
•	Sales Rep shall not create visits or orders after End Day
•	Start Day action shall capture timestamp and current geo coordinates
•	End Day action shall capture timestamp and current geo coordinates
•	System shall allow only one active day session per rep at a time
Data Captured
•	rep_id
•	business_date
•	start_day_timestamp
•	start_day_lat
•	start_day_long
•	end_day_timestamp
•	end_day_lat
•	end_day_long
•	device_id (optional)
•	sync_status
Acceptance Criteria
•	Rep can tap Start Day once per work session
•	Rep can tap End Day only after Start Day
•	System blocks visit/order creation without active day session
________________________________________
5.3 Route Assignment and Route Selection
Requirements
•	System shall allow Admin to assign multiple routes to one Sales Rep
•	System shall allow only one active rep assignment per route at a given time
•	System shall display all active assigned routes to the rep
•	Rep shall manually select one route before performing field activities
•	System shall associate all subsequent visits/orders with the selected route until route is changed
•	System may allow route switching during the day only to another assigned route
Acceptance Criteria
•	Rep sees only own assigned routes
•	Rep cannot access customers outside selected route
•	Route without active assignment cannot be selected
________________________________________
5.4 Customer Master Management
Requirements
•	System shall maintain Customer Master internally
•	Customer can be created via screen or bulk upload
•	Each customer must be mapped to exactly one route
•	Customer master shall support geo coordinates for tolerance-based check-in validation
Customer Master Fields
•	customer_id
•	customer_code
•	customer_name
•	contact_person
•	mobile_number
•	alternate_mobile_number
•	address_line_1
•	address_line_2
•	locality
•	city
•	state
•	pincode
•	route_id
•	territory_id (derived or stored)
•	distributor_id (derived or stored)
•	gst_number (optional)
•	channel / outlet_type
•	latitude
•	longitude
•	coordinate_capture_source (admin_upload, admin_manual, rep_capture)
•	coordinate_capture_timestamp
•	is_active
•	created_at
•	updated_at
Coordinate Capture Logic
•	If customer latitude/longitude is already available, system shall use it for geo tolerance check
•	If customer latitude/longitude is not available, rep shall be allowed to capture customer coordinates during visit/check-in flow
•	Once captured and synced, coordinates shall be stored in Customer Master
•	Subsequent visits shall use stored coordinates for geo tolerance validation
•	System should allow Admin to edit customer coordinates manually when required
Acceptance Criteria
•	New customer cannot be saved without route mapping
•	Missing coordinates can be captured during first valid visit
•	Subsequent check-ins use saved coordinates
________________________________________
5.5 Geo Validation and Check-In
Requirements
•	System shall allow rep to check in to customer during visit
•	System shall capture current device latitude and longitude at check-in
•	If customer master coordinates are available, system shall validate check-in against configurable tolerance radius
•	If customer master coordinates are unavailable, system shall allow check-in and prompt for coordinate capture
•	System shall store geo distance result where tolerance validation is performed
•	System shall support configurable tolerance radius at system level, with optional route/customer override in future
Check-In Rules
•	Rep must have active Start Day session
•	Rep must have selected a valid assigned route
•	Customer must belong to selected route
•	If coordinates exist: check-in passes only if within tolerance, unless override is allowed by policy
•	If coordinates do not exist: system allows coordinate capture and stores them for future
Captured Fields per Visit Check-In
•	visit_id
•	rep_id
•	route_id
•	customer_id
•	checkin_timestamp
•	checkin_lat
•	checkin_long
•	customer_ref_lat
•	customer_ref_long
•	geo_distance_meters
•	within_tolerance_flag
•	coordinate_captured_during_visit_flag
________________________________________
5.6 Visit Management
Requirements
•	System shall support customer visit creation after route selection
•	System shall support visit statuses:
o	Completed
o	Skipped
o	Not Visited (derived)
•	Rep shall be able to add notes
•	Rep shall be able to check out from visit
•	System shall capture checkout timestamp and coordinates
Business Rules
•	A visit belongs to one rep, one route, and one customer
•	Multiple visits to same customer on same day may be allowed only if business approves; MVP default is one completed visit per customer per rep per day
•	Not Visited shall be system-derived from customers in selected route with no completed/skipped action for the active day
________________________________________
5.7 Product and Pricing Master
Requirements
•	System shall maintain Product Master internally
•	System shall support product create/edit/upload
•	System shall maintain basic price list internally
•	System shall allow activation/deactivation of products
Product Master Fields
•	product_id
•	product_code
•	product_name
•	product_description
•	uom
•	mrp (optional)
•	selling_price
•	is_active
•	created_at
•	updated_at
Price List Fields
•	price_list_id
•	product_id
•	effective_from
•	effective_to
•	price
•	is_active
________________________________________
5.8 Order Management
Requirements
•	System shall allow order creation only within active day session
•	System shall allow order creation only for customers belonging to selected route
•	System shall allow order creation during or immediately after customer visit
•	System shall support one or more order lines
•	System shall auto-calculate line totals and header total
•	System shall save order offline when network is unavailable
•	System shall sync order when network is available
Order Header Fields
•	order_id
•	order_number
•	order_date
•	rep_id
•	route_id
•	customer_id
•	distributor_id
•	visit_id (nullable but recommended)
•	status
•	gross_amount
•	net_amount
•	source (pwa)
•	created_at
•	updated_at
•	sync_status
•	client_generated_uuid
Order Item Fields
•	order_item_id
•	order_id
•	product_id
•	quantity
•	unit_price
•	line_total
Order Statuses
•	Created
•	Synced
•	Accepted
•	Dispatched
•	Delivered
•	Cancelled (optional with admin control)
Routing Logic
•	System shall derive distributor from route
•	User shall not manually select distributor during order creation in MVP
Acceptance Criteria
•	Order cannot be created for customer outside selected route
•	Order automatically carries route distributor
•	Offline order sync does not create duplicate records
________________________________________
5.9 Distributor Module
Requirements
•	Distributor user shall view only orders mapped to distributor
•	Distributor user shall update order status
•	Distributor user shall not edit order lines in MVP
Distributor Status Update Fields
•	order_id
•	distributor_user_id
•	new_status
•	status_timestamp
•	remarks (optional)
________________________________________
5.10 Attendance / Day Session Management
The system shall treat Start Day / End Day as the formal attendance mechanism for field reps.
Requirements
•	Separate attendance login/logout event model is not required in MVP
•	Day session shall be the source for presence and activity reporting
•	TSI/Admin dashboards shall use day session data for attendance reports
________________________________________
5.11 Reporting and Dashboards
Rep Reports
•	Today’s route customers
•	Visited customers
•	Orders created
•	Pending sync count
TSI Reports
•	Rep-wise route coverage %
•	Route-wise coverage %
•	Strike rate = orders / completed visits
•	Orders per rep per day
•	Missed customers
•	Pending day end count
Admin Reports
•	Master data completeness
•	Customers without coordinates
•	Upload validation errors
•	Distributor-wise order status summary
________________________________________
5.12 Master Uploads
Upload-Supported Masters
•	Territories
•	Routes
•	Distributors
•	Customers
•	Products
•	Users
•	Route Assignments
•	Price Lists
Requirements
•	System shall provide downloadable templates
•	System shall validate uploaded files before commit
•	System shall generate error file with row-wise issue details
•	System shall support preview before final save
________________________________________
6. Non-Functional Requirements
6.1 Application Type
•	Frontend shall be delivered as a Progressive Web App (PWA)
•	PWA shall support installable behavior on supported mobile devices
•	PWA shall support offline storage and deferred sync
6.2 Performance
•	Page load for key screens under normal network should be under 3 seconds
•	Key actions such as route selection, customer open, order save should complete within 2 seconds excluding network delays
6.3 Offline Capability
•	Route/customer list, visits, Start Day, End Day, and order capture shall work offline after prior sync/login
•	Login may require online validation unless remember-session mode is enabled
•	Unsynced transactions shall persist across browser refresh/app restart wherever supported by device/browser storage
6.4 Security
•	Username/password authentication
•	Password hashing using strong industry-standard algorithm
•	Role-based authorization
•	HTTPS mandatory
•	Audit trail for critical changes
6.5 Scalability
•	Support 5,000 to 10,000 field users
•	Support concurrent use across territories and distributors
6.6 Reliability
•	Local queue-based sync
•	Retry on failed sync
•	Idempotent APIs for transaction creation
________________________________________
7. Data Model (Logical)
7.1 Master Tables
RoleMaster
•	role_id (PK)
•	role_code (UNIQUE)
•	role_name
•	role_description
•	is_active
•	created_at
•	updated_at
User
•	user_id (PK)
•	username (UNIQUE)
•	password_hash
•	full_name
•	mobile_number
•	email
•	role_id (FK -> RoleMaster)
•	manager_user_id (FK -> User, nullable for TSI/Admin)
•	distributor_id (FK nullable)
•	is_active
•	created_at
•	updated_at
Territory
•	territory_id (PK)
•	territory_code (UNIQUE)
•	territory_name
•	tsi_user_id (FK -> User)
•	is_active
Distributor
•	distributor_id (PK)
•	distributor_code (UNIQUE)
•	distributor_name
•	contact_person
•	mobile_number
•	address
•	is_active
Route
•	route_id (PK)
•	route_code (UNIQUE)
•	route_name
•	territory_id (FK)
•	distributor_id (FK)
•	is_active
RouteAssignment
•	route_assignment_id (PK)
•	route_id (FK)
•	rep_user_id (FK -> User)
•	start_date
•	end_date (nullable)
•	is_active
Customer
•	customer_id (PK)
•	customer_code (UNIQUE)
•	customer_name
•	route_id (FK)
•	distributor_id (FK nullable denormalized)
•	outlet_type
•	contact_person
•	mobile_number
•	address_line_1
•	address_line_2
•	locality
•	city
•	state
•	pincode
•	gst_number
•	latitude DECIMAL(10,7) nullable
•	longitude DECIMAL(10,7) nullable
•	coordinate_capture_source
•	coordinate_capture_timestamp
•	is_active
•	created_at
•	updated_at
Product
•	product_id (PK)
•	product_code (UNIQUE)
•	product_name
•	uom
•	selling_price DECIMAL(12,2)
•	is_active
•	created_at
•	updated_at
PriceList
•	price_list_id (PK)
•	product_id (FK)
•	effective_from
•	effective_to nullable
•	price DECIMAL(12,2)
•	is_active
7.2 Transaction Tables
DaySession
•	day_session_id (PK)
•	rep_user_id (FK)
•	business_date
•	start_day_timestamp
•	start_day_lat DECIMAL(10,7)
•	start_day_long DECIMAL(10,7)
•	end_day_timestamp nullable
•	end_day_lat DECIMAL(10,7) nullable
•	end_day_long DECIMAL(10,7) nullable
•	selected_route_id (FK nullable)
•	status (started, ended)
•	client_generated_uuid
•	sync_status
•	created_at
•	updated_at
Visit
•	visit_id (PK)
•	rep_user_id (FK)
•	day_session_id (FK)
•	route_id (FK)
•	customer_id (FK)
•	checkin_timestamp
•	checkin_lat DECIMAL(10,7)
•	checkin_long DECIMAL(10,7)
•	customer_ref_lat DECIMAL(10,7) nullable
•	customer_ref_long DECIMAL(10,7) nullable
•	geo_distance_meters DECIMAL(10,2) nullable
•	within_tolerance_flag BOOLEAN nullable
•	coordinate_captured_during_visit_flag BOOLEAN
•	visit_notes TEXT nullable
•	checkout_timestamp nullable
•	checkout_lat DECIMAL(10,7) nullable
•	checkout_long DECIMAL(10,7) nullable
•	visit_status (completed, skipped)
•	created_at
•	updated_at
•	client_generated_uuid
•	sync_status
OrderHeader
•	order_id (PK)
•	order_number (UNIQUE)
•	order_date
•	rep_user_id (FK)
•	day_session_id (FK)
•	route_id (FK)
•	customer_id (FK)
•	distributor_id (FK)
•	visit_id (FK nullable)
•	status
•	gross_amount DECIMAL(12,2)
•	net_amount DECIMAL(12,2)
•	created_at
•	updated_at
•	client_generated_uuid
•	sync_status
OrderItem
•	order_item_id (PK)
•	order_id (FK)
•	product_id (FK)
•	quantity DECIMAL(12,3)
•	unit_price DECIMAL(12,2)
•	line_total DECIMAL(12,2)
OrderStatusHistory
•	order_status_history_id (PK)
•	order_id (FK)
•	old_status
•	new_status
•	changed_by_user_id (FK)
•	changed_at
•	remarks
UploadJob
•	upload_job_id (PK)
•	upload_type
•	file_name
•	uploaded_by_user_id
•	uploaded_at
•	status
•	error_file_path nullable
________________________________________
8. Business Rules
1.	User must log in with username and password
2.	Sales Rep must start day before any field activity
3.	Sales Rep may end day only after starting day
4.	Sales Rep can select only assigned routes
5.	Customer must belong to selected route
6.	One route can have only one active rep assignment at a time
7.	One rep may have multiple route assignments
8.	One route maps to one distributor
9.	Order distributor is system-derived from route
10.	Customer coordinates, when available, must be used for geo tolerance check
11.	If customer coordinates are unavailable, system may capture them during visit and persist them to master
12.	Orders and visits created offline must sync idempotently
13.	Start Day / End Day forms attendance basis
________________________________________
9. Mobile / PWA Screens
9.1 Sales Rep PWA Screens
1.	Login
2.	Home / Dashboard
3.	Start Day
4.	Route Selection
5.	Customer List
6.	Customer Detail
7.	Check-In / Coordinate Capture
8.	Visit Notes / Checkout
9.	Order Creation
10.	Order Summary
11.	Pending Sync Queue
12.	End Day
9.2 TSI Screens
1.	Login
2.	Dashboard
3.	Rep Performance
4.	Route Coverage
5.	Orders Summary
6.	Customer Without Coordinates Report
9.3 Admin Screens
1.	Login
2.	Master Management
3.	User Management
4.	Role Management
5.	Upload Jobs
6.	Reports
9.4 Distributor Screens
1.	Login
2.	Assigned Orders
3.	Order Detail
4.	Status Update
________________________________________
10. Offline Sync Design
10.1 Client Storage
PWA shall store the following locally:
•	user session metadata
•	assigned routes
•	customer list for assigned routes
•	products and price list snapshot
•	unsynced Start Day / End Day events
•	unsynced visits
•	unsynced orders
Recommended local storage approach:
•	IndexedDB for transactional/offline data
•	Service Worker for asset caching
10.2 Sync Queue
Each locally created transaction shall enter a sync queue with:
•	queue_id
•	entity_type
•	entity_client_uuid
•	payload_json
•	created_at
•	retry_count
•	last_retry_at
•	sync_status
•	last_error_message
10.3 Sync Principles
•	Client generates UUID for offline-created transaction
•	Server APIs must support idempotency using client UUID
•	Sync order priority:
1.	Start Day
2.	Customer coordinate update (if any)
3.	Visit
4.	Order
5.	End Day
10.4 Conflict Rules
•	Duplicate transaction request with same client UUID must return existing server record
•	Customer coordinate update should not blindly overwrite admin-updated values without rule check
•	Recommended rule for MVP: first successful coordinate capture persists unless later manually updated by Admin
________________________________________
11. API Design Pack (REST)
11.1 Authentication APIs
POST /api/auth/login
Request:
{
  "username": "rep001",
  "password": "secret"
}
Response:
{
  "token": "jwt-or-session-token",
  "user": {
    "user_id": 101,
    "full_name": "Rep A",
    "role_code": "SALES_REP"
  }
}
POST /api/auth/logout
________________________________________
11.2 Master Sync APIs
GET /api/me/routes
Returns routes assigned to logged-in rep.
GET /api/routes/{routeId}/customers
Returns customers under route.
GET /api/products
Returns active products and price snapshot.
________________________________________
11.3 Day Session APIs
POST /api/day-sessions/start
{
  "client_generated_uuid": "uuid-1",
  "business_date": "2026-04-18",
  "start_day_timestamp": "2026-04-18T09:02:11Z",
  "start_day_lat": 19.1234567,
  "start_day_long": 72.1234567
}
POST /api/day-sessions/{id}/select-route
{
  "route_id": 501
}
POST /api/day-sessions/end
{
  "client_generated_uuid": "uuid-2",
  "day_session_id": 7001,
  "end_day_timestamp": "2026-04-18T18:15:22Z",
  "end_day_lat": 19.2222222,
  "end_day_long": 72.3333333
}
________________________________________
11.4 Visit APIs
POST /api/visits/checkin
{
  "client_generated_uuid": "uuid-visit-1",
  "day_session_id": 7001,
  "route_id": 501,
  "customer_id": 9001,
  "checkin_timestamp": "2026-04-18T10:05:00Z",
  "checkin_lat": 19.100001,
  "checkin_long": 72.100001
}
Response example:
{
  "visit_id": 8801,
  "within_tolerance_flag": true,
  "geo_distance_meters": 21.55,
  "customer_coordinates_missing": false
}
POST /api/customers/{customerId}/coordinates
{
  "latitude": 19.100001,
  "longitude": 72.100001,
  "capture_source": "rep_capture",
  "capture_timestamp": "2026-04-18T10:05:10Z"
}
POST /api/visits/{visitId}/checkout
{
  "checkout_timestamp": "2026-04-18T10:18:00Z",
  "checkout_lat": 19.100002,
  "checkout_long": 72.100002,
  "visit_notes": "Discussed secondary display",
  "visit_status": "completed"
}
________________________________________
11.5 Order APIs
POST /api/orders
{
  "client_generated_uuid": "uuid-order-1",
  "day_session_id": 7001,
  "route_id": 501,
  "customer_id": 9001,
  "visit_id": 8801,
  "items": [
    {
      "product_id": 1001,
      "quantity": 10,
      "unit_price": 120.0
    }
  ]
}
Response:
{
  "order_id": 9901,
  "order_number": "ORD-20260418-9901",
  "distributor_id": 301,
  "status": "Created"
}
________________________________________
11.6 Distributor APIs
GET /api/distributor/orders
POST /api/distributor/orders/{orderId}/status
{
  "new_status": "Dispatched",
  "remarks": "Out for delivery"
}
________________________________________
11.7 Upload APIs
POST /api/uploads/customers
POST /api/uploads/routes
POST /api/uploads/products
POST /api/uploads/users
________________________________________
12. Screen-Level UX Notes
12.1 Login
•	Username field
•	Password field
•	Forgot password / contact admin message
•	Remember me optional
12.2 Home
•	Start Day button if no active day session
•	Active route card if route selected
•	Pending sync counter
•	Today summary
12.3 Route Selection
•	List assigned routes
•	Search by route name/code
•	Route card shows territory, distributor, customer count
12.4 Customer List
•	Search by customer name/code
•	Badges: visited, skipped, order placed, coordinates missing
12.5 Check-In Flow
•	On tap Check-In, app fetches current GPS
•	If customer coordinates exist: show pass/fail tolerance result
•	If coordinates missing: allow capture and continue
12.6 Order Screen
•	Product search
•	Quantity entry
•	Auto total
•	Save draft / submit
12.7 Pending Sync
•	List unsynced actions
•	Retry button
•	Error reason
________________________________________
13. Suggested Technology Stack
13.1 Frontend
•	PWA using React / Next.js or React + Vite
•	Service Worker
•	IndexedDB wrapper (Dexie or equivalent)
13.2 Backend
•	Node.js / NestJS or Express
•	REST APIs
•	JWT/session auth
13.3 Database
•	PostgreSQL recommended
13.4 Infra
•	Object storage for upload files and reports
•	Redis optional for session/cache
________________________________________
14. SQL Starter Schema (Illustrative)
CREATE TABLE role_master (
  role_id BIGSERIAL PRIMARY KEY,
  role_code VARCHAR(50) UNIQUE NOT NULL,
  role_name VARCHAR(100) NOT NULL,
  role_description TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE distributor (
  distributor_id BIGSERIAL PRIMARY KEY,
  distributor_code VARCHAR(50) UNIQUE NOT NULL,
  distributor_name VARCHAR(150) NOT NULL,
  contact_person VARCHAR(150),
  mobile_number VARCHAR(20),
  address TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE app_user (
  user_id BIGSERIAL PRIMARY KEY,
  username VARCHAR(100) UNIQUE NOT NULL,
  password_hash TEXT NOT NULL,
  full_name VARCHAR(150) NOT NULL,
  mobile_number VARCHAR(20),
  email VARCHAR(150),
  role_id BIGINT NOT NULL REFERENCES role_master(role_id),
  manager_user_id BIGINT NULL REFERENCES app_user(user_id),
  distributor_id BIGINT NULL REFERENCES distributor(distributor_id),
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE territory (
  territory_id BIGSERIAL PRIMARY KEY,
  territory_code VARCHAR(50) UNIQUE NOT NULL,
  territory_name VARCHAR(150) NOT NULL,
  tsi_user_id BIGINT NOT NULL REFERENCES app_user(user_id),
  is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE route (
  route_id BIGSERIAL PRIMARY KEY,
  route_code VARCHAR(50) UNIQUE NOT NULL,
  route_name VARCHAR(150) NOT NULL,
  territory_id BIGINT NOT NULL REFERENCES territory(territory_id),
  distributor_id BIGINT NOT NULL REFERENCES distributor(distributor_id),
  is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE route_assignment (
  route_assignment_id BIGSERIAL PRIMARY KEY,
  route_id BIGINT NOT NULL REFERENCES route(route_id),
  rep_user_id BIGINT NOT NULL REFERENCES app_user(user_id),
  start_date DATE NOT NULL,
  end_date DATE,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  CONSTRAINT uq_active_route_assignment UNIQUE (route_id, is_active)
);

CREATE TABLE customer (
  customer_id BIGSERIAL PRIMARY KEY,
  customer_code VARCHAR(50) UNIQUE NOT NULL,
  customer_name VARCHAR(200) NOT NULL,
  route_id BIGINT NOT NULL REFERENCES route(route_id),
  distributor_id BIGINT REFERENCES distributor(distributor_id),
  outlet_type VARCHAR(50),
  contact_person VARCHAR(150),
  mobile_number VARCHAR(20),
  address_line_1 VARCHAR(200),
  address_line_2 VARCHAR(200),
  locality VARCHAR(100),
  city VARCHAR(100),
  state VARCHAR(100),
  pincode VARCHAR(20),
  gst_number VARCHAR(30),
  latitude DECIMAL(10,7),
  longitude DECIMAL(10,7),
  coordinate_capture_source VARCHAR(30),
  coordinate_capture_timestamp TIMESTAMP,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);
________________________________________
15. Acceptance Criteria Pack
15.1 Start Day
•	Given rep is logged in
•	When rep taps Start Day
•	Then system creates active day session with timestamp and geo coordinates
15.2 Route Selection
•	Given rep has active route assignments
•	When rep opens route selection
•	Then system shows only assigned routes
15.3 Coordinate Capture
•	Given customer coordinates are missing
•	When rep checks in successfully
•	Then system prompts capture/store of customer coordinates
•	And subsequent visits use stored coordinates for tolerance check
15.4 Order Routing
•	Given route is mapped to distributor
•	When rep creates order for customer in route
•	Then order is auto-assigned to mapped distributor
15.5 End Day
•	Given rep has active day session
•	When rep taps End Day
•	Then system captures end timestamp and geo coordinates and closes day session
________________________________________
16. Engineering Backlog Suggestions
Phase A
•	Auth + role master
•	Master setup screens + uploads
•	Route assignment
Phase B
•	Rep PWA: Start Day, route selection, customer list, visit flow
•	Coordinate capture logic
Phase C
•	Order capture + distributor routing
•	Distributor portal
Phase D
•	Reports + dashboards
•	Sync hardening + audit logs
________________________________________
17. Final Notes
This document is intended to serve as both: 1. the updated SRS, and 2. the initial Dev Execution Pack for engineering and AI-assisted development.
For production implementation, the next recommended artifacts are:
•	detailed API Swagger / OpenAPI spec
•	complete SQL migration scripts
•	screen wireframes
•	test case pack
