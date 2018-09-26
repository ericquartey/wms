using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class ItemListTypeConfiguration : IEntityTypeConfiguration<ItemListType>
    {
        public void Configure(EntityTypeBuilder<ItemListType> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Description).IsRequired();
        }
    }
}
