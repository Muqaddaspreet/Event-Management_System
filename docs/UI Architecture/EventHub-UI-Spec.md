# EventHub — Event Management System
## Frontend UI / Mock Design Specification (Angular 18)

> Scope: **Frontend UI only**, built with **mock/static data**. No backend, no API integration, no real auth logic. Designed so services can be swapped from mock → real HTTP later with zero template changes. A live, clickable HTML mockup of every screen ships alongside this doc (`EventHub.dc.html`) — use it as the visual source of truth.

---

## 1. Design Summary

EventHub is a modern SaaS-style event booking platform with **four role-based layouts**:

| Layout | Used by | Chrome |
|---|---|---|
| **Public** | anonymous visitors | Light sticky top navbar, no sidebar |
| **Attendee** | logged-in attendees | Light top app-bar (listing/details) + light side-nav (registrations/profile) |
| **Organizer** | event organizers | Light (white) side-nav |
| **Admin** | platform admins | Dark navy side-nav |

**Visual language**
- Clean light background (`#f6f7fb`), white cards, soft 1px borders, soft shadows, 16px rounded corners.
- A single **purple → indigo gradient** (`#7c3aed → #4f46e5`) drives every primary action, active state, logo and accent.
- Dark navy (`#19162f`) sidebar isolates the Admin area; Organizer/Attendee use a white sidebar with a light-purple active pill.
- Type: **Plus Jakarta Sans** throughout (weights 400–800).
- Imagery: event cards/hero use gradient thumbnails as placeholders → swap for real images later.
- Status is always communicated with a colored **pill + dot** badge (never color alone).

**Responsive**: desktop-first, fully responsive. Grids use `auto-fit/minmax` so card counts reflow; sidebars and filter panels collapse under ~920px (replace with a hamburger drawer in implementation).

---

## 2. Theme / Design Tokens

Define once in `src/styles/_tokens.scss` (or `:root` in `styles.scss`).

```scss
:root {
  /* ---- Brand / accent ---- */
  --eh-primary:        #7c3aed;   /* purple */
  --eh-primary-700:    #6d28d9;
  --eh-indigo:         #4f46e5;
  --eh-gradient:       linear-gradient(135deg, #7c3aed, #4f46e5);
  --eh-gradient-soft:  linear-gradient(135deg, #7c3aed, #4f46e5, #a855f7);
  --eh-primary-tint:   #f4f0ff;   /* active pill / icon bg */
  --eh-primary-tint-2: #f7f5ff;

  /* ---- Surfaces ---- */
  --eh-bg:        #f6f7fb;        /* app background */
  --eh-surface:   #ffffff;        /* cards */
  --eh-navy:      #19162f;        /* admin sidebar */
  --eh-navy-muted:#9b97c0;        /* admin nav idle text */

  /* ---- Text ---- */
  --eh-text:      #1a1d29;        /* primary */
  --eh-text-2:    #475467;        /* body */
  --eh-text-3:    #667085;        /* secondary */
  --eh-muted:     #98a2b3;        /* tertiary / placeholder */

  /* ---- Lines ---- */
  --eh-border:    #eceef4;
  --eh-border-2:  #d0d5dd;        /* inputs */
  --eh-divider:   #f0f1f5;

  /* ---- Status ---- */
  --eh-green-bg:#ecfdf3; --eh-green:#067647; --eh-green-dot:#12b76a;  /* Published / Confirmed */
  --eh-amber-bg:#fffaeb; --eh-amber:#b54708; --eh-amber-dot:#f79009;  /* PendingApproval */
  --eh-red-bg:#fef3f2;   --eh-red:#b42318;   --eh-red-dot:#f04438;    /* Rejected */
  --eh-gray-bg:#f2f4f7;  --eh-gray:#475467;  --eh-gray-dot:#98a2b3;   /* Cancelled */
  --eh-blue-bg:#eff8ff;  --eh-blue:#175cd3;  --eh-blue-dot:#2e90fa;   /* Registered */
  --eh-violet-bg:#f4f3ff;--eh-violet:#5925dc;--eh-violet-dot:#7a5af8; /* Draft */

  /* ---- Radius / shadow / spacing ---- */
  --eh-r-sm: 10px;  --eh-r-md: 14px;  --eh-r-lg: 16px;  --eh-r-xl: 18px;  --eh-r-pill: 999px;
  --eh-shadow-sm: 0 1px 2px rgba(16,24,40,.05);
  --eh-shadow-md: 0 8px 28px rgba(16,24,40,.08);
  --eh-shadow-pop: 0 16px 40px rgba(16,24,40,.12);
  --eh-shadow-primary: 0 8px 20px rgba(124,58,237,.32);
  --eh-space: 4px; /* base unit; use multiples: 8/12/16/20/24/28/32/36 */
}
```

