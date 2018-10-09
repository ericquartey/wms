using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class BayTypeConfiguration : IEntityTypeConfiguration<BayType>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<BayType> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id).HasColumnType("char(1)");
            builder.Property(m => m.Description).IsRequired();
        }

        #endregion Methods
    }
}
