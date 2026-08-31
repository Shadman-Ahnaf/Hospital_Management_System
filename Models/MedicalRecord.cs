using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class MedicalRecord
    {
        public int MedicalRecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        public Patient? Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public Doctor? Doctor { get; set; }

        public int AppointmentId { get; set; }

        public Appointment? Appointment { get; set; }

        [Required]
        [StringLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? TreatmentDetails { get; set; }

        public DateTime RecordDate { get; set; } = DateTime.Now;
    }
}