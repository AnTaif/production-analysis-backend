using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Context.Configurations;

public class FormDataDboConfiguration : IEntityTypeConfiguration<FormDataDbo>
{
    public void Configure(EntityTypeBuilder<FormDataDbo> builder)
    {
        builder.ToTable("form_data");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(x => x.Value).HasColumnType("jsonb");

        builder.HasOne(x => x.Form)
            .WithMany()
            .HasForeignKey(x => x.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Indicator)
            .WithMany()
            .HasForeignKey(x => x.IndicatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserDbo>()
            .WithMany()
            .HasForeignKey(x => x.LastModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Составной уникальный индекс для предотвращения дублирования ячеек
        builder.HasIndex(x => new { x.FormId, x.RowOrder, x.IndicatorId })
            .IsUnique();

        // Индекс для быстрого поиска данных формы
        builder.HasIndex(x => x.FormId);

        // Индекс для поиска по индикатору
        builder.HasIndex(x => x.IndicatorId);
    }
}