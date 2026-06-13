# Backend Folder Structure

ASP.NET Core Web API project following the Controllers / Services / Repositories / DTOs / Entities / Data layering.

```
EventManagement.API/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── EventsController.cs
│   ├── RegistrationsController.cs
│   ├── CategoriesController.cs
│   ├── VenuesController.cs
│   └── Admin/
│       ├── AdminDashboardController.cs
│       ├── AdminEventsController.cs
│       ├── AdminUsersController.cs
│       └── AdminRegistrationsController.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IEventService.cs
│   │   ├── IRegistrationService.cs
│   │   ├── ICategoryService.cs
│   │   ├── IVenueService.cs
│   │   └── IAdminDashboardService.cs
│   ├── AuthService.cs
│   ├── EventService.cs
│   ├── RegistrationService.cs
│   ├── CategoryService.cs
│   ├── VenueService.cs
│   └── AdminDashboardService.cs
│
├── Repositories/
│   ├── Interfaces/
│   │   ├── IUserRepository.cs
│   │   ├── IEventRepository.cs
│   │   ├── IRegistrationRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   └── IVenueRepository.cs
│   ├── UserRepository.cs
│   ├── EventRepository.cs
│   ├── RegistrationRepository.cs
│   ├── CategoryRepository.cs
│   └── VenueRepository.cs
│
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   └── AuthResponse.cs
│   ├── Events/
│   │   ├── CreateEventRequest.cs
│   │   ├── UpdateEventRequest.cs
│   │   ├── RejectEventRequest.cs
│   │   ├── EventSummaryResponse.cs
│   │   └── EventDetailResponse.cs
│   ├── Registrations/
│   │   ├── CreateRegistrationRequest.cs
│   │   └── RegistrationResponse.cs
│   ├── Categories/
│   │   ├── CreateCategoryRequest.cs
│   │   ├── UpdateCategoryRequest.cs
│   │   └── CategoryResponse.cs
│   ├── Venues/
│   │   ├── CreateVenueRequest.cs
│   │   ├── UpdateVenueRequest.cs
│   │   └── VenueResponse.cs
│   ├── Users/
│   │   └── UserResponse.cs
│   ├── Admin/
│   │   └── AdminDashboardResponse.cs
│   └── Common/
│       └── ErrorResponse.cs
│
├── Entities/
│   ├── User.cs
│   ├── Event.cs
│   ├── EventRegistration.cs
│   ├── Category.cs
│   └── Venue.cs
│
├── Data/
│   ├── AppDbContext.cs
│   ├── Migrations/               ← EF Core auto-generated
│   └── Seed/
│       └── AdminSeeder.cs
│
├── Enums/
│   ├── UserRole.cs               ← Admin, Organizer, Attendee
│   ├── EventStatus.cs            ← PendingApproval, Published, Rejected, Cancelled
│   └── RegistrationStatus.cs     ← Registered, Cancelled
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs   ← DI registration helpers
│
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---

## Layer Notes

- **Controllers** are thin: they validate ModelState, call one service method, and return an HTTP result.
- **Services** are the only place business rules execute.
- **Repositories** contain all EF Core queries; they receive and return entities.
- **Enums** are shared by entities and mapped to string columns via EF Core value converters.
- **Middleware** provides a single global exception-to-HTTP response handler.
- **Extensions** keeps `Program.cs` clean by grouping DI registrations.

---

## Test Project

```
EventManagement.Tests/
│
├── Services/
│   ├── AuthServiceTests.cs
│   ├── EventServiceTests.cs
│   ├── RegistrationServiceTests.cs
│   ├── CategoryServiceTests.cs
│   └── VenueServiceTests.cs
│
└── EventManagement.Tests.csproj
```

Tests target the service layer only, using mocked repositories.
