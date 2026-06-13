# Database Design

All primary keys are `INT AUTO_INCREMENT`. No hard deletes — soft cancellation via `Status` columns.

---

## Tables

### `Users`

| Column | Type | Constraints |
|---|---|---|
| `Id` | INT | PK, AUTO_INCREMENT |
| `FullName` | VARCHAR(100) | NOT NULL |
| `Email` | VARCHAR(150) | NOT NULL, UNIQUE |
| `PasswordHash` | VARCHAR(255) | NOT NULL |
| `Role` | ENUM('Admin','Organizer','Attendee') | NOT NULL |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP |

Notes:
- Admin rows are seeded manually; public registration can only create `Organizer` or `Attendee`.
- `Email` uniqueness is enforced at both DB and service layers.

---

### `Categories`

| Column | Type | Constraints |
|---|---|---|
| `Id` | INT | PK, AUTO_INCREMENT |
| `Name` | VARCHAR(100) | NOT NULL, UNIQUE |

Notes:
- A category can only be deleted if no events reference it. Attempting to delete a referenced category returns `409 Conflict`.

---

### `Venues`

| Column | Type | Constraints |
|---|---|---|
| `Id` | INT | PK, AUTO_INCREMENT |
| `Name` | VARCHAR(150) | NOT NULL |
| `Address` | VARCHAR(300) | NOT NULL |
| `City` | VARCHAR(100) | NOT NULL |
| `Capacity` | INT | NOT NULL, CHECK (Capacity > 0) |

Notes:
- A venue can only be deleted if no events reference it. Attempting to delete a referenced venue returns `409 Conflict`.

---

### `Events`

| Column | Type | Constraints |
|---|---|---|
| `Id` | INT | PK, AUTO_INCREMENT |
| `Title` | VARCHAR(200) | NOT NULL |
| `Description` | TEXT | NULL |
| `StartTime` | DATETIME | NOT NULL |
| `EndTime` | DATETIME | NOT NULL |
| `Capacity` | INT | NOT NULL, CHECK (Capacity > 0) |
| `Status` | ENUM('PendingApproval','Published','Rejected','Cancelled') | NOT NULL, DEFAULT 'PendingApproval' |
| `OrganizerId` | INT | NOT NULL, FK → Users(Id) |
| `CategoryId` | INT | NOT NULL, FK → Categories(Id) |
| `VenueId` | INT | NOT NULL, FK → Venues(Id) |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP |
| `UpdatedAt` | DATETIME | NOT NULL |

Notes:
- `Status` defaults to `PendingApproval` on insert.
- `Draft` is not a valid status value.
- `EndTime` must be after `StartTime` — enforced at service layer.
- `CategoryId` and `VenueId` are required; every event must belong to a category and a venue.
- No hard deletes; cancellation sets `Status = 'Cancelled'`.

---

### `EventRegistrations`

| Column | Type | Constraints |
|---|---|---|
| `Id` | INT | PK, AUTO_INCREMENT |
| `EventId` | INT | NOT NULL, FK → Events(Id) |
| `UserId` | INT | NOT NULL, FK → Users(Id) |
| `Status` | ENUM('Registered','Cancelled') | NOT NULL, DEFAULT 'Registered' |
| `RegisteredAt` | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP |
| `UpdatedAt` | DATETIME | NOT NULL |

Notes:
- Composite UNIQUE constraint on `(EventId, UserId)`.
- If a `Cancelled` record already exists for `(EventId, UserId)` and the attendee registers again, the service **reactivates** the existing row (`Status = 'Registered'`) instead of inserting a new one.
- No hard deletes; cancellation sets `Status = 'Cancelled'`.

---

## Relationships

```
Users (1) ──────────────────── (many) Events          [OrganizerId]
Users (1) ──────────────────── (many) EventRegistrations [UserId]
Events (1) ─────────────────── (many) EventRegistrations [EventId]
Categories (1) ─────────────── (many) Events          [CategoryId]
Venues (1) ──────────────────── (many) Events          [VenueId]
```

---

## Indexes

| Table | Index | Columns | Type |
|---|---|---|---|
| `Users` | `IX_Users_Email` | `Email` | UNIQUE |
| `Events` | `IX_Events_OrganizerId` | `OrganizerId` | Index |
| `Events` | `IX_Events_Status` | `Status` | Index |
| `Events` | `IX_Events_CategoryId` | `CategoryId` | Index |
| `Events` | `IX_Events_VenueId` | `VenueId` | Index |
| `EventRegistrations` | `UQ_EventRegistrations_EventId_UserId` | `(EventId, UserId)` | UNIQUE |
| `EventRegistrations` | `IX_EventRegistrations_UserId` | `UserId` | Index |

---

## Seed Data

Only `Admin` users are seeded manually. No other seed data is required for V1.

Example admin seed (values to be configured before first run):

| Field | Value |
|---|---|
| FullName | System Admin |
| Email | admin@example.com |
| PasswordHash | bcrypt hash of chosen password |
| Role | Admin |
