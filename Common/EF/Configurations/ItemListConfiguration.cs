using System;
using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.Common.EF.Configurations
{
    public class ItemListConfiguration : IEntityTypeConfiguration<ItemList>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ItemList> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(i => i.Id);

            builder.HasIndex(i => i.Code)
                .IsUnique();

            builder.Property(i => i.Code)
                .IsRequired();

            builder.Property(i => i.CreationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.LastModificationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.Priority)
                .HasDefaultValue(1);

            builder.Property(i => i.ItemListType)
                .IsRequired()
                .HasColumnType("char(1)")
                .HasConversion(
                    enumValue => (char)enumValue,
                    charValue => (Enums.ItemListType)Enum.ToObject(typeof(Enums.ItemListType), charValue));
        }

        #endregion
    }
}
