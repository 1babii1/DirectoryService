using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Application.Department;
using DirectoryService.Application.Location;
using DirectoryService.Application.Position;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Repositories;
using DirectoryService.Infrastructure.Postgres.Repositories.Departments;
using DirectoryService.Infrastructure.Postgres.Repositories.Locations;
using DirectoryService.Infrastructure.Postgres.Repositories.Positions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

// builder.Services.AddScoped<ILocationsRepository, EfCoreLocationsRepository>();
builder.Services.AddScoped<ILocationsRepository, NpgsqlLocationsRepository>();
builder.Services.AddScoped<IPositionRepository, EfCorePositionRepository>();
builder.Services.AddScoped<IDepartmentRepository, EfCoreDepartmentsRepository>();

builder.Services.AddScoped<CreateLocationHandle>();
builder.Services.AddScoped<CreatePositionHandle>();
builder.Services.AddScoped<CreateDepartmentHandle>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/v1/swagger.json");
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1/swagger.json", "DirectoryService"));
}

app.MapControllers();

app.Run();