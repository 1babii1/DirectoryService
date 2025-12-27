using DirectoryService.Application.Database;
using DirectoryService.Application.Department;
using DirectoryService.Application.Department.Commands;
using DirectoryService.Application.Department.Queries;
using DirectoryService.Application.Location;
using DirectoryService.Application.Location.Commands;
using DirectoryService.Application.Location.Queries;
using DirectoryService.Application.Position;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Repositories.Departments;
using DirectoryService.Infrastructure.Postgres.Repositories.Locations;
using DirectoryService.Infrastructure.Postgres.Repositories.Positions;
using DirectoryService.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Serilog;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddConsole();

builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Host.UseSerilog((context, _, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddHttpLogging();

builder.Services.AddValidatorsFromAssemblyContaining<CreateDepartmentValidation>();

builder.Services.AddSingleton<IConfigureOptions<JsonOptions>, InjectJSONSerializeConfig>();

builder.Services.AddScoped<DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddScoped<ITransactionManager, TransactionManager>();

builder.Services.AddScoped<ILocationsRepository, EfCoreLocationsRepository>();

// builder.Services.AddScoped<ILocationsRepository, NpgsqlLocationsRepository>();
builder.Services.AddScoped<IPositionRepository, EfCorePositionRepository>();

builder.Services.AddScoped<IDepartmentRepository, EfCoreDepartmentsRepository>();

// builder.Services.AddScoped<IReadDbContext, DirectoryServiceDbContext>();
builder.Services.AddScoped<CreateLocationHandle>();

builder.Services.AddScoped<CreatePositionHandle>();

builder.Services.AddScoped<CreateDepartmentHandle>();

builder.Services.AddScoped<UpdateDepartmentLocationsHadler>();

builder.Services.AddScoped<UpdateParentDepartmentHandle>();

builder.Services.AddScoped<GetLocationByIdHandle>();

builder.Services.AddScoped<GetLocationByDepartmentHandle>();

builder.Services.AddScoped<GetDepartmentByIdHandle>();

builder.Services.AddScoped<GetDepartmentByLocationHandle>();

builder.Services.AddScoped<GetDepartmentsTopByPositionsHandle>();

builder.Services.AddScoped<GetParentDepartmentsHandle>();

builder.Services.AddScoped<GetChildrenLazyHandle>();

builder.Services.AddScoped<SoftDeleteDepartmentHandle>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseHttpLogging();

app.UseMiddleware<ExeptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/v1/swagger.json");
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1/swagger.json", "DirectoryService"));
}

app.MapControllers();

app.Run();

namespace DirectoryService
{
    public partial class Program;
}