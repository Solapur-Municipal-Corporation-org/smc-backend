using Microsoft.EntityFrameworkCore;
using SMC.Master.Domain.Entities;

namespace SMC.Master.Infrastructure.Data;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Citizen> Citizens => Set<Citizen>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<DepartmentService> DepartmentServices => Set<DepartmentService>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationStatus> ApplicationStatuses => Set<ApplicationStatus>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DepartmentService>()
            .HasKey(ds => new { ds.DepartmentId, ds.ServiceId });

        modelBuilder.Entity<Citizen>()
            .HasIndex(c => c.AadhaarNumber)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MasterDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
