using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class ItemListRowConfiguration : IEntityTypeConfiguration<ItemListRow>
  {
    public void Configure(EntityTypeBuilder<ItemListRow> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(i => i.Id);

      builder.HasIndex(i => i.Code).IsUnique();
      builder.Property(i => i.Code).IsRequired();
      builder.Property(i => i.CreationDate)
          .HasDefaultValueSql("GETDATE()");

      builder.HasOne(i => i.ItemList)
          .WithMany(i => i.ItemListRows)
          .HasForeignKey(i => i.ItemListId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(i => i.Item)
          .WithMany(i => i.ItemListRows)
          .HasForeignKey(i => i.ItemId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(i => i.MaterialStatus)
          .WithMany(m => m.ItemListRows)
          .HasForeignKey(i => i.MaterialStatusId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(i => i.PackageType)
          .WithMany(p => p.ItemListRows)
          .HasForeignKey(i => i.PackageTypeId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(i => i.ItemListRowStatus)
          .WithMany(i => i.ItemListRows)
          .HasForeignKey(i => i.ItemListRowStatusId)
          .OnDelete(DeleteBehavior.ClientSetNull);
    }
  }
}
