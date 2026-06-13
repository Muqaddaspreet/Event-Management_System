# API Contract

Base URL: `/api`

All request and response bodies are JSON. Authenticated endpoints require `Authorization: Bearer <token>`.

---

## Auth

### `POST /api/auth/register`
Register a new Organizer or Attendee account.

- **Auth:** None
- **Request body:** `RegisterRequest`
- **Success:** `201 Created` → `AuthResponse`
- **Errors:** `400` validation failure · `409` email already in use

---

### `POST /api/auth/login`
Authenticate and receive a JWT access token.

- **Auth:** None
- **Request body:** `LoginRequest`
- **Success:** `200 OK` → `AuthResponse`
- **Errors:** `400` validation failure · `401` invalid credentials

---

## Events — Public & Organizer

### `GET /api/events`
List all published events. Supports optional query filters and pagination.

- **Auth:** None (public)
- **Query params:** `categoryId` (int, optional) · `venueId` (int, optional) · `search` (string, optional) · `page` (int, default 1) · `pageSize` (int, default 10)
- **Success:** `200 OK` → `PagedResult<EventSummaryResponse>`

---

### `GET /api/events/{id}`
Get a single event by ID. Visibility rules:
- Anonymous and Attendee callers: only `Published` events are visible.
- Organizer callers: own events across all statuses are visible; other organizers' events are not.
- Admin: any event in any status is visible.
- If the event exists but is not visible to the caller, return `404` (not `403`) to avoid leaking the existence of non-published events.

- **Auth:** None required for published events; token required to view non-published events as owner or admin.
- **Success:** `200 OK` → `EventDetailResponse`
- **Errors:** `404` not found or not visible to caller

---

### `GET /api/events/mine`
List all events created by the authenticated organizer, across all statuses.

- **Auth:** `Organizer` role required
- **Query params:** `page` (int, default 1) · `pageSize` (int, default 10)
- **Success:** `200 OK` → `PagedResult<EventSummaryResponse>`
- **Errors:** `401` unauthenticated · `403` wrong role

---

### `POST /api/events`
Create a new event. Status defaults to `PendingApproval`.

- **Auth:** `Organizer` role required
- **Request body:** `CreateEventRequest`
- **Success:** `201 Created` → `EventDetailResponse`
- **Errors:** `400` validation failure · `401` · `403`

---

### `PUT /api/events/{id}`
Update an existing event. Only the owning Organizer or an Admin may update.

- **Auth:** `Organizer` (own event only) or `Admin`
- **Request body:** `UpdateEventRequest`
- **Success:** `200 OK` → `EventDetailResponse`
- **Errors:** `400` · `401` · `403` · `404`

---

### `DELETE /api/events/{id}`
Cancel an event (sets `Status = Cancelled`). No hard delete.

- **Auth:** `Organizer` (own event only) or `Admin`
- **Success:** `204 No Content`
- **Errors:** `401` · `403` · `404` · `409` event already cancelled

---

## Admin — Dashboard

### `GET /api/admin/dashboard`
Return aggregate counts for the admin overview.

- **Auth:** `Admin` role required
- **Success:** `200 OK` → `AdminDashboardResponse`
- **Errors:** `401` · `403`

---

## Admin — Event Management

### `GET /api/admin/events`
List all events across all statuses and all organizers.

- **Auth:** `Admin` role required
- **Query params:** `status` (string, optional) · `organizerId` (int, optional) · `page` (int, default 1) · `pageSize` (int, default 10)
- **Success:** `200 OK` → `PagedResult<EventSummaryResponse>`
- **Errors:** `401` · `403`

---

### `POST /api/admin/events/{id}/approve`
Approve a `PendingApproval` event, setting `Status = Published`.

- **Auth:** `Admin` role required
- **Request body:** None
- **Success:** `200 OK` → `EventDetailResponse`
- **Errors:** `401` · `403` · `404` · `409` event not in `PendingApproval` state

---

### `POST /api/admin/events/{id}/reject`
Reject a `PendingApproval` event, setting `Status = Rejected`.

- **Auth:** `Admin` role required
- **Request body:** `RejectEventRequest` (optional rejection reason)
- **Success:** `200 OK` → `EventDetailResponse`
- **Errors:** `401` · `403` · `404` · `409` event not in `PendingApproval` state

