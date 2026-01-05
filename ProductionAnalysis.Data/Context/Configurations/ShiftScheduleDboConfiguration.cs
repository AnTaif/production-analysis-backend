using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class ShiftScheduleDboConfiguration : IEntityTypeConfiguration<ShiftScheduleDbo>
{
    public void Configure(EntityTypeBuilder<ShiftScheduleDbo> builder)
    {
        builder.ToTable("shift_schedules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.HasOne(x => x.Shift)
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AuxiliaryOperation)
            .WithMany()
            .HasForeignKey(x => x.AuxiliaryOperationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ShiftId);
    }
}