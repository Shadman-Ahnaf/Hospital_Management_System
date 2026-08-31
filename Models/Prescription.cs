using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }

        [Required]
        public int PatientId { get; set; }

        public Patient? Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public Doctor? Doctor { get; set; }

        [Required]
        public int MedicalRecordId { get; set; }

        public MedicalRecord? MedicalRecord { get; set; }

        [Required]
        [StringLength(150)]
        public string MedicineName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Duration { get; set; } = string.Empty;

        public DateTime PrescriptionDate { get; set; } = DateTime.Now;
    }
}