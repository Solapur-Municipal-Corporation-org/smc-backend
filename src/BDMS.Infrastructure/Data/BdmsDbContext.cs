using BDMS.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Infrastructure.Data;

public class BdmsDbContext : DbContext
{
    public BdmsDbContext(DbContextOptions<BdmsDbContext> options) : base(options) { }

    public DbSet<BirthApplication> BirthApplications => Set<BirthApplication>();
    public DbSet<TempBirthApplication> TempBirthApplications => Set<TempBirthApplication>();
    public DbSet<DeathApplication> DeathApplications => Set<DeathApplication>();
    public DbSet<TempDeathApplication> TempDeathApplications => Set<TempDeathApplication>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<DocumentUpload> DocumentUploads => Set<DocumentUpload>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BirthApplication>(e =>
        {
            e.ToTable("TR_B&D_BirthApplications");
            e.HasIndex(x => x.ApplicationNumber).IsUnique();
            e.Property(x => x.PaidAmount).HasColumnType("decimal(10,2)");
            e.Property(x => x.AmountToPay).HasColumnType("decimal(10,2)");
            e.Property(x => x.DakhalaFee).HasColumnType("decimal(10,2)");
            e.Property(x => x.Penalty).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<TempBirthApplication>(e =>
        {
            e.HasIndex(x => x.TempApplicationNumber).IsUnique();
            e.Property(x => x.PaidAmount).HasColumnType("decimal(10,2)");
            e.Property(x => x.AmountToPay).HasColumnType("decimal(10,2)");
            e.Property(x => x.DakhalaFee).HasColumnType("decimal(10,2)");
            e.Property(x => x.Penalty).HasColumnType("decimal(10,2)");
        });
        modelBuilder.Entity<DeathApplication>(e =>
        {
            e.ToTable("TR_B&D_DeathApplications");
            e.HasIndex(x => x.ApplicationNumber).IsUnique();
        });
        modelBuilder.Entity<TempDeathApplication>(e => e.HasIndex(x => x.TempApplicationNumberValue).IsUnique());

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.UserName).IsUnique();
        });

        modelBuilder.Entity<PaymentRecord>(e =>
        {
            e.Property(x => x.Amount).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<DocumentUpload>(e =>
        {
            e.HasIndex(x => new { x.TempApplicationNumber, x.DocumentKey }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
