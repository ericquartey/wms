using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class LoadingUnitHeightClassConfiguration : IEntityTypeConfiguration<LoadingUnitHeightClass>
    {
        public void Configure(EntityTypeBuilder<LoadingUnitHeightClass> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Description).IsRequired();
            builder.Property(l => l.MinHeight).IsRequired();
            builder.Property(l => l.MaxHeight).IsRequired();
        }
    }
}
