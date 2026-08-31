using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Receptionist
    {
        public int ReceptionistId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Identity account relationship
        // Nullable because existing receptionists may not have an account yet
        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public bool IsActive { get; set; } = true;
    }
}