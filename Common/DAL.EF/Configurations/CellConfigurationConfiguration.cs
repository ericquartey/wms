using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
    public class CellConfigurationConfiguration : IEntityTypeConfiguration<Models.CellConfiguration>
    {
        public void Configure(EntityTypeBuilder<Models.CellConfiguration> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Description).IsRequired();
        }
    }
}
