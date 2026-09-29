using CitizenPortal.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CitizenPortal.Api.Data.Configurations;

public class CitizenConfiguration : IEntityTypeConfiguration<Citizen>
{
    public void Configure(EntityTypeBuilder<Citizen> e)
    {
        e.ToTable("MR_DEPT_Citizens");
        e.HasIndex(c => c.MobileNumber).IsUnique();
        e.HasIndex(c => c.Email).IsUnique();
        e.HasIndex(c => c.AadhaarNumber).IsUnique();
        e.Property(c => c.Id).ValueGeneratedNever();
    }
}
