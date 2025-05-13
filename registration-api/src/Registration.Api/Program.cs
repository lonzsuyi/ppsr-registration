using Microsoft.EntityFrameworkCore;
using Registration.Api.Background;
using Registration.Domain.Interfaces;
using Registration.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add Log configuration
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
    options.SingleLine = true;
});

// Add Cors configuration
var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUi", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register EF Core InMemory Database
builder.Services.AddDbContext<Registration.Infrastructure.Persistence.ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("PpsrDb"));

// Add Background Services
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<CsvProcessingWorker>();

// Add services to the container. 
builder.Services.AddScoped<IVehicleRegistrationRepository, VehicleRegistrationRepository>();
builder.Services.AddScoped<IUploadedFileRepository, UploadedFileRepository>();
builder.Services.AddScoped<IRegistrationService, Registration.Application.Services.RegistrationService>();
builder.Services.AddScoped<IUploadTaskStatusRepository, UploadTaskStatusRepository>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Log App Startup
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Application starting up in {Environment} mode", app.Environment.EnvironmentName);

// Global request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request Start: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("Request End: {Method} {Path} - {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
});

app.UseCors("AllowWebUi");

// Configure Swagger
var swaggerEnabled = configuration.GetValue<bool>("Swagger:Enabled");
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
    startupLogger.LogInformation("Swagger UI enabled at /swagger");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

startupLogger.LogInformation("Application started successfully at {Timestamp}", DateTime.UtcNow);

app.Run();
