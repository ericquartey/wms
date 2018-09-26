using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class ItemCompartmentTypeConfiguration : IEntityTypeConfiguration<ItemCompartmentType>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ItemCompartmentType> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(i => new { i.CompartmentTypeId, i.ItemId });

            builder.HasOne(i => i.Item)
                .WithMany(i => i.ItemsCompartmentTypes)
                .HasForeignKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(i => i.CompartmentType)
                .WithMany(c => c.ItemsCompartmentTypes)
                .HasForeignKey(i => i.CompartmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion Methods
    }
}
