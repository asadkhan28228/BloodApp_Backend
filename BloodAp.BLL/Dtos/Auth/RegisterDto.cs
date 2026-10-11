using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[A-Za-z]+(?:[ '\-][A-Za-z]+)*$", ErrorMessage = "Enter a valid full name.")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, RegularExpression(@"^(?:\+92|0)3\d{9}$", ErrorMessage = "Enter a valid Pakistani mobile number without spaces.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(128, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s]).{8,128}$", ErrorMessage = "Password requires uppercase, lowercase, number and special character.")]
        public string Password { get; set; } = string.Empty;

        [Required, RegularExpression(@"^(A|B|AB|O)[+-]$", ErrorMessage = "Select a valid blood type.")]
        public string BloodType { get; set; } = string.Empty;

        [Required, RegularExpression(@"^(Male|Female)$")]
        public string? Gender { get; set; }

        [Required, StringLength(200, MinimumLength = 2)]
        public string? Location { get; set; }

        [Range(-90, 90)] public double? Latitude { get; set; }
        [Range(-180, 180)] public double? Longitude { get; set; }
    }
}