### Typography rules
- Font family: `'Plus Jakarta Sans', system-ui, sans-serif` (Google Fonts).
- Scale: H1 26–34 / 800 · H2 19–22 / 700 · H3 16 / 700 · Body 14–15 / 400–500 · Label 13 / 600 · Caption 12 / 500.
- Letter-spacing: tighten large headings (`-0.5px` to `-1.5px`). Line-height 1.6–1.7 for body copy.
- Numbers/stat figures: 30px / 800.

### Button styles
| Variant | Spec |
|---|---|
| **Primary** | `background: var(--eh-gradient)`, white text, `--eh-r-sm`, `padding:11px 18px`, `font-weight:600`, `box-shadow: var(--eh-shadow-primary)`. Hover: `translateY(-1px)`. |
| **Secondary** | white bg, `1px solid var(--eh-border-2)`, text `#344054`. |
| **Success** | solid `#12b76a` (Approve). |
| **Danger (outline)** | white bg, `1px solid #fda29b`, text `--eh-red` (Reject). |
| **Ghost / link** | transparent, `--eh-primary` text, hover underline-color shift. |
| Sizes | sm `9px 16px` · md `11px 18px` · lg `13–14px` full-width. |

### Card styles
`background: var(--eh-surface); border:1px solid var(--eh-border); border-radius: var(--eh-r-lg); box-shadow: var(--eh-shadow-sm);`
Interactive cards add `.eh-card-hover`: `transition` + on hover `translateY(-4px)` and `--eh-shadow-pop`.

### Form field styles
`width:100%; padding:11–12px 14px; border:1px solid var(--eh-border-2); border-radius: var(--eh-r-sm); font-size:14px;`
Focus: `border-color: var(--eh-primary); box-shadow: 0 0 0 3px rgba(124,58,237,.12)`.
Disabled/read-only: `background:#f9fafc; color: var(--eh-muted)`.
Label: 13/600 `#344054`, 7px gap. Required marker: red `*`. Helper/error text: 12px (error uses `--eh-red`).
Upload dropzone: `1.5px dashed var(--eh-border-2)`, tinted icon chip, centered copy.

### Table styles
- Header row: `#fafafe` bg, 12px/700 uppercase `letter-spacing:.5px` `--eh-muted`.
- Body rows: 14px, `1px solid #f5f6f9` row divider, hover `#fafafe`.
- Use CSS grid columns (e.g. `2.2fr 1.6fr 1.2fr 1.2fr .9fr`) so columns align; actions right-aligned icon group (view / edit / delete).
- Pagination: prev/next chevrons + numbered pills, active = gradient.

### Badge / status styles
Reusable `StatusBadgeComponent` — pill with leading dot. `padding:5px 11px; border-radius: pill; font-size:12px; font-weight:700`.

| Status | bg / text / dot |
|---|---|
| `Published` | green-bg / green / green-dot |
| `PendingApproval` | amber-bg / amber / amber-dot |
| `Rejected` | red-bg / red / red-dot |
| `Cancelled` | gray-bg / gray / gray-dot |
| `Registered` / `Confirmed` | blue/green-bg / blue/green |
| `Draft` | violet-bg / violet / violet-dot |

### Sidebar / navbar behavior
- **Public navbar**: sticky, blurred white, logo left, center links (Home/Events/Categories/Venues/About), Login (secondary) + Register (primary) right. Mobile: links hide → hamburger drawer.
- **Attendee top app-bar**: logo + search field + bell (with unread dot) + avatar (→ profile).
- **White sidebar** (Attendee/Organizer): 248px, logo, section label, nav items; idle `#667085`, active = `--eh-primary-tint` bg + `--eh-primary-700` text; Logout pinned bottom (`margin-top:auto`).
- **Dark sidebar** (Admin): 248px, `--eh-navy`; idle `--eh-navy-muted`, active = gradient pill with shadow.
- Sidebars `position: sticky; top:0`. Under ~920px collapse to off-canvas drawer toggled by a hamburger in a compact top bar.

