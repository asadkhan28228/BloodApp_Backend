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

// =====================================================
// CONTROLLERS + OPENAPI
// =====================================================

builder.Services.AddControllers();
builder.Services.AddOpenApi();


// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// =====================================================
// SIGNALR
// =====================================================

builder.Services.AddSignalR();


// =====================================================
// HTTP CLIENT
// Required by SignalRNotificationPublisher for Expo Push
// =====================================================

builder.Services.AddHttpClient();


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

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };

        // =============================================
        // SIGNALR JWT SUPPORT
        // =============================================

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken =
                    context.Request.Query["access_token"];

                var path =
                    context.HttpContext.Request.Path;

                // SignalR sends JWT through query string
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
// REPOSITORIES
// =====================================================

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IRefreshTokenRepository,
    RefreshTokenRepository>();

builder.Services.AddScoped<
    IBloodRequestRepository,
    BloodRequestRepository>();

builder.Services.AddScoped<
    ICommunityNotificationRepository,
    CommunityNotificationRepository>();

builder.Services.AddScoped<
    IBloodBatchRepository,
    BloodBatchRepository>();

builder.Services.AddScoped<
    IBloodInventoryRepository,
    BloodInventoryRepository>();

builder.Services.AddScoped<
    IEmergencyRequestRepository,
    EmergencyRequestRepository>();

builder.Services.AddScoped<
    IEmergencyResponseRepository,
    EmergencyResponseRepository>();

builder.Services.AddScoped<
    INotificationRepository,
    NotificationRepository>();

builder.Services.AddScoped<
    IBloodBankSettingRepository,
    BloodBankSettingRepository>();

builder.Services.AddScoped<
    IPasswordResetTokenRepository,
    PasswordResetTokenRepository>();


// =====================================================
// SERVICES
// =====================================================

builder.Services.AddHostedService<
    BloodRequestExpiryService>();

builder.Services.AddScoped<
    IEmergencyResponseService,
    EmergencyResponseService>();

builder.Services.AddScoped<
    IBloodBankSettingService,
    BloodBankSettingService>();

builder.Services.AddScoped<
    INotificationService,
    NotificationService>();

builder.Services.AddScoped<
    ICommunityNotificationService,
    CommunityNotificationService>();

builder.Services.AddScoped<
    IBloodRequestService,
    BloodRequestService>();

builder.Services.AddScoped<
    IJwtService,
    JwtService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IBloodInventoryService,
    BloodInventoryService>();

builder.Services.AddScoped<
    IBloodBatchService,
    BloodBatchService>();

builder.Services.AddScoped<
    IUserService,
    UserService>();

builder.Services.AddScoped<IEmergencyService, EmergencyService>();


// =====================================================
// NOTIFICATION PUBLISHER
// SignalR + Expo Push Notification
// =====================================================

builder.Services.AddScoped<
    INotificationPublisher,
    SignalRNotificationPublisher>();


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


// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();


// =====================================================
// OPENAPI
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// =====================================================
// NOTE ABOUT HTTPS
// =====================================================

// HTTPS redirection is intentionally disabled
// for local React Native / Expo development.
//
// Mobile device connects using:
// http://192.168.1.4:5097
//
// Enable HTTPS when deploying to production.


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

app.MapHub<NotificationHub>(
    "/notificationHub"
);


// =====================================================
// RUN APPLICATION
// =====================================================

app.Run();