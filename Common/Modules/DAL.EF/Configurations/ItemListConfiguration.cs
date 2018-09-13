using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
    public class ItemListConfiguration : IEntityTypeConfiguration<ItemList>
    {
        public void Configure(EntityTypeBuilder<ItemList> builder)
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
            builder.Property(i => i.Priority)
                .HasDefaultValue(1);

            builder.HasOne(i => i.ItemListType)
                .WithMany(i => i.ItemLists)
                .HasForeignKey(i => i.ItemListTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(i => i.ItemListStatus)
                .WithMany(i => i.ItemLists)
                .HasForeignKey(i => i.ItemListStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(i => i.Area)
                .WithMany(a => a.ItemLists)
                .HasForeignKey(i => i.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
