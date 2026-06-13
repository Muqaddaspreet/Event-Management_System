# Authorization Matrix

Roles: **Anonymous** (no token) · **Attendee** · **Organizer** · **Admin**

Legend: ✅ Allowed · ❌ Denied · ⚠️ Conditional (see notes)

---

## Auth Endpoints

| Endpoint | Anonymous | Attendee | Organizer | Admin |
|---|---|---|---|---|
| `POST /api/auth/register` | ✅ | ✅ | ✅ | ✅ |
| `POST /api/auth/login` | ✅ | ✅ | ✅ | ✅ |

---

## Event Endpoints

| Endpoint | Anonymous | Attendee | Organizer | Admin | Notes |
|---|---|---|---|---|---|
| `GET /api/events` | ✅ | ✅ | ✅ | ✅ | Returns `Published` only for all callers |
| `GET /api/events/{id}` | ⚠️ | ⚠️ | ⚠️ | ✅ | Anonymous/Attendee: `Published` only. Organizer: own events any status. Admin: any event any status. Returns `404` if not accessible. |
| `GET /api/events/mine` | ❌ | ❌ | ✅ | ❌ | Organizer only, own events across all statuses |
| `POST /api/events` | ❌ | ❌ | ✅ | ❌ | New event defaults to `PendingApproval` |
| `PUT /api/events/{id}` | ❌ | ❌ | ⚠️ | ✅ | Organizer: own events only. Admin: any event. |
| `DELETE /api/events/{id}` | ❌ | ❌ | ⚠️ | ✅ | Sets `Status = Cancelled`, no hard delete. Organizer: own events only. |

---

## Admin Endpoints

| Endpoint | Anonymous | Attendee | Organizer | Admin | Notes |
|---|---|---|---|---|---|
| `GET /api/admin/dashboard` | ❌ | ❌ | ❌ | ✅ | Aggregate counts: users, events by status, registrations |
| `GET /api/admin/events` | ❌ | ❌ | ❌ | ✅ | All events, all statuses, all organizers |
| `POST /api/admin/events/{id}/approve` | ❌ | ❌ | ❌ | ✅ | Only valid when `Status = PendingApproval` |
| `POST /api/admin/events/{id}/reject` | ❌ | ❌ | ❌ | ✅ | Only valid when `Status = PendingApproval` |

---

## Registration Endpoints

| Endpoint | Anonymous | Attendee | Organizer | Admin | Notes |
|---|---|---|---|---|---|
| `GET /api/registrations/mine` | ❌ | ✅ | ❌ | ❌ | Own registrations only |
| `POST /api/registrations` | ❌ | ✅ | ❌ | ❌ | Event must be `Published`; reactivates cancelled registration |
| `DELETE /api/registrations/{id}` | ❌ | ✅ | ❌ | ❌ | Own registrations only; sets `Status = Cancelled` |

---

## Category Endpoints

| Endpoint | Anonymous | Attendee | Organizer | Admin |
|---|---|---|---|---|
| `GET /api/categories` | ✅ | ✅ | ✅ | ✅ |
| `POST /api/categories` | ❌ | ❌ | ❌ | ✅ |
| `PUT /api/categories/{id}` | ❌ | ❌ | ❌ | ✅ |
| `DELETE /api/categories/{id}` | ❌ | ❌ | ❌ | ✅ |

---

## Venue Endpoints

| Endpoint | Anonymous | Attendee | Organizer | Admin |
|---|---|---|---|---|
| `GET /api/venues` | ✅ | ✅ | ✅ | ✅ |
| `POST /api/venues` | ❌ | ❌ | ❌ | ✅ |
| `PUT /api/venues/{id}` | ❌ | ❌ | ❌ | ✅ |
| `DELETE /api/venues/{id}` | ❌ | ❌ | ❌ | ✅ |

---

## Admin User & Registration Endpoints

| Endpoint | Anonymous | Attendee | Organizer | Admin |
|---|---|---|---|---|
| `GET /api/admin/users` | ❌ | ❌ | ❌ | ✅ |
| `GET /api/admin/users/{id}` | ❌ | ❌ | ❌ | ✅ |
| `GET /api/admin/registrations` | ❌ | ❌ | ❌ | ✅ |

---

## Key Rules Summary

1. **Public registration** accepts only `Organizer` or `Attendee` as the role — Admin cannot be self-registered.
2. **Published events** are the only events visible to anonymous users and attendees.
3. **Organizers** cannot see other organizers' events; `/api/events/mine` is scoped to the token's `userId`.
4. **Admins** have unrestricted read and write access to all events regardless of status or owner.
5. **No endpoint hard-deletes** an event or registration; `DELETE` maps to a status change.
6. **`GET /api/events/{id}`** returns `404` — not `403` — when the resource exists but is not accessible to the caller, to avoid information leakage about non-published events.
