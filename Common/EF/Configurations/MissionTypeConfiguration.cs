using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class MissionTypeConfiguration : IEntityTypeConfiguration<MissionType>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<MissionType> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id).HasColumnType("char(2)");
            builder.Property(m => m.Description).IsRequired();
        }

        #endregion Methods
    }
}
