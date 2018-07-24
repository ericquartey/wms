using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Ferretto.Common.DAL.EF.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(i => i.Id);

            builder.HasIndex(i => i.Code).IsUnique();

            builder.Property(i => i.Code).IsRequired();
            builder.Property(i => i.Class)
                .HasColumnType("char(1)");
            builder.Property(i => i.Image)
                .HasColumnType("image");
            builder.Property(i => i.Note)
                .HasColumnType("text");
            builder.Property(i => i.CreationDate)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(i => i.MeasureUnit)
                .WithMany(m => m.Items)
                .HasForeignKey(i => i.MeasureUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(i => i.ItemManagementType)
                .WithMany(i => i.Items)
                .HasForeignKey(i => i.ItemManagementTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
