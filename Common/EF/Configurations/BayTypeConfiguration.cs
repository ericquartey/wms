using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class BayTypeConfiguration : IEntityTypeConfiguration<BayType>
    {
        public void Configure(EntityTypeBuilder<BayType> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id).HasColumnType("char(1)");
            builder.Property(m => m.Description).IsRequired();
        }
    }
}
