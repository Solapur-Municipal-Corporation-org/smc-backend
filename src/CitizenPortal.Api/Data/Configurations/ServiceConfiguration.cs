using CitizenPortal.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CitizenPortal.Api.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> e)
    {
        var intGuidConverter = new ValueConverter<Guid, int>(
            id => BitConverter.ToInt32(id.ToByteArray(), 0),
            id => GuidFromInt(id));

        e.ToTable("TR_CFC_Services", "dbo");
        e.Property(s => s.Id).HasColumnName("ServiceId").HasConversion(intGuidConverter).ValueGeneratedNever();
        e.Property(s => s.DepartmentId).HasColumnName("DepartmentId").HasConversion(intGuidConverter);
        e.Property(s => s.Name).HasColumnName("ServiceName");
        e.Property(s => s.NameMarathi).HasColumnName("ServiceNameMarathi");
        e.Property(s => s.ServiceCode).HasColumnName("ServiceCode");
        e.Property(s => s.Fee).HasColumnType("decimal(10,2)");
        e.HasOne(s => s.Department).WithMany(d => d.Services).HasForeignKey(s => s.DepartmentId);
    }

    private static Guid GuidFromInt(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}

public class ServiceFieldConfiguration : IEntityTypeConfiguration<ServiceField>
{
    public void Configure(EntityTypeBuilder<ServiceField> e)
    {
        e.Property(f => f.Id).ValueGeneratedNever();
        e.HasOne(f => f.Service).WithMany(s => s.Fields).HasForeignKey(f => f.ServiceId);
    }
}

public class ServiceDocumentConfiguration : IEntityTypeConfiguration<ServiceDocument>
{
    public void Configure(EntityTypeBuilder<ServiceDocument> e)
    {
        e.Property(d => d.Id).ValueGeneratedNever();
        e.HasOne(d => d.Service).WithMany(s => s.RequiredDocuments).HasForeignKey(d => d.ServiceId);
    }
}
