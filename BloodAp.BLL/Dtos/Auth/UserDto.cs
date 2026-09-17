using System;
using System.Collections.Generic;
using System.Text;

namespace BloodDonationAPI.BLL.Dtos.Auth
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? Location { get; set; }
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool AvailableToDonate { get; set; }
    }
}