using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
    public class ItemAreaConfiguration : IEntityTypeConfiguration<ItemArea>
    {
        public void Configure(EntityTypeBuilder<ItemArea> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(i => new {i.ItemId, i.AreaId});

            builder.HasOne(i => i.Item)
                .WithMany(i => i.ItemAreas)
                .HasForeignKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(i => i.Area)
                .WithMany(a => a.AreaItems)
                .HasForeignKey(i => i.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
