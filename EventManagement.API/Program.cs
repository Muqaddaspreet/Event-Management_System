using EventManagement.API.Data;
using EventManagement.API.Data.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ───────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── OpenAPI / Swagger ─────────────────────────────────────────────────
// Microsoft.AspNetCore.OpenApi generates the spec; Swashbuckle serves the UI.
builder.Services.AddOpenApi();

// ── CORS — allow Angular dev server ──────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Database (EF Core + Pomelo MySQL) ────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ── Authentication (JWT Bearer) — configured in Milestone 4 ──────────
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options => { ... });

// ── Authorization ─────────────────────────────────────────────────────
builder.Services.AddAuthorization();

// ── Application services (DI) — registered in later milestones ───────
// builder.Services.AddApplicationServices();

// ────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Admin seeding ─────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AdminSeeder.SeedAsync(db, app.Configuration);
}

// ── HTTP pipeline ─────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // Native OpenAPI spec endpoint: GET /openapi/v1.json
    app.MapOpenApi();

    // Swagger UI: http://localhost:<port>/swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "EventManagement API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularDev");

// app.UseAuthentication(); // Milestone 4
app.UseAuthorization();

app.MapControllers();

app.Run();
