using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Errors;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IErrorStatisticsDataLayer
    {
        #region Methods

        public ErrorStatisticsSummary GetErrorStatistics()
        {
            var totalErrors = this.primaryDataContext.ErrorStatistics.Sum(s => s.TotalErrors);
            var summary = new ErrorStatisticsSummary
            {
                TotalErrors = totalErrors,
                Errors = this.primaryDataContext.ErrorStatistics
                    .Select(s =>
                        new ErrorStatisticsDetail
                        {
                            Code = s.Code,
                            Description = s.Error.Description,
                            Total = s.TotalErrors,
                            RatioTotal = s.TotalErrors * 100.0 / totalErrors
                        }),
            };

            if (this.primaryDataContext.MachineStatistics.Any())
            {
                summary.TotalLoadingUnits = this.primaryDataContext.MachineStatistics.First().TotalMovedTrays;
                if (summary.TotalLoadingUnits > 0)
                {
                    summary.TotalLoadingUnitsBetweenErrors = summary.TotalLoadingUnits / totalErrors;
                }

                summary.ReliabilityPercentage = totalErrors * 100.0 / summary.TotalLoadingUnits;
            }

            return summary;
        }

        #endregion
    }
}
