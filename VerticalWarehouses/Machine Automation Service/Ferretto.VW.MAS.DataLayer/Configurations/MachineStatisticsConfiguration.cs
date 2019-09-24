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
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(a => a.Id);

            builder.Ignore(s => s.AutomaticTimePercentage);
            builder.Ignore(s => s.AreaFillPercentage);
            builder.Ignore(s => s.UsageTimePercentage);

            builder.HasData(
                new MachineStatistics
                {
                    Id = 1,
                    TotalAutomaticTime = System.TimeSpan.FromDays(130),
                    TotalBeltCycles = 12352,
                    TotalMovedTrays = 534,
                    TotalMovedTraysInBay1 = 123,
                    TotalMovedTraysInBay2 = 456,
                    TotalMovedTraysInBay3 = 789,
                    TotalPowerOnTime = System.TimeSpan.FromDays(190),
                    TotalShutter1Cycles = 321,
                    TotalShutter2Cycles = 654,
                    TotalShutter3Cycles = 987,
                    TotalVerticalAxisCycles = 5232,
                    TotalVerticalAxisKilometers = 34,
                    AreaFillPercentage = 87,
                    TotalMissionTime = System.TimeSpan.FromDays(30),
                    WeightCapacityPercentage = 60,
                });
        }

        #endregion
    }
}
