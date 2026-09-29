using CitizenPortal.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CitizenPortal.Api.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> e)
    {
        e.ToTable("TR_CFC_Applications");
        e.Property(a => a.Id).ValueGeneratedNever();
        e.HasIndex(a => a.ApplicationNumber).IsUnique();
        e.HasOne(a => a.Citizen).WithMany(c => c.Applications).HasForeignKey(a => a.CitizenId);
        e.HasOne(a => a.Service).WithMany().HasForeignKey(a => a.ServiceId);
    }
}

public class ApplicationDocumentConfiguration : IEntityTypeConfiguration<ApplicationDocument>
{
    public void Configure(EntityTypeBuilder<ApplicationDocument> e)
    {
        e.Property(d => d.Id).ValueGeneratedNever();
        e.HasOne(d => d.Application).WithMany(a => a.Documents).HasForeignKey(d => d.ApplicationId);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> e)
    {
        e.Property(p => p.Id).ValueGeneratedNever();
        e.Property(p => p.Amount).HasColumnType("decimal(10,2)");
        e.HasIndex(p => p.ReceiptNumber).IsUnique();
        e.HasOne(p => p.Application).WithOne(a => a.Payment).HasForeignKey<Payment>(p => p.ApplicationId);
    }
}

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> e)
    {
        e.Property(c => c.Id).ValueGeneratedNever();
        e.HasOne(c => c.Application).WithOne(a => a.Certificate).HasForeignKey<Certificate>(c => c.ApplicationId);
    }
}
