using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class AdditionalOperationDboConfiguration : IEntityTypeConfiguration<AdditionalOperationDbo>
{
    public void Configure(EntityTypeBuilder<AdditionalOperationDbo> builder)
    {
        builder.ToTable("additional_operations");
        
        builder.HasKey(x => x.Id);
    }
}