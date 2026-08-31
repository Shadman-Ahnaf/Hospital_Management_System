using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        public Patient? Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public Doctor? Doctor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [StringLength(250)]
        public string? Reason { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Scheduled";
    }
}