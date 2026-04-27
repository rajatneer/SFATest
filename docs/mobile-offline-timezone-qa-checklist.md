# Mobile Offline + Timezone QA Checklist

## Scope
Validate offline queue, reconnect sync, and timezone capture for:
- Start Day
- End Day
- Check-In
- Visit Checkout
- Order Creation (multi-SKU merge + 2-decimal quantity display)

## Preconditions
- App is running at http://localhost:5076
- Sales rep user is logged in on mobile pages
- Browser devtools or automation can force offline/online network state
- Route and customers are available

## General Pass Criteria
- Offline submission is saved in IndexedDB queue
- Queue payload includes timezone fields:
  - TimeZoneId or startTimeZoneId/endTimeZoneId
  - UtcOffsetMinutes or startUtcOffsetMinutes/endUtcOffsetMinutes
- On reconnect, queue syncs and local queue becomes 0 pending
- Server record reflects expected values after sync

## Test 1: End Day Offline
1. Open Dashboard with active day session.
2. Force network offline.
3. Submit End Day.
4. Verify offline message appears.
5. Inspect latest IndexedDB queue payload.
Expected:
- Entity type is EndDay.
- Payload includes endTimeZoneId and endUtcOffsetMinutes.

6. Restore network.
7. Open Pending Sync Queue.
Expected:
- Device queue shows 0 pending.

8. Open Dashboard.
Expected:
- Agent check-out time is shown.
- Time includes timezone context (timezone id or UTC offset).

## Test 2: Start Day Offline
1. Ensure day is ended (Start Day button visible).
2. Force network offline.
3. Submit Start Day.
4. Verify offline message appears.
5. Inspect latest IndexedDB queue payload.
Expected:
- Entity type is StartDay.
- Payload includes startTimeZoneId and startUtcOffsetMinutes.

6. Restore network.
7. Open Pending Sync Queue.
Expected:
- Device queue shows 0 pending.

8. Open Dashboard.
Expected:
- Agent check-in time is shown with timezone context.

## Test 3: Check-In Offline
1. Select route if not selected.
2. Open Check-In page for a customer.
3. Force network offline.
4. Submit Check-In.
5. Inspect latest IndexedDB queue payload.
Expected:
- Entity type is CheckIn.
- Payload includes TimeZoneId and UtcOffsetMinutes.

6. Restore network.
7. Open Pending Sync Queue.
Expected:
- Device queue shows 0 pending.

8. Verify server-side visit write.
Expected:
- Visit includes CheckinTimeZoneId and CheckinUtcOffsetMinutes.

## Test 4: Visit Checkout Offline
1. Open Visit Checkout page for an existing visit.
2. Force network offline.
3. Submit checkout.
4. Inspect latest IndexedDB queue payload.
Expected:
- Entity type is VisitCheckout.
- Payload includes TimeZoneId and UtcOffsetMinutes.

5. Restore network.
6. Open Pending Sync Queue.
Expected:
- Device queue shows 0 pending.

7. Verify server-side visit update.
Expected:
- Visit includes CheckoutTimeZoneId and CheckoutUtcOffsetMinutes.

## Test 5: Order Creation Offline (Multi-SKU)
1. Open Order Creation page.
2. Add at least 2 lines with same SKU and different quantities.
3. Force network offline.
4. Submit order.
5. Inspect latest IndexedDB queue payload.
Expected:
- Entity type is OrderCreation.
- Payload includes TimeZoneId and UtcOffsetMinutes.
- Payload includes multiple OrderLines entries.

6. Restore network.
7. Open Pending Sync Queue.
Expected:
- Device queue shows 0 pending.

8. Open Order Summary for synced order.
Expected:
- Duplicate SKU lines are merged server-side.
- Quantity is displayed with 2 decimals (example 3.50).

## Evidence To Capture
- Screenshot of offline saved message per flow
- Screenshot of Pending Sync Queue showing 0 pending
- IndexedDB payload screenshot showing timezone fields
- Order Summary screenshot showing merged line and 2-decimal quantity
- Server log snippet showing timezone columns in insert/update SQL

## Current Baseline (Validated)
- End Day offline payload captured timezone and synced
- Start Day offline payload captured timezone and synced
- Check-In offline payload captured timezone and synced
- Visit Checkout offline payload captured timezone and synced
- Order offline payload captured timezone, merged duplicate SKU, and showed 2-decimal quantity
