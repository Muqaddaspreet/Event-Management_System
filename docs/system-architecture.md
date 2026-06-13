# System Architecture

## Stack Overview

| Layer | Technology |
|---|---|
| Frontend | Angular (SPA) |
| Backend API | ASP.NET Core Web API (.NET 8) |
| Database | MySQL 8+ |
| ORM | Entity Framework Core |
| Authentication | JWT access tokens only (no refresh tokens) |

---

## Architecture Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                        Angular SPA                           │
│                                                              │
│  Feature modules: Auth, Events, Admin, Registrations         │
│  Guards: AuthGuard, RoleGuard                                │
│  Services: HTTP clients per feature                          │
│                                                              │
│  Attaches: Authorization: Bearer <jwt_access_token>          │
└─────────────────────────┬────────────────────────────────────┘
                          │ HTTPS / REST (JSON)
                          ▼
┌──────────────────────────────────────────────────────────────┐
│                   ASP.NET Core Web API                       │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐     │
│  │                  Controllers                        │     │
│  │  Route handling · HTTP responses · [Authorize]      │     │
│  └────────────────────────┬────────────────────────────┘     │
│                           │ calls                            │
│  ┌────────────────────────▼────────────────────────────┐     │
│  │                   Services                          │     │
│  │  Business logic · status rules · DTO mapping        │     │
│  └────────────────────────┬────────────────────────────┘     │
│                           │ calls                            │
│  ┌────────────────────────▼────────────────────────────┐     │
│  │                 Repositories                        │     │
│  │  EF Core queries · no business logic                │     │
│  └────────────────────────┬────────────────────────────┘     │
│                           │                                  │
│  ┌────────────────────────▼────────────────────────────┐     │
│  │               AppDbContext (EF Core)                │     │
│  └────────────────────────┬────────────────────────────┘     │
└───────────────────────────┼──────────────────────────────────┘
                            │ EF Core / Pomelo MySQL connector
                            ▼
┌──────────────────────────────────────────────────────────────┐
│                      MySQL Database                          │
│                                                              │
│  Tables: Users · Events · EventRegistrations                 │
│          Categories · Venues                                 │
└──────────────────────────────────────────────────────────────┘
```

---

## Layer Responsibilities

### Controllers
- Receive and parse HTTP requests.
- Validate input via data annotations and ModelState.
- Delegate all decisions to services.
- Return HTTP status codes (`200`, `201`, `400`, `401`, `403`, `404`, `409`).
- Apply `[Authorize]` / `[Authorize(Roles = "...")]` attributes.
- Never contain business logic.

### Services
- Enforce all business rules (status transitions, capacity checks, duplicate registration prevention).
- Map entities to response DTOs and request DTOs to entities.
- Orchestrate one or more repository calls.
- Never call `DbContext` directly — always go through repositories.

### Repositories
- Thin data-access wrappers over EF Core.
- Return entities or `null` — never DTOs.
- Own all LINQ queries, `.Include()` chains, and tracking/no-tracking decisions.

### DTOs
- `Request` DTOs: carry inbound data (POST/PUT bodies). Annotated for validation.
- `Response` DTOs: carry outbound data. Entities are never serialized directly.

### Entities
- Plain C# classes mapped to MySQL tables via EF Core.
- No business logic.

### Data
- `AppDbContext` — DbSet declarations, model configuration, seeding.
- EF Core migrations folder.

---

## Authentication Flow

1. Client sends `POST /api/auth/register` or `POST /api/auth/login`.
2. On success, server signs and returns a JWT access token containing `userId`, `email`, and `role` claims.
3. Client attaches the token as `Authorization: Bearer <token>` on every subsequent request.
4. ASP.NET Core JWT middleware validates the token on every request.
5. Role claims inside the token drive `[Authorize(Roles = "...")]` decisions.

No refresh tokens are used in V1.

---

## Role Model

| Role | How Created | Capabilities |
|---|---|---|
| `Admin` | Seeded manually in DB | Full system access |
| `Organizer` | Self-registered | Create and manage own events |
| `Attendee` | Self-registered | Browse published events, manage own registrations |
| Anonymous | No account needed | Browse published events only |

Public registration accepts only `Organizer` or `Attendee` as the requested role.

---

## Event Status Flow

```
[Organizer creates event]
         │
         ▼
   PendingApproval
    /           \
   ▼             ▼
Published      Rejected
   │
   ▼
Cancelled   (organizer or admin cancels)
```

- `Draft` is not a valid status in V1.
- Events and registrations are never hard-deleted.
- Cancelling an event sets `Status = Cancelled`.
