using System;
using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class ItemListRowConfiguration : IEntityTypeConfiguration<ItemListRow>
    {
        #region Methods

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
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.LastModificationDate)
                .HasDefaultValueSql("GETUTCDATE()");

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

            builder.Property(i => i.ItemListRowStatus).IsRequired()
               .HasColumnType("char(1)")
               .HasConversion(x => (char)x, x => (ItemListRowStatus)Enum.ToObject(typeof(ItemListRowStatus), x));
        }

        #endregion Methods
    }
}
