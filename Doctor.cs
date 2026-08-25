using HospitalManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
