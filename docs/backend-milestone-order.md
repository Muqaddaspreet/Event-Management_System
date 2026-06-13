# Backend Milestone Order

Implementation order for the ASP.NET Core Web API. Each milestone builds on the previous.
Do not start a milestone until all earlier ones are complete and verified.

---

## Milestone 1 — Project Setup & Configuration

Goal: A runnable API skeleton with database connectivity confirmed.

- Create the ASP.NET Core Web API project (`EventManagement.API`).
- Create the test project (`EventManagement.Tests`).
- Add NuGet packages: `Pomelo.EntityFrameworkCore.MySql`, `Microsoft.EntityFrameworkCore.Design`, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`.
- Configure `appsettings.json` with `ConnectionStrings:DefaultConnection` and `Jwt` section (`Key`, `Issuer`, `Audience`, `ExpiresInMinutes`).
- Register services in `Program.cs`: EF Core, JWT authentication, CORS for Angular dev origin.
- Add `ExceptionHandlingMiddleware` skeleton.
- Verify the app starts without errors (`dotnet run`).

---

## Milestone 2 — Entities & Database Schema

Goal: All tables created in MySQL via EF Core migration.

- Create entity classes: `User`, `Event`, `EventRegistration`, `Category`, `Venue`.
- Create enum files: `UserRole`, `EventStatus`, `RegistrationStatus`.
- Create `AppDbContext` with `DbSet` declarations and model configuration:
  - `EventStatus` stored as string (value converter), default `PendingApproval`.
  - `(EventId, UserId)` unique index on `EventRegistrations`.
  - All FK relationships.
- Add and apply the initial EF Core migration.
- Verify all tables exist in MySQL with correct columns and constraints.

---

## Milestone 3 — Admin Seeding

Goal: At least one Admin user exists in the database.

- Create `AdminSeeder.cs` in `Data/Seed/`.
- Seed one admin user with a hashed password on app startup (only if no admin exists).
- Verify login with the seeded admin credentials returns a token with `Role = Admin`.

---

## Milestone 4 — Authentication (Register & Login)

Goal: Users can register and log in; JWT tokens are issued and validated.

- Create `IUserRepository` / `UserRepository` — methods: `GetByEmailAsync`, `CreateAsync`, `GetByIdAsync`.
- Create `IAuthService` / `AuthService` — register (hash password, enforce role restriction, check unique email), login (verify password, sign JWT with `userId`/`email`/`role` claims).
- Create `AuthController` — `POST /api/auth/register`, `POST /api/auth/login`.
- Validate: registration rejects `Admin` role; duplicate email returns `409`; invalid credentials return `401`.
- Write service-layer unit tests: `AuthServiceTests`.

---

## Milestone 5 — Categories & Venues (Reference Data)

Goal: Admin can manage categories and venues; all roles can read them.

- Create repository and service pairs for `Category` and `Venue`.
- Create `CategoriesController` and `VenuesController`.
- Implement CRUD endpoints per API contract.
- Write service-layer unit tests: `CategoryServiceTests`, `VenueServiceTests`.

---

## Milestone 6 — Event CRUD (Organizer & Admin)

Goal: Organizers can create and manage their own events; admins can manage all events.

- Create `IEventRepository` / `EventRepository` — methods: `GetPublishedAsync`, `GetByIdAsync`, `GetByOrganizerIdAsync`, `GetAllAsync` (admin), `CreateAsync`, `UpdateAsync`.
- Create `IEventService` / `EventService` — enforce:
  - New events default to `PendingApproval`.
  - `EndTime` > `StartTime`.
  - `Capacity` > 0.
  - Organizer can only update/cancel own events.
  - No hard deletes — cancel sets `Status = Cancelled`.
- Create `EventsController` — all public and organizer endpoints.
- Verify `GET /api/events` returns only `Published` events.
- Verify `GET /api/events/mine` returns all statuses for the owning organizer.
- Write service-layer unit tests: `EventServiceTests`.

---

## Milestone 7 — Admin Event Approval Workflow

Goal: Admins can approve and reject pending events.

- Create `AdminEventsController`:
  - `GET /api/admin/events` — all events, all statuses.
  - `POST /api/admin/events/{id}/approve` — sets `Status = Published`; only valid from `PendingApproval`.
  - `POST /api/admin/events/{id}/reject` — sets `Status = Rejected`; only valid from `PendingApproval`.
- Validate status-transition rules return `409` when the event is not in `PendingApproval`.
- Add admin event approval tests to `EventServiceTests`.

---

## Milestone 8 — Event Registrations (Attendee)

Goal: Attendees can register for published events with all business rules enforced.

- Create `IRegistrationRepository` / `RegistrationRepository` — methods: `GetByUserIdAsync`, `FindByEventAndUserAsync`, `CreateAsync`, `UpdateAsync`.
- Create `IRegistrationService` / `RegistrationService` — enforce:
  - Event must be `Published`.
  - Capacity must not be exceeded (compare active registrations count to event capacity).
  - Unique `(EventId, UserId)` — reactivate cancelled record instead of inserting duplicate.
  - Attendee can only cancel own registrations; cancel sets `Status = Cancelled`, no hard delete.
- Create `RegistrationsController` — `GET /api/registrations/mine`, `POST /api/registrations`, `DELETE /api/registrations/{id}`.
- Write service-layer unit tests: `RegistrationServiceTests`.

---

## Milestone 9 — Admin User & Registration Views

Goal: Admins can list all users and all registrations.

- Create `AdminUsersController` — `GET /api/admin/users`, `GET /api/admin/users/{id}`.
- Create `AdminRegistrationsController` — `GET /api/admin/registrations` with optional `eventId`/`userId` filters.
- No additional business logic — these are read-only admin views.

---

## Milestone 10 — Validation, Error Handling & Polish

Goal: All endpoints return consistent, well-formed error responses.

- Wire up `ExceptionHandlingMiddleware` to catch unhandled exceptions and return `500` with a safe message.
- Ensure all `400` responses return the `ErrorResponse` envelope with field-level errors.
- Review all controllers for missing `[Authorize]` attributes.
- Review all service methods for missing async/await on DB calls.
- Run all unit tests; fix failures.
- Smoke-test critical flows via Swagger: register, login, create event, approve event, register for event, cancel registration.

---

## Summary Table

| # | Milestone | Key Output |
|---|---|---|
| 1 | Project Setup | Runnable skeleton, DB connection |
| 2 | Entities & Schema | All tables in MySQL |
| 3 | Admin Seeding | Admin user in DB |
| 4 | Auth | Register / Login / JWT |
| 5 | Categories & Venues | Reference data CRUD |
| 6 | Event CRUD | Organizer event management |
| 7 | Admin Approval | Approve / Reject workflow |
| 8 | Registrations | Attendee register / cancel |
| 9 | Admin Views | User & registration read endpoints |
| 10 | Polish | Validation, errors, test pass |
