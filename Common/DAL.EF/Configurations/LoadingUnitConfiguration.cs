using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class LoadingUnitConfiguration : IEntityTypeConfiguration<LoadingUnit>
  {
    public void Configure(EntityTypeBuilder<LoadingUnit> builder)
    {
      builder.HasKey(l => l.Id);

      builder.HasIndex(l => l.Code).IsUnique();

      builder.Property(l => l.Code).IsRequired();
      builder.Property(l => l.ClassId).IsRequired()
          .HasColumnType("char(1)");
      builder.Property(l => l.Reference).IsRequired()
          .HasColumnType("char(1)");
      builder.Property(l => l.Note)
          .HasColumnType("text");
      builder.Property(l => l.CreationDate)
          .HasDefaultValueSql("GETDATE()");
      builder.Property(l => l.InCycleCount)
          .HasDefaultValue(0);
      builder.Property(l => l.OutCycleCount)
          .HasDefaultValue(0);
      builder.Property(l => l.OtherCycleCount)
          .HasDefaultValue(0);

      builder.HasOne(l => l.AbcClass)
        .WithMany(a => a.LoadingUnits)
        .HasForeignKey(l => l.ClassId)
        .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(l => l.Cell)
          .WithMany(c => c.LoadingUnits)
          .HasForeignKey(l => l.CellId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(l => l.CellPosition)
          .WithMany(c => c.LoadingUnits)
          .HasForeignKey(l => l.CellPositionId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(l => l.LoadingUnitType)
          .WithMany(l => l.LoadingUnits)
          .HasForeignKey(l => l.LoadingUnitTypeId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(l => l.LoadingUnitStatus)
          .WithMany(l => l.LoadingUnits)
          .HasForeignKey(l => l.LoadingUnitStatusId)
          .OnDelete(DeleteBehavior.ClientSetNull);
    }
  }
}
