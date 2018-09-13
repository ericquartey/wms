using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CellHeightClassConfiguration : IEntityTypeConfiguration<CellHeightClass>
    {
        public void Configure(EntityTypeBuilder<CellHeightClass> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Description).IsRequired();
            builder.Property(c => c.MinHeight).IsRequired();
            builder.Property(c => c.MaxHeight).IsRequired();
        }
    }
}
