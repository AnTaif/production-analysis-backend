using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Context.Configurations;

public class FormRowValueDboConfiguration : IEntityTypeConfiguration<FormRowValueDbo>
{
    public void Configure(EntityTypeBuilder<FormRowValueDbo> builder)
    {
        builder.ToTable("form_row_values");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(x => x.Value).HasColumnType("jsonb");

        builder.HasOne(x => x.FormRow)
            .WithMany(x => x.Values)
            .HasForeignKey(x => x.FormRowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Indicator)
            .WithMany()
            .HasForeignKey(x => x.IndicatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FormRowId);
        builder.HasIndex(x => new { x.FormRowId, x.IndicatorId })
            .IsUnique();
    }
}