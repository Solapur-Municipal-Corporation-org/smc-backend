using CitizenPortal.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CitizenPortal.Api.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Models.Department>
{
    public void Configure(EntityTypeBuilder<Models.Department> e)
    {
        // StaffDepartment is the writable EF entity for the existing table and has
        // its real int DepartmentId key. Keep this Citizen Portal shape as a query
        // projection over that same object; mapping two incompatible entity keys as
        // table entities makes EF reject the model (and must never create Departments).
        e.ToView("MR_DEPT_Departments", "dbo");
        e.HasKey(d => d.Id);
        e.Property(d => d.Id)
            .HasColumnName("DepartmentId")
            .HasConversion(new ValueConverter<Guid, int>(
                id => BitConverter.ToInt32(id.ToByteArray(), 0),
                id => GuidFromInt(id)));
        e.Property(d => d.Code).HasColumnName("DepartmentCode");
        e.Property(d => d.Name).HasColumnName("DepartmentName");
        e.Property(d => d.NameMarathi).HasColumnName("DepartmentNameMarathi");
        e.Property(d => d.IconName).HasColumnName("IconName");
        e.Property(d => d.DisplayOrder).HasColumnName("DisplayOrder");
        e.Property(d => d.IsActive).HasColumnName("IsActive");
        e.Property(d => d.DepartmentDescription).HasColumnName("DepartmentDescription");
        e.Property(d => d.Id).ValueGeneratedNever();
    }

    private static Guid GuidFromInt(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