---

## 3. Page-by-Page Mock Specification

> Every page uses mock data from in-memory services. List pages need **loading** (skeleton cards/rows) and **empty** states; detail/forms need **error/validation** states.

### 1. Public Landing / Home  · route `/`
- **Purpose**: Convert visitors — showcase events, drive search/registration.
- **Layout**: Public navbar → hero (2-col: copy+search / gradient feature panel) → Popular Categories → Upcoming Events → footer.
- **Sections/components**: `PublicNavbar`, hero with search box + stat row (1.2k events / 86k attendees / 120 cities), `CategoryCard` grid, `EventCard` grid (4), footer.
- **Mock data**: 5 categories, 4 featured events.
- **Actions**: Search → `/events`; category → `/events?category=`; card → `/events/:id`; Login/Register.
- **States**: skeleton for category/event grids while "loading".
- **Responsive**: hero stacks; nav links → hamburger; grids reflow to 1–2 cols.

### 2. Login  · route `/login`
- **Purpose**: Authenticate (mock — any input routes in).
- **Layout**: Split screen — left brand/gradient panel (illustration placeholder + value prop), right form.
- **Components**: email + password (with show/hide eye) inputs, "Forgot password?", primary Login, divider, "Continue with Google", "Register here" link.
- **States**: inline field validation (required, email format), error banner on "failed" mock, button loading spinner.
- **Responsive**: brand panel hides < 920px; form centers full-width.
- **Actions**: Login → attendee `/events` (mock); links to register/forgot.

### 3. Register  · route `/register`
- **Purpose**: Create account + pick role.
- **Layout**: Split screen (form left, gradient panel right with benefit checklist).
- **Components**: full name, email, password, confirm password, **Role selector** (Attendee / Organizer cards — selectable), Create account, "Login here".
- **States**: validation (passwords match, required, email), role required, success → `/login`.
- **Responsive**: panel hides < 920px; role cards stack to 1 col on narrow.

### 4. Events Listing (Attendee)  · route `/events`
- **Purpose**: Browse/filter/sort all events.
- **Layout**: Attendee top app-bar → 2-col: left **Filters** panel (sticky) + right results.
- **Components**: search, `FilterPanel` (category radio list, date, price min/max, Apply, Clear all), results header (count + sort dropdown), `EventCard` grid (`auto-fit minmax(220px)`), pagination.
- **Mock data**: 8 events across 5 categories. Category filter is functional in the mockup.
- **States**: loading skeleton grid; **empty** ("No events match your filters" + Clear); error retry.
- **Responsive**: filters → top collapsible / drawer < 920px; grid 1–2 cols.
- **Actions**: filter/sort, card → details, paginate.

