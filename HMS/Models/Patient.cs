using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(250)]
        [Display(Name = "Address")]
        public string? Address { get; set; }
    }
}