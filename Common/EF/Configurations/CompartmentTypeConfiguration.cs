using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CompartmentTypeConfiguration : IEntityTypeConfiguration<CompartmentType>
    {
        public void Configure(EntityTypeBuilder<CompartmentType> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Description).IsRequired();
        }
    }
}
