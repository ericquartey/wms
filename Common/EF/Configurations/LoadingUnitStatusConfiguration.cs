using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class LoadingUnitStatusConfiguration : IEntityTypeConfiguration<LoadingUnitStatus>
    {
        public void Configure(EntityTypeBuilder<LoadingUnitStatus> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id).HasColumnType("char(1)");
            builder.Property(l => l.Description).IsRequired();
        }
    }
}
