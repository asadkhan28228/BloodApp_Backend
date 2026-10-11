using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BloodDonationAPI.DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/emergency-insights")]
    public class EmergencyInsightsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EmergencyInsightsController(AppDbContext db) => _db = db;

        // Only the emergency reporter and administrators can access donor locations.
        private bool IsAdmin() => User.IsInRole("admin") || User.IsInRole("bloodBankAdmin");
        private Guid? CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }

        [HttpGet("{emergencyId:guid}/nearby-donors")]
        public async Task<IActionResult> NearbyDonors(Guid emergencyId, [FromQuery] double radiusKm = 20, [FromQuery] int limit = 30)
        {
            var current = CurrentUserId();
            if (current == null) return Unauthorized();
            if (!double.IsFinite(radiusKm) || radiusKm <= 0 || radiusKm > 200 || limit < 1 || limit > 100)
                return BadRequest(new { message = "radiusKm must be 0-200 and limit must be 1-100." });

            var emergency = await _db.EmergencyRequests.AsNoTracking().FirstOrDefaultAsync(e => e.Id == emergencyId);
            if (emergency == null) return NotFound();
            if (emergency.ReporterUid != current.Value && !IsAdmin()) return Forbid();
            if (!ValidCoordinate(emergency.Latitude, emergency.Longitude))
                return BadRequest(new { message = "Emergency GPS coordinates are required." });

            // Exact-group targeting only. Medical blood compatibility must be validated separately.
            var bloodGroup = emergency.BloodType.Trim().ToUpper();
            var donors = await _db.Users.AsNoTracking()
                .Where(u => u.IsActive && u.AvailableToDonate && u.Role == "user" &&
                            u.Latitude.HasValue && u.Longitude.HasValue &&
                            (bloodGroup == "ANY" || u.BloodType.ToUpper() == bloodGroup))
                .Select(u => new { u.Id, u.FullName, u.BloodType, u.Location, u.Latitude, u.Longitude })
                .ToListAsync();

            var nearby = donors
                .Where(d => ValidCoordinate(d.Latitude, d.Longitude))
                .Select(d => new
                {
                    d.Id,
                    d.FullName,
                    d.BloodType,
                    d.Location,
                    DistanceKm = Math.Round(DistanceKm(emergency.Latitude!.Value, emergency.Longitude!.Value,
                        d.Latitude!.Value, d.Longitude!.Value), 2)
                })
                .Where(d => d.DistanceKm <= radiusKm)
                .OrderBy(d => d.DistanceKm).Take(limit).ToList();

            return Ok(new
            {
                emergencyId,
                radiusKm,
                total = nearby.Count,
                donors = nearby,
                note = "Approximate straight-line distance. Blood eligibility and compatibility require clinical verification."
            });
        }

        [HttpGet("responses/{responseId:guid}/eta")]
        public async Task<IActionResult> ResponseEta(Guid responseId, [FromQuery] double assumedSpeedKmh = 25)
        {
            var current = CurrentUserId();
            if (current == null) return Unauthorized();
            if (!double.IsFinite(assumedSpeedKmh) || assumedSpeedKmh < 5 || assumedSpeedKmh > 120)
                return BadRequest(new { message = "assumedSpeedKmh must be between 5 and 120." });

            var response = await _db.EmergencyResponses.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == responseId);
            if (response == null) return NotFound();
            var emergency = await _db.EmergencyRequests.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == response.EmergencyRequestId);
            if (emergency == null) return NotFound();
            if (response.DonorId != current.Value && emergency.ReporterUid != current.Value && !IsAdmin()) return Forbid();
            if (!ValidCoordinate(response.Latitude, response.Longitude) ||
                !ValidCoordinate(emergency.Latitude, emergency.Longitude))
                return BadRequest(new { message = "Donor and emergency GPS coordinates are required." });

            var distance = DistanceKm(response.Latitude!.Value, response.Longitude!.Value,
                emergency.Latitude!.Value, emergency.Longitude!.Value);
            // Deliberately not a road-route or traffic-aware ETA.
            var minutes = Math.Ceiling(distance / assumedSpeedKmh * 60);
            return Ok(new
            {
                responseId,
                emergencyId = emergency.Id,
                status = response.Status,
                straightLineDistanceKm = Math.Round(distance, 2),
                assumedSpeedKmh,
                estimatedMinutes = (int)minutes,
                estimatedArrivalUtc = DateTime.UtcNow.AddMinutes(minutes),
                locationUpdatedAtUtc = response.UpdatedAt,
                note = "Rough estimate based on straight-line distance and assumed speed; not a navigation ETA."
            });
        }

        [HttpGet("{emergencyId:guid}/command-center")]
        public async Task<IActionResult> CommandCenter(Guid emergencyId)
        {
            var current = CurrentUserId();
            if (current == null) return Unauthorized();
            var emergency = await _db.EmergencyRequests.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == emergencyId);
            if (emergency == null) return NotFound();
            if (emergency.ReporterUid != current.Value && !IsAdmin()) return Forbid();

            var responses = await _db.EmergencyResponses.AsNoTracking()
                .Where(r => r.EmergencyRequestId == emergencyId)
                .Select(r => new {
                    r.Id,
                    r.DonorId,
                    r.DonorName,
                    r.Status,
                    r.RespondedAt,
                    r.OnTheWayAt,
                    r.ArrivedAt,
                    r.CompletedAt,
                    r.Latitude,
                    r.Longitude,
                    r.UpdatedAt
                })
                .ToListAsync();
            return Ok(new
            {
                emergency = new
                {
                    emergency.Id,
                    emergency.Status,
                    emergency.Priority,
                    emergency.BloodType,
                    emergency.UnitsNeeded,
                    emergency.ReservedUnits,
                    emergency.Location,
                    emergency.CreatedAt,
                    emergency.ExpireAt
                },
                totalResponses = responses.Count,
                onTheWay = responses.Count(r => string.Equals(r.Status, "On The Way", StringComparison.OrdinalIgnoreCase)),
                arrived = responses.Count(r => string.Equals(r.Status, "Arrived", StringComparison.OrdinalIgnoreCase)),
                completed = responses.Count(r => string.Equals(r.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                responses
            });
        }

        private static bool ValidCoordinate(double? lat, double? lon) =>
            lat.HasValue && lon.HasValue && double.IsFinite(lat.Value) && double.IsFinite(lon.Value) &&
            lat.Value >= -90 && lat.Value <= 90 && lon.Value >= -180 && lon.Value <= 180;

        private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0088;
            static double Radians(double degrees) => degrees * Math.PI / 180.0;
            var deltaLat = Radians(lat2 - lat1);
            var deltaLon = Radians(lon2 - lon1);
            var a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                    Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) *
                    Math.Pow(Math.Sin(deltaLon / 2), 2);
            return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(Math.Max(0, 1 - a)));
        }
    }
}