---

## Registrations

### `GET /api/registrations/mine`
List the authenticated attendee's own registrations.

- **Auth:** `Attendee` role required
- **Success:** `200 OK` → `RegistrationResponse[]`
- **Errors:** `401` · `403`

---

### `POST /api/registrations`
Register the authenticated attendee for a published event.
If a cancelled registration already exists for the same `(EventId, UserId)`, reactivate it.

- **Auth:** `Attendee` role required
- **Request body:** `CreateRegistrationRequest`
- **Success:** `201 Created` → `RegistrationResponse`
- **Errors:** `400` · `401` · `403` · `404` event not found · `409` already registered · `422` event at capacity or not published

---

### `DELETE /api/registrations/{id}`
Cancel own registration (sets `Status = Cancelled`). No hard delete.

- **Auth:** `Attendee` role required (own registration only)
- **Success:** `204 No Content`
- **Errors:** `401` · `403` · `404` · `409` already cancelled

---

## Categories (Admin-managed)

### `GET /api/categories`
List all categories.

- **Auth:** None (public)
- **Success:** `200 OK` → `CategoryResponse[]`

---

### `POST /api/categories`
Create a new category.

- **Auth:** `Admin` role required
- **Request body:** `CreateCategoryRequest`
- **Success:** `201 Created` → `CategoryResponse`
- **Errors:** `400` · `401` · `403` · `409` name already exists

---

### `PUT /api/categories/{id}`
Update a category name.

- **Auth:** `Admin` role required
- **Request body:** `UpdateCategoryRequest`
- **Success:** `200 OK` → `CategoryResponse`
- **Errors:** `400` · `401` · `403` · `404`

---

### `DELETE /api/categories/{id}`
Delete a category. Only succeeds if no events reference it.

- **Auth:** `Admin` role required
- **Success:** `204 No Content`
- **Errors:** `401` · `403` · `404` · `409` one or more events reference this category

---

## Venues (Admin-managed)

### `GET /api/venues`
List all venues.

- **Auth:** None (public)
- **Success:** `200 OK` → `VenueResponse[]`

---

### `POST /api/venues`
Create a new venue.

- **Auth:** `Admin` role required
- **Request body:** `CreateVenueRequest`
- **Success:** `201 Created` → `VenueResponse`
- **Errors:** `400` · `401` · `403`

---

### `PUT /api/venues/{id}`
Update a venue.

- **Auth:** `Admin` role required
- **Request body:** `UpdateVenueRequest`
- **Success:** `200 OK` → `VenueResponse`
- **Errors:** `400` · `401` · `403` · `404`

---

### `DELETE /api/venues/{id}`
Delete a venue. Only succeeds if no events reference it.

- **Auth:** `Admin` role required
- **Success:** `204 No Content`
- **Errors:** `401` · `403` · `404` · `409` one or more events reference this venue

---

## Admin — Users

### `GET /api/admin/users`
List all users.

- **Auth:** `Admin` role required
- **Success:** `200 OK` → `UserResponse[]`
- **Errors:** `401` · `403`

---

### `GET /api/admin/users/{id}`
Get a single user by ID.

- **Auth:** `Admin` role required
- **Success:** `200 OK` → `UserResponse`
- **Errors:** `401` · `403` · `404`

---

## Admin — Registrations

### `GET /api/admin/registrations`
List all registrations system-wide.

- **Auth:** `Admin` role required
- **Query params:** `eventId` (int, optional) · `userId` (int, optional)
- **Success:** `200 OK` → `RegistrationResponse[]`
- **Errors:** `401` · `403`

---

## Notes

- Endpoints return `404` when a resource does not exist or is not visible to the caller (no information leakage).
- No endpoint performs a hard delete on events or registrations.
- `GET /api/events/mine` requires the Organizer role; it is not accessible to Attendees or Admins via this route.
- `GET /api/admin/events` requires the Admin role and returns events of all statuses and all organizers.
- `PagedResult<T>` is returned by `GET /api/events`, `GET /api/events/mine`, and `GET /api/admin/events`. See dto-contract.md for the shape.
- Deleting a category or venue that is still referenced by one or more events returns `409 Conflict`; no soft-delete fields are added to those tables in V1.