### 5. Event Details  · route `/events/:id`
- **Purpose**: Full info + register.
- **Layout**: Top app-bar → back link → gradient hero banner (category pill + title) → 2-col: main (About, What you'll learn checklist, Organizer card) + sticky **register card** (price, date/time/location/seats rows, Register now, Save for later).
- **Mock data**: selected event + organizer (Tech Events Inc., verified, 24 events).
- **States**: loading skeleton; sold-out (disable Register); already-registered (show "Registered" + go to My Registrations); error/not-found.
- **Actions**: Register now → confirmation toast → `/my-registrations`; back; save.
- **Responsive**: columns stack; register card moves below content.

### 6. My Registrations (Attendee)  · route `/my-registrations`
- **Purpose**: Manage bookings.
- **Layout**: White sidebar (Browse events / My registrations* / Profile / Logout) + content.
- **Components**: title, **tabs** (Upcoming / Past / Cancelled — functional), registration rows (thumb, title, date·location, status badge, View details).
- **Mock data**: 6 registrations tagged by tab + status (Confirmed / Cancelled).
- **States**: per-tab **empty** state (icon + "Browse events" CTA); loading row skeletons.
- **Actions**: switch tab, view details, cancel registration (→ moves to Cancelled).
- **Responsive**: sidebar → drawer; rows stack content.

### 7. Organizer Dashboard  · route `/organizer`
- **Purpose**: Organizer home — performance + quick actions.
- **Layout**: White sidebar (Dashboard* / My events / Create event / Registrations / Profile / Logout) + content.
- **Components**: header + **Create event** button, 4 `StatCard`s (Total Events 12, Published 7, Pending Approval 3, Total Registrations 256), Recent Events list (thumb, title, date·location, status, Manage).
- **States**: zero-state ("Create your first event"); loading skeletons.
- **Actions**: Create event → form; Manage → edit; View all.
- **Responsive**: stat cards `auto-fit`; sidebar drawer.

### 8. Create / Edit Event (Organizer)  · route `/organizer/events/new` & `/organizer/events/:id/edit`
- **Purpose**: Create or edit an event (same form, prefilled for edit).
- **Layout**: White sidebar + form card (max ~940px).
- **Components/fields**: Title*, Description* (textarea), Start date-time*, End date-time*, Capacity, Price ($, 0=free), Category* (select), Venue* (select), Event image (dropzone). Footer: Cancel / Create event (or Save changes).
- **States**: required validation, date-order validation (end > start), image preview, submit loading, success toast.
- **Actions**: Cancel → dashboard; Submit → mock add/update → dashboard + toast.
- **Responsive**: paired fields → single column < 920px.

### 9. Admin Dashboard  · route `/admin`
- **Purpose**: Platform KPIs + trends.
- **Layout**: Dark navy sidebar (Dashboard* / Users / Events / Categories / Venues / Registrations / Logout) + content.
- **Components**: header + admin avatar, 4 `StatCard`s (Total Users 342, Total Events 86, Pending Events 14, Total Registrations 1,245), 2 chart cards — **Registrations overview** (line/area) + **Events overview** (bar). Use a light SVG chart (ngx-charts or hand-rolled SVG; mock data inline).
- **States**: loading skeleton for cards + charts.
- **Responsive**: charts stack < 920px; sidebar drawer.

### 10. Manage Events (Admin)  · route `/admin/events`
- **Purpose**: Master moderation table.
- **Layout**: Dark sidebar + content.
- **Components**: header + Add event, status **tabs/segmented filter** (All / Pending Approval / Published / Cancelled — functional), data table (Title, Organizer, Date, Status badge, Actions: view→approval / edit / delete), pagination.
- **Mock data**: 6 events with varied statuses.
- **States**: filtered empty state; delete confirm dialog; loading rows.
- **Actions**: filter, view → approval page, edit, delete (confirm).
- **Responsive**: table → stacked cards < 768px (or horizontal scroll).

### 11. Approve / Reject Event (Admin)  · route `/admin/events/:id/review`
- **Purpose**: Moderate a single pending event.
- **Layout**: Dark sidebar + back link + 2-col: left **event preview** (banner, title, organizer/category/date/location/capacity/price grid, description) + right sticky **Review actions** card.
- **Components**: Approve & publish (success), Reject (danger outline), Notes textarea (optional feedback), pending badge in header.
- **States**: confirm on reject (notes encouraged), success toast, already-moderated lock.
- **Actions**: Approve → status Published + toast → manage; Reject → status Rejected + toast → manage.
- **Responsive**: columns stack; actions card moves below.

### 12. User Profile  · route `/profile`
- **Purpose**: View/edit account.
- **Layout**: White sidebar + profile card (gradient cover, avatar, name/role/member-since).
- **Components**: Edit profile toggle, fields (Full name, Email, Phone, Role [read-only], Bio). View mode = read-only tinted fields; Edit mode = editable + Save/Cancel.
- **States**: edit/save (with loading), validation, avatar change (Change photo), success toast.
- **Actions**: toggle edit, save, cancel, change photo.
- **Responsive**: field grid → 1 col; sidebar drawer.

---

## 4. Recommended Angular Component Breakdown

**Standalone components** (Angular 18). Group as `layout`, `shared` (dumb/presentational), `features` (smart/pages).

```
Layouts
  PublicLayoutComponent        (navbar + <router-outlet> + footer)
  AttendeeLayoutComponent      (top app-bar / side-nav + outlet)
  OrganizerLayoutComponent     (white side-nav + outlet)
  AdminLayoutComponent         (dark side-nav + outlet)

Shared (presentational, @Input/@Output, OnPush)
  PublicNavbarComponent
  SideNavComponent             (config-driven: items[], theme: 'light'|'dark', activeId)
  TopAppBarComponent           (search, notifications, avatar)
  EventCardComponent           (@Input event; @Output open)
  CategoryCardComponent
  StatCardComponent            (label, value, delta, icon, tint)
  StatusBadgeComponent         (@Input status: EventStatus | RegStatus)
  TabsComponent / SegmentedControlComponent (@Input tabs; @Output change)
  FilterPanelComponent         (@Output filtersChange)
  DataTableComponent           (generic columns + row templates) or per-feature table
  PaginationComponent
  EmptyStateComponent          (icon, title, message, ctaLabel)
  SkeletonComponent            (card / row / chart variants)
  ButtonComponent (optional)   (variant, size, loading)
  FormFieldComponent (optional)(label, error, required wrapper)
  ToastService + ToastOutlet
  ConfirmDialogComponent
  LineChartComponent / BarChartComponent (ngx-charts or SVG)

Feature pages
  LandingPageComponent
  LoginPageComponent / RegisterPageComponent
  EventsListPageComponent / EventDetailsPageComponent
  MyRegistrationsPageComponent
  OrganizerDashboardPageComponent / EventFormPageComponent
  AdminDashboardPageComponent / AdminManageEventsPageComponent / AdminEventReviewPageComponent
  ProfilePageComponent
```

Conventions: presentational components `ChangeDetectionStrategy.OnPush`, pure `@Input`/`@Output`; pages inject mock services and hold view state (signals recommended). Route data/guards stub the role; no real auth.

---

## 5. Suggested Angular Folder Structure

```
src/
  app/
    app.routes.ts
    app.config.ts
    core/
      models/            event.model.ts, category.model.ts, venue.model.ts,
                         registration.model.ts, user.model.ts, enums.ts
      services/          event.service.ts, category.service.ts, venue.service.ts,
                         registration.service.ts, user.service.ts, auth.service.ts (mock)
      mock/              mock-events.ts, mock-categories.ts, mock-venues.ts,
                         mock-registrations.ts, mock-users.ts
      guards/            role.guard.ts (stub)
    layout/
      public-layout/  attendee-layout/  organizer-layout/  admin-layout/
    shared/
      components/        (all presentational components above)
      ui/                button/ form-field/ toast/ confirm-dialog/
      pipes/  directives/
    features/
      public/            landing/ login/ register/
      attendee/          events-list/ event-details/ my-registrations/
      organizer/         dashboard/ event-form/
      admin/             dashboard/ manage-events/ event-review/
      profile/
  styles/
    styles.scss  _tokens.scss  _mixins.scss  _buttons.scss  _forms.scss  _tables.scss
  assets/  (placeholder images, logo)
```

### Suggested route structure (`app.routes.ts`)

```ts
export const routes: Routes = [
  { path: '', component: PublicLayoutComponent, children: [
    { path: '', component: LandingPageComponent },
    { path: 'login', component: LoginPageComponent },
    { path: 'register', component: RegisterPageComponent },
  ]},
  { path: '', component: AttendeeLayoutComponent, /* canActivate: [roleGuard('Attendee')] */ children: [
    { path: 'events', component: EventsListPageComponent },
    { path: 'events/:id', component: EventDetailsPageComponent },
    { path: 'my-registrations', component: MyRegistrationsPageComponent },
    { path: 'profile', component: ProfilePageComponent },
  ]},
  { path: 'organizer', component: OrganizerLayoutComponent, /* roleGuard('Organizer') */ children: [
    { path: '', component: OrganizerDashboardPageComponent },
    { path: 'events/new', component: EventFormPageComponent },
    { path: 'events/:id/edit', component: EventFormPageComponent },
  ]},
  { path: 'admin', component: AdminLayoutComponent, /* roleGuard('Admin') */ children: [
    { path: '', component: AdminDashboardPageComponent },
    { path: 'events', component: AdminManageEventsPageComponent },
    { path: 'events/:id/review', component: AdminEventReviewPageComponent },
  ]},
  { path: '**', redirectTo: '' },
];
```

---

## 6. Mock Data Model Examples

```ts
// core/models/enums.ts
export type EventStatus = 'Published' | 'PendingApproval' | 'Rejected' | 'Cancelled' | 'Draft';
export type RegistrationStatus = 'Registered' | 'Confirmed' | 'Cancelled';
export type UserRole = 'Attendee' | 'Organizer' | 'Admin';

// core/models/category.model.ts
export interface Category { id: string; name: string; icon: string; eventCount: number; }

// core/models/venue.model.ts
export interface Venue { id: string; name: string; address: string; city: string; capacity: number; }

// core/models/user.model.ts
export interface User {
  id: string; fullName: string; email: string; phone?: string;
  role: UserRole; avatarUrl?: string; bio?: string; memberSince: string; /* ISO */
}

// core/models/event.model.ts
export interface EventItem {
  id: string; title: string; description: string;
  startDate: string; endDate: string;          // ISO
  location: string; venueId: string;
  categoryId: string; categoryName: string;
  price: number;                                // 0 = Free
  capacity: number; seatsBooked: number;
  status: EventStatus;
  organizerId: string; organizerName: string;
  imageUrl?: string;                            // null → gradient placeholder
}

// core/models/registration.model.ts
export interface Registration {
  id: string; eventId: string; userId: string;
  status: RegistrationStatus; registeredOn: string; // ISO
}
```

```ts
// core/mock/mock-events.ts (excerpt — matches the HTML mockup)
export const MOCK_EVENTS: EventItem[] = [
  { id:'e1', title:'Tech Conference 2024', description:'Two days of talks & workshops.',
    startDate:'2024-05-20T09:00', endDate:'2024-05-21T17:00', location:'New York', venueId:'v1',
    categoryId:'c2', categoryName:'Tech', price:99, capacity:200, seatsBooked:120,
    status:'Published', organizerId:'o1', organizerName:'Tech Events Inc.' },
  { id:'e2', title:'Summer Music Festival', startDate:'2024-06-15T16:00', endDate:'2024-06-15T23:00',
    location:'Los Angeles', venueId:'v2', categoryId:'c1', categoryName:'Music', price:59,
    capacity:1000, seatsBooked:850, status:'PendingApproval', organizerId:'o2',
    organizerName:'Music World', description:'Live performances all night.' },
  // Business Workshop ($49, Chicago, Published), Art Exhibition (Free, NY, PendingApproval),
  // Sports Tournament ($30, Houston, Cancelled), Photography Workshop ($75, SF, Draft) ...
];

// core/mock/mock-categories.ts
export const MOCK_CATEGORIES: Category[] = [
  { id:'c1', name:'Music', icon:'music', eventCount:12 },
  { id:'c2', name:'Tech', icon:'cpu', eventCount:8 },
  { id:'c3', name:'Business', icon:'briefcase', eventCount:15 },
  { id:'c4', name:'Sports', icon:'trophy', eventCount:9 },
  { id:'c5', name:'Art & Culture', icon:'palette', eventCount:7 },
];
```

```ts
// core/services/event.service.ts — mock now, HTTP-ready later
@Injectable({ providedIn: 'root' })
export class EventService {
  private events = signal<EventItem[]>(MOCK_EVENTS);
  list(filter?: { categoryId?: string }): Observable<EventItem[]> {
    let data = this.events();
    if (filter?.categoryId) data = data.filter(e => e.categoryId === filter.categoryId);
    return of(data).pipe(delay(300)); // simulate latency → drives skeletons
  }
  getById(id: string) { return of(this.events().find(e => e.id === id)).pipe(delay(300)); }
  create(e: EventItem) { this.events.update(list => [{ ...e, id: crypto.randomUUID() }, ...list]); }
  setStatus(id: string, status: EventStatus) {
    this.events.update(l => l.map(e => e.id === id ? { ...e, status } : e));
  }
}
// Later: replace `of(...)` bodies with `this.http.get<...>('/api/events')` — signatures stay identical.
```

---

## 7. Step-by-Step Implementation Plan (for Claude Code)

1. **Scaffold tokens & global styles** — add Plus Jakarta Sans (`index.html` link), create `styles/_tokens.scss` + partials (`_buttons`, `_forms`, `_tables`), import into `styles.scss`. Do **not** disturb existing structure.
2. **Models & enums** — add all interfaces in `core/models`.
3. **Mock data + services** — `core/mock/*` and signal-based services returning `of(...).pipe(delay())`. Stub `AuthService.currentRole` (settable, default Attendee).
4. **Shared presentational components** — build `StatusBadge`, `EventCard`, `StatCard`, `SideNav`, `TopAppBar`, `Tabs/SegmentedControl`, `FilterPanel`, `DataTable`, `Pagination`, `EmptyState`, `Skeleton`, `Toast`. Match the mockup exactly.
5. **Layouts** — `Public`, `Attendee`, `Organizer`, `Admin` layout components, each with its nav + `<router-outlet>`.
6. **Routing** — wire `app.routes.ts` as in §5 (guards stubbed/commented).
7. **Public pages** — Landing, Login, Register (with role selector + client-side validation).
8. **Attendee pages** — Events list (functional category filter + sort + pagination + empty/loading), Event details (register → toast → my-registrations), My Registrations (functional tabs + empty), Profile (edit toggle).
9. **Organizer pages** — Dashboard (stats + recent), Event form (validation, edit prefill).
10. **Admin pages** — Dashboard (stats + 2 charts), Manage Events (status filter + table + delete confirm), Event Review (approve/reject → status update + toast).
11. **States polish** — wire loading skeletons, empty states, toasts, confirm dialogs everywhere noted in §3.
12. **Responsive pass** — collapse sidebars/filters to drawers < 920px; verify all grids reflow; table → cards on mobile.
13. **QA** — navigate every route, confirm no console errors, confirm services swap-ready.

Each step should compile and run independently before moving on.

---

## 8. Final Claude Code Prompt (paste directly)

> **You are implementing the EventHub frontend in an existing Angular 18 project. Build UI only with mock/static data — no backend, no HTTP, no real authentication. Follow `EventHub-UI-Spec.md` and the visual mockup `EventHub.dc.html` as the source of truth.**
>
> **Hard constraints**
> - Do **not** remove, rename, or break existing project files/structure. Add new code under `src/app/{core,layout,shared,features}` and `src/styles`.
> - Angular **standalone components** only; use **signals** for state and `ChangeDetectionStrategy.OnPush` for presentational components.
> - Plain **SCSS** with the CSS variables/tokens defined in §2. No new CSS framework. Add Plus Jakarta Sans via Google Fonts.
> - **Mock data only**: services return `of(mockData).pipe(delay(300))`. Keep service method signatures HTTP-ready so `of(...)` can later be replaced with `HttpClient` calls without touching components.
> - Keep the **four role layouts separated**: Public, Attendee, Organizer, Admin (separate layout components + nav). Role is a settable stub on a mock `AuthService`; do not implement real auth.
> - No real form submission — validate client-side, mutate the in-memory signal stores, show a toast, and navigate.
>
> **Design system** (from spec): purple→indigo gradient `#7c3aed→#4f46e5` for all primary actions/active states; light `#f6f7fb` app bg; white cards, 1px `#eceef4` borders, 16px radius, soft shadows; Admin uses a dark navy `#19162f` sidebar, Attendee/Organizer a white sidebar with a light-purple active pill; status pill+dot badges for Published/PendingApproval/Rejected/Cancelled/Registered/Draft. Gradient placeholders for event images.
>
> **Build, in order** (each step must compile before the next): (1) tokens + global SCSS, (2) models + enums, (3) mock data + signal services + stub AuthService, (4) shared presentational components, (5) four layouts, (6) routing per spec, (7) Public pages, (8) Attendee pages, (9) Organizer pages, (10) Admin pages incl. dashboard charts, (11) loading/empty/error states + toasts + confirm dialogs, (12) responsive pass (sidebars/filters → drawers < 920px), (13) QA all routes with zero console errors.
>
> **Deliver**: all 12 pages matching the mockup, fully navigable with mock data, responsive, and structured so real API services drop in later. Report the final folder tree and any assumptions you made.

---

*Companion file: `EventHub.dc.html` — the live, clickable mockup of all 12 screens (open it, click any card on the index to explore each flow).*
