using BloodDonationAPI.API.Hubs;
using BloodDonationAPI.API.Services;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.BLL.Services;
using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Repositories;
using BloodDonationAPI.DAL.Repositories.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// DATABASE
// =====================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// =====================================================
// CONTROLLERS
// =====================================================

builder.Services.AddControllers();


// =====================================================
// SIGNALR
// =====================================================

builder.Services.AddSignalR();


// =====================================================
// JWT AUTHENTICATION
// =====================================================

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is missing from appsettings.json."
    );
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

        ClockSkew = TimeSpan.Zero
    };

    // =================================================
    // SIGNALR JWT SUPPORT
    // =================================================

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken =
                context.Request.Query["access_token"];

            var path =
                context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// =====================================================
// DEPENDENCY INJECTION — REPOSITORIES & SERVICES
// =====================================================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IBloodRequestRepository, BloodRequestRepository>();
builder.Services.AddScoped<ICommunityNotificationRepository,CommunityNotificationRepository>();
builder.Services.AddScoped<IBloodBatchRepository, BloodBatchRepository>();
builder.Services.AddScoped<IBloodInventoryRepository,BloodInventoryRepository>();
builder.Services.AddScoped<IEmergencyRequestRepository,EmergencyRequestRepository>();
builder.Services.AddScoped<IEmergencyResponseRepository,EmergencyResponseRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IBloodBankSettingRepository,BloodBankSettingRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IEmergencyResponseRepository, EmergencyResponseRepository>();

builder.Services.AddHostedService<BloodRequestExpiryService>();
builder.Services.AddScoped<IEmergencyResponseService, EmergencyResponseService>();
builder.Services.AddScoped<IBloodBankSettingService,BloodBankSettingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICommunityNotificationService,CommunityNotificationService>();
builder.Services.AddScoped<IBloodRequestService, BloodRequestService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBloodInventoryService,BloodInventoryService>();
builder.Services.AddScoped<IBloodBatchService,BloodBatchService>();
builder.Services.AddScoped<INotificationPublisher,SignalRNotificationPublisher>();
builder.Services.AddScoped<IBloodInventoryRepository, BloodInventoryRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// =====================================================
// AUTHORIZATION
// =====================================================

builder.Services.AddAuthorization();


// =====================================================
// CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileApp", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}

// HTTPS redirection is intentionally disabled for local mobile development.
// The Expo client reaches the API over the PC LAN address (HTTP :5097).
// Enable HTTPS redirection again when deploying behind a trusted HTTPS endpoint.


// =====================================================
// STATIC FILES
// =====================================================

app.UseStaticFiles();


// =====================================================
// CORS
// =====================================================

app.UseCors("MobileApp");


// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// CONTROLLERS
// =====================================================

app.MapControllers();


// =====================================================
// SIGNALR HUB
// =====================================================

app.MapHub<NotificationHub>("/notificationHub");


// =====================================================
// RUN
// =====================================================

app.Run();