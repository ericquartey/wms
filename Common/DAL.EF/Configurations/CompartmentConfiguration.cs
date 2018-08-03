using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class CompartmentConfiguration : IEntityTypeConfiguration<Compartment>
  {
    public void Configure(EntityTypeBuilder<Compartment> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(c => c.Id);

      builder.HasIndex(c => c.Code).IsUnique();

      builder.Property(c => c.Stock)
          .HasDefaultValue(0);
      builder.Property(c => c.ReservedForPick)
          .HasDefaultValue(0);
      builder.Property(c => c.ReservedToStore)
          .HasDefaultValue(0);
      builder.Property(c => c.CreationDate)
          .HasDefaultValueSql("GETDATE()");

      builder.HasOne(c => c.LoadingUnit)
          .WithMany(l => l.Compartments)
          .HasForeignKey(c => c.LoadingUnitId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.CompartmentType)
          .WithMany(c => c.Compartments)
          .HasForeignKey(c => c.CompartmentTypeId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.Item)
          .WithMany(i => i.Compartments)
          .HasForeignKey(c => c.ItemId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.MaterialStatus)
          .WithMany(m => m.Compartments)
          .HasForeignKey(c => c.MaterialStatusId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.PackageType)
          .WithMany(p => p.Compartments)
          .HasForeignKey(c => c.PackageTypeId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.CompartmentStatus)
          .WithMany(c => c.Compartments)
          .HasForeignKey(c => c.CompartmentStatusId)
          .OnDelete(DeleteBehavior.ClientSetNull);
    }
  }
}
