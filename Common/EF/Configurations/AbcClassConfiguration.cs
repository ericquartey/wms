using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class AbcClassConfiguration : IEntityTypeConfiguration<AbcClass>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<AbcClass> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id).HasColumnType("char(1)");
            builder.Property(a => a.Description).IsRequired();
        }

        #endregion
    }
}
