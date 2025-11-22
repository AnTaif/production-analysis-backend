using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class DepartmentDboConfiguration : IEntityTypeConfiguration<DepartmentDbo>
{
    public void Configure(EntityTypeBuilder<DepartmentDbo> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(e => e.Id);
        
        builder.HasOne<EnterpriseDbo>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId);
    }
}