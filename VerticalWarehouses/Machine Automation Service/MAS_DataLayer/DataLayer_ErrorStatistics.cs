using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IErrorStatistics
    {
        #region Methods

        public ErrorStatisticsSummary GetErrorStatistics()
        {
            var totalErrors = this.primaryDataContext.ErrorStatistics.Sum(s => s.TotalErrors);
            var errorStatisticsSummary = new ErrorStatisticsSummary();
            var erros = this.primaryDataContext.ErrorStatistics.Select(errorStat => new ErrorStatisticsDetail
            {
                Code = errorStat.Code,
                Description = errorStat.Error.Description,
                Total = errorStat.TotalErrors,
                RatioTotal = ((double)errorStat.TotalErrors * 100) / (double)totalErrors
            });
            errorStatisticsSummary.Errors = erros;
            errorStatisticsSummary.TotalErrors = totalErrors;

            if (this.primaryDataContext.MachineStatistics.Any())
            {
                errorStatisticsSummary.TotalLoadingUnits = this.primaryDataContext.MachineStatistics.First().TotalMovedTrays;
                if (errorStatisticsSummary.TotalLoadingUnits > 0)
                {
                    errorStatisticsSummary.TotalLoadingUnitsBetweenErrors = errorStatisticsSummary.TotalLoadingUnits / totalErrors;
                }
                errorStatisticsSummary.RatioRealiability = 99.937;
            }

            return errorStatisticsSummary;
        }

        #endregion
    }
}
