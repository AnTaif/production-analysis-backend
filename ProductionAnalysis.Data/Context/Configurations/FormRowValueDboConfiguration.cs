using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Context.Configurations;

public class FormRowValueDboConfiguration : IEntityTypeConfiguration<FormRowValueDbo>
{
    public void Configure(EntityTypeBuilder<FormRowValueDbo> builder)
    {
        builder.ToTable("form_row_values");

        builder.HasKey(x => new { x.FormId, x.FormRowOrder, x.IndicatorId });

        builder.Property(x => x.Value).HasColumnType("jsonb");
        builder.Property(x => x.CumulativeValue).HasColumnType("jsonb");

        builder.HasOne(x => x.FormRow)
            .WithMany(x => x.Values)
            .HasForeignKey(x => new { x.FormId, x.FormRowOrder })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Indicator)
            .WithMany()
            .HasForeignKey(x => x.IndicatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FormId, x.FormRowOrder });
    }
}