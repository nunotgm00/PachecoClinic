using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Intervention> Interventions { get; set; }
        public DbSet<AppointmentRequest> AppointmentRequests { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Client>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Client>(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Doctor>()
                .HasOne(d => d.Specialty)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Animal>()
                .HasOne(a => a.Client)
                .WithMany(c => c.Animals)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DoctorSchedule>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.Schedules)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Animal)
                .WithMany(a => a.Appointments)
                .HasForeignKey(a => a.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Room)
                .WithMany(r => r.Appointments)
                .HasForeignKey(a => a.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Intervention>()
                .HasOne(i => i.Appointment)
                .WithMany(a => a.Interventions)
                .HasForeignKey(i => i.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AppointmentRequest>()
                .HasOne(ar => ar.Appointment)
                .WithMany(a => a.Requests)
                .HasForeignKey(ar => ar.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Animal>()
                .Property(a => a.Weight)
                .HasPrecision(5, 2);
        }


    }
}
