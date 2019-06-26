using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CellSizeClassConfiguration : IEntityTypeConfiguration<CellSizeClass>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<CellSizeClass> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Description)
                .IsRequired();

            builder.Property(c => c.Depth)
                .IsRequired();

            builder.Property(c => c.Width)
                .IsRequired();
        }

        #endregion
    }
}
