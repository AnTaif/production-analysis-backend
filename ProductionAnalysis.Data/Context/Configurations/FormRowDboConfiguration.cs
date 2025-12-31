using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Context.Configurations;

public class FormRowDboConfiguration : IEntityTypeConfiguration<FormRowDbo>
{
    public void Configure(EntityTypeBuilder<FormRowDbo> builder)
    {
        builder.ToTable("form_rows");

        builder.HasKey(x => new { x.FormId, x.Order });

        builder.HasOne(x => x.Form)
            .WithMany(x => x.FormRows)
            .HasForeignKey(x => x.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FormId);
    }
}