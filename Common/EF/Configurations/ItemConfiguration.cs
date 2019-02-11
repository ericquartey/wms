using System;
using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Item> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(i => i.Id);

            builder.Property(i => i.AbcClassId).IsRequired()
              .HasColumnType("char(1)");

            builder.HasIndex(i => i.Code).IsUnique();

            builder.Property(i => i.Code).IsRequired();

            builder.Property(i => i.ManagementType).IsRequired()
                .HasColumnType("char(1)")
                .HasConversion(x => (char)x, x => (ItemManagementType)Enum.ToObject(typeof(ItemManagementType), x));

            builder.Property(i => i.Note)
                .HasColumnType("text");

            builder.Property(i => i.CreationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.LastModificationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(i => i.AbcClass)
                .WithMany(a => a.Items)
                .HasForeignKey(i => i.AbcClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(i => i.MeasureUnit)
                .WithMany(m => m.Items)
                .HasForeignKey(i => i.MeasureUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(i => i.ItemCategory)
                .WithMany(i => i.Items)
                .HasForeignKey(i => i.ItemCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
