using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
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
