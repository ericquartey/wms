using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
    public class ItemManagementTypeConfiguration : IEntityTypeConfiguration<ItemManagementType>
    {
        public void Configure(EntityTypeBuilder<ItemManagementType> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Description).IsRequired();
        }
    }
}
