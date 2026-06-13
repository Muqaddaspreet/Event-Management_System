# Frontend Folder Structure

Angular project organized by feature module. Each feature owns its own components, services, and models.

```
event-management-ui/
│
├── src/
│   ├── app/
│   │   │
│   │   ├── core/                         ← Singleton services, guards, interceptors
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── role.guard.ts
│   │   │   ├── interceptors/
│   │   │   │   └── auth.interceptor.ts   ← Attaches Bearer token to requests
│   │   │   ├── services/
│   │   │   │   └── auth.service.ts       ← Login, register, token storage
│   │   │   └── models/
│   │   │       └── user.model.ts
│   │   │
│   │   ├── shared/                       ← Reusable UI components and pipes
│   │   │   ├── components/
│   │   │   │   ├── navbar/
│   │   │   │   ├── footer/
│   │   │   │   └── loading-spinner/
│   │   │   └── models/
│   │   │       ├── api-error.model.ts
│   │   │       └── pagination.model.ts
│   │   │
│   │   ├── features/
│   │   │   │
│   │   │   ├── auth/
│   │   │   │   ├── login/
│   │   │   │   │   ├── login.component.ts
│   │   │   │   │   └── login.component.html
│   │   │   │   ├── register/
│   │   │   │   │   ├── register.component.ts
│   │   │   │   │   └── register.component.html
│   │   │   │   └── auth.routes.ts
│   │   │   │
│   │   │   ├── events/                   ← Public and organizer event pages
│   │   │   │   ├── services/
│   │   │   │   │   └── event.service.ts
│   │   │   │   ├── models/
│   │   │   │   │   ├── event-summary.model.ts
│   │   │   │   │   └── event-detail.model.ts
│   │   │   │   ├── event-list/           ← Public: published events
│   │   │   │   │   ├── event-list.component.ts
│   │   │   │   │   └── event-list.component.html
│   │   │   │   ├── event-detail/         ← Public event detail page
│   │   │   │   │   ├── event-detail.component.ts
│   │   │   │   │   └── event-detail.component.html
│   │   │   │   ├── event-form/           ← Create / edit event (Organizer)
│   │   │   │   │   ├── event-form.component.ts
│   │   │   │   │   └── event-form.component.html
│   │   │   │   ├── my-events/            ← Organizer: own events all statuses
│   │   │   │   │   ├── my-events.component.ts
│   │   │   │   │   └── my-events.component.html
│   │   │   │   └── events.routes.ts
│   │   │   │
│   │   │   ├── registrations/            ← Attendee registration pages
│   │   │   │   ├── services/
│   │   │   │   │   └── registration.service.ts
│   │   │   │   ├── models/
│   │   │   │   │   └── registration.model.ts
│   │   │   │   ├── my-registrations/
│   │   │   │   │   ├── my-registrations.component.ts
│   │   │   │   │   └── my-registrations.component.html
│   │   │   │   └── registrations.routes.ts
│   │   │   │
│   │   │   ├── dashboards/
│   │   │   │   ├── organizer-dashboard/
│   │   │   │   │   ├── organizer-dashboard.component.ts
│   │   │   │   │   └── organizer-dashboard.component.html
│   │   │   │   ├── attendee-dashboard/
│   │   │   │   │   ├── attendee-dashboard.component.ts
│   │   │   │   │   └── attendee-dashboard.component.html
│   │   │   │   └── dashboards.routes.ts
│   │   │   │
│   │   │   └── admin/                    ← Admin-only pages
│   │   │       ├── services/
│   │   │       │   └── admin.service.ts
│   │   │       ├── admin-dashboard/
│   │   │       │   ├── admin-dashboard.component.ts
│   │   │       │   └── admin-dashboard.component.html
│   │   │       ├── manage-events/        ← All events, all statuses
│   │   │       │   ├── manage-events.component.ts
│   │   │       │   └── manage-events.component.html
│   │   │       ├── manage-users/
│   │   │       │   ├── manage-users.component.ts
│   │   │       │   └── manage-users.component.html
│   │   │       ├── manage-categories/
│   │   │       │   ├── manage-categories.component.ts
│   │   │       │   └── manage-categories.component.html
│   │   │       ├── manage-venues/
│   │   │       │   ├── manage-venues.component.ts
│   │   │       │   └── manage-venues.component.html
│   │   │       ├── view-registrations/
│   │   │       │   ├── view-registrations.component.ts
│   │   │       │   └── view-registrations.component.html
│   │   │       └── admin.routes.ts
│   │   │
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   └── app.routes.ts
│   │
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   │
│   └── index.html
│
├── angular.json
├── package.json
└── tsconfig.json
```

---

## Key Design Decisions

- **Feature modules** are standalone; each feature folder is self-contained with its own services, models, components, and routes.
- **`core/`** is loaded once at app startup — guards and the auth interceptor live here.
- **`shared/`** contains only presentational, stateless components and utility models.
- **`auth.interceptor.ts`** automatically attaches `Authorization: Bearer <token>` to outbound HTTP requests.
- **`auth.guard.ts`** protects routes requiring authentication. **`role.guard.ts`** further restricts by role (`Admin`, `Organizer`, `Attendee`).
- **`event-form/`** is reused for both Create and Edit; the route determines which API call is made.
- Admin routes are grouped under `/admin` and protected by `role.guard.ts` with the `Admin` role.
