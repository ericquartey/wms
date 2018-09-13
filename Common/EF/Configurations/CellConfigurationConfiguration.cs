using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CellConfigurationConfiguration : IEntityTypeConfiguration<Common.Models.CellConfiguration>
    {
        public void Configure(EntityTypeBuilder<Common.Models.CellConfiguration> builder)
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
