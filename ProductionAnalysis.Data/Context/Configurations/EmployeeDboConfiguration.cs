using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class EmployeeDboConfiguration : IEntityTypeConfiguration<EmployeeDbo>
{
    public void Configure(EntityTypeBuilder<EmployeeDbo> builder)
    {
        builder.ToTable("employees");

        builder.HasKey(e => e.Id);

        builder.HasOne<DepartmentDbo>()
            .WithMany()
            .HasForeignKey(e => e.DepartmentId);

        builder.HasOne<UserDbo>(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);
    }
}