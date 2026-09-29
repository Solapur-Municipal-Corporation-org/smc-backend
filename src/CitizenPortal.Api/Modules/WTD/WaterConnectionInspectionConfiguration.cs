using CitizenPortal.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CitizenPortal.Api.Modules.Wtd;

public class WaterConnectionInspectionConfiguration : IEntityTypeConfiguration<WaterConnectionInspection>
{
    public void Configure(EntityTypeBuilder<WaterConnectionInspection> e)
    {
        // Table name prefixed with the department code (WTD) so it can never
        // collide with another department's module table in the shared DB.
        e.ToTable("WTD_ConnectionInspections");

        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasOne(x => x.Application)
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
