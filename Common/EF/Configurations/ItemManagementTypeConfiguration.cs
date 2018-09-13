using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class ItemManagementTypeConfiguration : IEntityTypeConfiguration<ItemManagementType>
    {
        public void Configure(EntityTypeBuilder<ItemManagementType> builder)
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
