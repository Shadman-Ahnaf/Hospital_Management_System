using HospitalManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Data
{
    public static class HmsDataSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var context =
                serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ==========================================
            // 1. SEED DEPARTMENTS
            // ==========================================

            if (!await context.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new Department
                    {
                        DepartmentName = "Cardiology",
                        Description = "Heart and cardiovascular care.",
                        Location = "Block A - Floor 2"
                    },

                    new Department
                    {
                        DepartmentName = "Neurology",
                        Description = "Diagnosis and treatment of neurological disorders.",
                        Location = "Block A - Floor 3"
                    },

                    new Department
                    {
                        DepartmentName = "Orthopedics",
                        Description = "Bone, joint and musculoskeletal care.",
                        Location = "Block B - Floor 2"
                    },

                    new Department
                    {
                        DepartmentName = "General Medicine",
                        Description = "General medical consultation and treatment.",
                        Location = "Block B - Floor 1"
                    }
                };

                context.Departments.AddRange(departments);
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 2. SEED DOCTORS
            // ==========================================

            if (!await context.Doctors.AnyAsync())
            {
                var cardiology =
                    await context.Departments
                        .FirstAsync(d => d.DepartmentName == "Cardiology");

                var neurology =
                    await context.Departments
                        .FirstAsync(d => d.DepartmentName == "Neurology");

                var orthopedics =
                    await context.Departments
                        .FirstAsync(d => d.DepartmentName == "Orthopedics");

                var generalMedicine =
                    await context.Departments
                        .FirstAsync(d => d.DepartmentName == "General Medicine");

                var doctors = new List<Doctor>
                {
                    new Doctor
                    {
                        FullName = "Dr. Demo Doctor",
                        Specialization = "Cardiologist",
                        Phone = "01710000001",
                        Email = "doctor@hms.com",
                        DepartmentId = cardiology.DepartmentId,
                        IsAvailable = true
                    },

                    new Doctor
                    {
                        FullName = "Dr. Sarah Ahmed",
                        Specialization = "Neurologist",
                        Phone = "01710000002",
                        Email = "sarah.ahmed@hms.com",
                        DepartmentId = neurology.DepartmentId,
                        IsAvailable = true
                    },

                    new Doctor
                    {
                        FullName = "Dr. Rahim Khan",
                        Specialization = "Orthopedic Surgeon",
                        Phone = "01710000003",
                        Email = "rahim.khan@hms.com",
                        DepartmentId = orthopedics.DepartmentId,
                        IsAvailable = true
                    },

                    new Doctor
                    {
                        FullName = "Dr. Nusrat Jahan",
                        Specialization = "General Physician",
                        Phone = "01710000004",
                        Email = "nusrat.jahan@hms.com",
                        DepartmentId = generalMedicine.DepartmentId,
                        IsAvailable = true
                    }
                };

                context.Doctors.AddRange(doctors);
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 3. SEED PATIENTS
            // ==========================================

            if (!await context.Patients.AnyAsync())
            {
                var patients = new List<Patient>
                {
                    new Patient
                    {
                        FullName = "Demo Patient",
                        DateOfBirth = new DateTime(2000, 5, 15),
                        Gender = "Female",
                        Phone = "01810000001",
                        Email = "patient@hms.com",
                        Address = "Dhaka, Bangladesh",
                        EmergencyContact = "01810000011",
                        BloodGroup = "O+"
                    },

                    new Patient
                    {
                        FullName = "Arif Hasan",
                        DateOfBirth = new DateTime(1995, 8, 20),
                        Gender = "Male",
                        Phone = "01810000002",
                        Email = "arif.hasan@example.com",
                        Address = "Mirpur, Dhaka",
                        EmergencyContact = "01810000012",
                        BloodGroup = "A+"
                    },

                    new Patient
                    {
                        FullName = "Nusrat Akter",
                        DateOfBirth = new DateTime(1998, 2, 10),
                        Gender = "Female",
                        Phone = "01810000003",
                        Email = "nusrat.akter@example.com",
                        Address = "Uttara, Dhaka",
                        EmergencyContact = "01810000013",
                        BloodGroup = "B+"
                    },

                    new Patient
                    {
                        FullName = "Tanvir Rahman",
                        DateOfBirth = new DateTime(1988, 11, 5),
                        Gender = "Male",
                        Phone = "01810000004",
                        Email = "tanvir.rahman@example.com",
                        Address = "Dhanmondi, Dhaka",
                        EmergencyContact = "01810000014",
                        BloodGroup = "AB+"
                    }
                };

                context.Patients.AddRange(patients);
                await context.SaveChangesAsync();
            }
        }
    }
}