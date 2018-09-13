using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class DefaultLoadingUnitConfiguration : IEntityTypeConfiguration<DefaultLoadingUnit>
    {
        public void Configure(EntityTypeBuilder<DefaultLoadingUnit> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(d => d.Id);

            builder.HasOne(d => d.LoadingUnitType)
                .WithMany(l => l.DefaultLoadingUnits)
                .HasForeignKey(d => d.LoadingUnitTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
