using System.Collections.Generic;
using System.Linq;
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
            var errors = new List<ErrorStatisticsDetail>();
            foreach (var errorStat in this.primaryDataContext.ErrorStatistics)
            {
                errors.Add(new ErrorStatisticsDetail
                {
                    Code = errorStat.Code,
                    Description = errorStat.Error.Description,
                    Total = errorStat.TotalErrors,
                    RatioTotal = errorStat.TotalErrors / totalErrors
                });
            }
            errorStatisticsSummary.Errors = errors;
            errorStatisticsSummary.Totalerrors = totalErrors;
            errorStatisticsSummary.TotalLoadingUnits = 1000;
            errorStatisticsSummary.RatioRealiability = 99.937;
            errorStatisticsSummary.TotalLoadingUnitsBetweenErrors = 200;

            return new ErrorStatisticsSummary { RatioRealiability = 1, TotalLoadingUnitsBetweenErrors = 99, Totalerrors = 1000, TotalLoadingUnits = 999 };
        }

        #endregion
    }
}
