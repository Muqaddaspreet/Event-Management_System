# DTO Contract

All DTOs are plain data shapes — no business logic. Request DTOs include validation annotations. Entities are never returned directly.

---

## Auth DTOs

### `RegisterRequest`
```
FullName   : string   required, max 100
Email      : string   required, valid email format
Password   : string   required, min 8 characters
Role       : string   required, must be "Organizer" or "Attendee"
```

### `LoginRequest`
```
Email      : string   required
Password   : string   required
```

### `AuthResponse`
```
Token      : string   JWT access token
UserId     : int
FullName   : string
Email      : string
Role       : string
```

---

## Event DTOs

### `CreateEventRequest`
```
Title       : string   required, max 200
Description : string   optional
StartTime   : DateTime required
EndTime     : DateTime required (must be after StartTime — enforced in service)
Capacity    : int      required, must be > 0
CategoryId  : int      required
VenueId     : int      required
```

### `UpdateEventRequest`
```
Title       : string   required, max 200
Description : string   optional
StartTime   : DateTime required
EndTime     : DateTime required
Capacity    : int      required, must be > 0
CategoryId  : int      required
VenueId     : int      required
```

### `EventSummaryResponse`
```
Id           : int
Title        : string
StartTime    : DateTime
EndTime      : DateTime
Capacity     : int
Status       : string
OrganizerName: string
CategoryName : string
VenueName    : string
VenueCity    : string
```

### `EventDetailResponse`
```
Id               : int
Title            : string
Description      : string?
StartTime        : DateTime
EndTime          : DateTime
Capacity         : int
Status           : string
CreatedAt        : DateTime
UpdatedAt        : DateTime
OrganizerId      : int
OrganizerName    : string
CategoryId       : int
CategoryName     : string
VenueId          : int
VenueName        : string
VenueAddress     : string
VenueCity        : string
RegistrationCount: int
```

### `RejectEventRequest`
```
Reason : string   optional
```

---

## Registration DTOs

### `CreateRegistrationRequest`
```
EventId : int   required
```

### `RegistrationResponse`
```
Id             : int
EventId        : int
EventTitle     : string
EventStartTime : DateTime
UserId         : int
UserFullName   : string
Status         : string
RegisteredAt   : DateTime
UpdatedAt      : DateTime
```

---

## Category DTOs

### `CreateCategoryRequest`
```
Name : string   required, max 100
```

### `UpdateCategoryRequest`
```
Name : string   required, max 100
```

### `CategoryResponse`
```
Id   : int
Name : string
```

---

## Venue DTOs

### `CreateVenueRequest`
```
Name     : string   required, max 150
Address  : string   required, max 300
City     : string   required, max 100
Capacity : int      required, must be > 0
```

### `UpdateVenueRequest`
```
Name     : string   required, max 150
Address  : string   required, max 300
City     : string   required, max 100
Capacity : int      required, must be > 0
```

### `VenueResponse`
```
Id       : int
Name     : string
Address  : string
City     : string
Capacity : int
```

---

## User DTOs

### `UserResponse`
```
Id        : int
FullName  : string
Email     : string
Role      : string
CreatedAt : DateTime
```

---

## Admin DTOs

### `AdminDashboardResponse`
```
TotalUsers          : int
TotalEvents         : int
PendingEvents       : int
PublishedEvents     : int
TotalRegistrations  : int
```

---

## Pagination

### `PagedResult<T>`
Returned by `GET /api/events`, `GET /api/events/mine`, and `GET /api/admin/events`.

```
Items      : T[]   the current page of results
Page       : int   current page number (1-based)
PageSize   : int   number of items per page
TotalCount : int   total number of matching records across all pages
```

Callers derive total pages from `ceil(TotalCount / PageSize)`.

---

## Error Response

Standard error envelope returned on `4xx` responses:

```
Message : string
Errors  : string[]   optional, for validation failures
```

---

## Notes

- `Status` fields in responses use string literals matching the enum names: `PendingApproval`, `Published`, `Rejected`, `Cancelled`.
- `Draft` is not a valid status and must never appear in any DTO.
- Password fields are never included in any response DTO.
- `RegistrationCount` in `EventDetailResponse` reflects only `Registered` (non-cancelled) registrations.
