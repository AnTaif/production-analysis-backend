using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Context.Configurations;

public class FormDboConfiguration : IEntityTypeConfiguration<FormDbo>
{
    public void Configure(EntityTypeBuilder<FormDbo> builder)
    {
        builder.ToTable("forms");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(x => x.Context).HasColumnType("jsonb");
        builder.Property(x => x.TemplateSnapshot).HasColumnType("jsonb");

        builder.HasOne<PaTypeDbo>()
            .WithMany()
            .HasForeignKey(x => x.PaTypeId);

        builder.HasOne<UserDbo>()
            .WithMany()
            .HasForeignKey(x => x.CreatorId);

        builder.HasOne<UserDbo>()
            .WithMany()
            .HasForeignKey(x => x.LastEditorId);
    }
}