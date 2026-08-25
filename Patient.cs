using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Patient
    {
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? EmergencyContact { get; set; }

        [StringLength(10)]
        public string? BloodGroup { get; set; }
    }
}
