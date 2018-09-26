using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CellWeightClassConfiguration : IEntityTypeConfiguration<CellWeightClass>
    {
        public void Configure(EntityTypeBuilder<CellWeightClass> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Description).IsRequired();
            builder.Property(c => c.MinWeight).IsRequired();
            builder.Property(c => c.MaxWeight).IsRequired();
        }
    }
}
