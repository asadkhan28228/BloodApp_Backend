using BloodDonationAPI.API.Hubs;
using BloodDonationAPI.API.Services;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.BLL.Services;
using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
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

                // Important for [Authorize(Roles = "admin")]
                RoleClaimType =
                    System.Security.Claims.ClaimTypes.Role,

                NameClaimType =
                    System.Security.Claims.ClaimTypes.NameIdentifier,

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

builder.Services.AddScoped<
    IBloodRequestResponseRepository,
    BloodRequestResponseRepository>();


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

builder.Services.AddScoped<
    IEmergencyService,
    EmergencyService>();


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
// DEFAULT ADMIN ACCOUNT
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    var configuration =
        scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

    var adminEmail =
        configuration["AdminAccount:Email"]?
            .Trim()
            .ToLower();

    var adminPassword =
        configuration["AdminAccount:Password"];

    // Only try to create admin when credentials
    // are present in configuration.
    if (!string.IsNullOrWhiteSpace(adminEmail) &&
        !string.IsNullOrWhiteSpace(adminPassword))
    {
        var existingUser =
            await dbContext.Users
                .FirstOrDefaultAsync(
                    user =>
                        user.Email.ToLower() ==
                        adminEmail
                );

        // =============================================
        // CREATE ADMIN IF EMAIL DOES NOT EXIST
        // =============================================

        if (existingUser == null)
        {
            var admin = new User
            {
                Id = Guid.NewGuid(),

                FullName = "BloodCare Admin",

                Email = adminEmail,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        adminPassword
                    ),

                PhoneNumber = "",

                // Admin is not a donor
                BloodType = "N/A",

                Gender = null,

                Location = null,

                Latitude = null,

                Longitude = null,

                Role = "admin",

                IsActive = true,

                AvailableToDonate = false,

                DonationsCount = 0,

                AvatarUrl = null,

                PushToken = null,

                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(admin);

            await dbContext.SaveChangesAsync();

            Console.WriteLine(
                "======================================"
            );

            Console.WriteLine(
                $"ADMIN CREATED: {adminEmail}"
            );

            Console.WriteLine(
                "======================================"
            );
        }

        // =============================================
        // EMAIL EXISTS AND IS ALREADY ADMIN
        // =============================================

        else if (
            string.Equals(
                existingUser.Role,
                "admin",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            Console.WriteLine(
                $"Admin account already exists: {adminEmail}"
            );
        }

        // =============================================
        // EMAIL EXISTS AS NORMAL USER
        // Do NOT silently promote a normal user.
        // =============================================

        else
        {
            Console.WriteLine(
                $"WARNING: Admin email {adminEmail} already belongs to a normal user."
            );

            Console.WriteLine(
                "Admin role was NOT assigned automatically."
            );
        }
    }
    else
    {
        Console.WriteLine(
            "AdminAccount configuration not found. Admin seeding skipped."
        );
    }
}


// =====================================================
// RUN APPLICATION
// =====================================================

app.Run();