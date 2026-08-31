using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public bool MustChangePassword { get; set; } = false;

        public string? ActivationCode { get; set; }

        public DateTime? ActivationCodeExpiry { get; set; }

        public Doctor? Doctor { get; set; }

        public Patient? Patient { get; set; }

        public Receptionist? Receptionist { get; set; }
    }
}