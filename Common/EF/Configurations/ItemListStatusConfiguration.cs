using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class ItemListStatusConfiguration : IEntityTypeConfiguration<ItemListStatus>
    {
        public void Configure(EntityTypeBuilder<ItemListStatus> builder)
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
