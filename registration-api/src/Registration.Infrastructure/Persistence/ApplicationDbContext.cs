namespace Registration.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using Registration.Domain.Entities;

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<VehicleRegistration> VehicleRegistrations => Set<VehicleRegistration>();
        public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
        public DbSet<UploadTaskStatus> UploadTaskStatuses { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleRegistration>(builder =>
            {
                builder.OwnsOne(v => v.Duration, duration =>
                {
                    duration.Property(d => d.Value)
                        .HasColumnName("RegistrationDuration")
                        .IsRequired();
                });
            });
        }
    }
}