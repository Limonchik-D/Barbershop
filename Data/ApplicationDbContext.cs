using Barbershop.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Barber> Barbers { get; set; }
    public DbSet<BarberService> BarberServices { get; set; }
    public DbSet<WorkDay> WorkDays { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Переименовываем таблицы Identity
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("Roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");

        // BarberService: составной PK (Many-to-Many)
        builder.Entity<BarberService>()
            .HasKey(bs => new { bs.BarberId, bs.ServiceId });

        builder.Entity<BarberService>()
            .HasOne(bs => bs.Barber)
            .WithMany(b => b.BarberServices)
            .HasForeignKey(bs => bs.BarberId);

        builder.Entity<BarberService>()
            .HasOne(bs => bs.Service)
            .WithMany(s => s.BarberServices)
            .HasForeignKey(bs => bs.ServiceId);

        // Appointment -> Client (нельзя каскадно удалить пользователя с записями)
        builder.Entity<Appointment>()
            .HasOne(a => a.Client)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Review -> Client
        builder.Entity<Review>()
            .HasOne(r => r.Client)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Review -> Barber
        builder.Entity<Review>()
            .HasOne(r => r.Barber)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BarberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Review -> Appointment (один к одному)
        builder.Entity<Review>()
            .HasOne(r => r.Appointment)
            .WithOne(a => a.Review)
            .HasForeignKey<Review>(r => r.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Payment -> Appointment (один к одному)
        builder.Entity<Payment>()
            .HasOne(p => p.Appointment)
            .WithOne(a => a.Payment)
            .HasForeignKey<Payment>(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Точность decimal
        builder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(10, 2);

        builder.Entity<Appointment>()
            .Property(a => a.TotalPrice)
            .HasPrecision(10, 2);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        // Индексы
        builder.Entity<Appointment>()
            .HasIndex(a => new { a.BarberId, a.AppointmentDate, a.StartTime });

        builder.Entity<Appointment>()
            .HasIndex(a => new { a.ClientId, a.AppointmentDate });

        builder.Entity<Schedule>()
            .HasIndex(s => new { s.BarberId, s.Date })
            .IsUnique();

        builder.Entity<WorkDay>()
            .HasIndex(w => new { w.BarberId, w.DayOfWeek })
            .IsUnique();
    }
}
