# Event Management System — Requirements (V1 final)

Purpose

- A junior-to-mid-level portfolio Event Management System demonstrating full-stack development with Angular (frontend), ASP.NET Core Web API (backend), MySQL + EF Core, and JWT access-token authentication.

Core constraints for V1

- Use int primary keys for all entities.
- Event statuses (V1 only): PendingApproval, Published, Rejected, Cancelled.
- Remove `Draft` status from V1.
- Organizer-created events default to `PendingApproval`.
- Public users may only view `Published` events; Admins can view all statuses; Organizers can view only their own events across statuses.
- Do not hard-delete events or registrations in V1. Cancellation is represented by `Status = Cancelled`.
- Admin users must be seeded manually; public registration cannot create Admin accounts.
- JWT access tokens only for V1 (no refresh tokens).

User roles

- Admin: manage users, categories, venues, view/approve/reject events, view registrations, view dashboard statistics.
- Organizer: create events (defaults to `PendingApproval`), edit/cancel own events, view registrations for own events, organizer dashboard.
- Attendee: browse published events, register/cancel own registrations, view own registrations, attendee dashboard.

Functional requirements (high level)

- Authentication: register/login with password hashing and JWT access token issuance.
- Events: create, update, cancel (set `Cancelled`), search/filter, view details. Organizers create events which require admin approval before publication.
- Registration: attendees register for `Published` events only. Enforce capacity limits. Registrations have statuses `Registered` or `Cancelled`. Re-registration reactivates cancelled registration when present.
- Admin workflows: approve or reject pending events; manage categories and venues; view system-wide dashboards and registrations.

Business rules (selected)

- Unique user emails.
- Event end time must be after start time.
- Event capacity must be > 0.
- Public endpoints and unauthenticated views expose only `Published` events.
- Organizers can only modify their own events.
- Registrations are unique per (EventId, UserId). Reactivate cancelled registration records instead of inserting duplicates.

Pages and UI

- Public: landing, events listing (published only), event details, login, register.
- Attendee: attendee dashboard, my registrations, profile.
- Organizer: organizer dashboard, my events (all statuses for owner), create/edit event pages.
- Admin: admin dashboard, manage users, manage events, approve/reject events, manage categories/venues, view registrations.

Testing scope

- Focus on service-layer unit tests first: AuthService, EventService, RegistrationService, CategoryService, VenueService.
- API testing via Swagger/Postman for critical flows.

Out of scope for V1

- Payments, email notifications, image uploads, refresh tokens, deployment automation, and advanced realtime features.

Success criteria

- Users can register/login and receive JWT access tokens.
- Role-based access is enforced.
- Organizers can create events that require admin approval.
- Attendees can browse published events and manage registrations with business rules enforced.
- Events and registrations use `Cancelled` status instead of hard deletes.
