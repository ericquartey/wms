using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class MachineStatisticsConfiguration : IEntityTypeConfiguration<MachineStatistics>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<MachineStatistics> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(a => a.Id);

            builder.Ignore(s => s.AutomaticTimePercentage);
            builder.Ignore(s => s.AreaFillPercentage);
            builder.Ignore(s => s.UsageTimePercentage);

            builder.HasData(new MachineStatistics { Id = -1 });
        }

        #endregion
    }
}
