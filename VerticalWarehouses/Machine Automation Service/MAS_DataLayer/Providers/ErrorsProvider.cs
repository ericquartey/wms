using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels.Errors;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class ErrorsProvider : IErrorsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public ErrorsProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public Error GetCurrent()
        {
            return this.dataContext.Errors
                .Where(e => !e.ResolutionDate.HasValue)
                .OrderBy(e => e.Definition.Severity)
                .ThenBy(e => e.OccurrenceDate)
                .FirstOrDefault();
        }

        public ErrorStatisticsSummary GetStatistics()
        {
            var totalErrors = this.dataContext.ErrorStatistics.Sum(s => s.TotalErrors);
            var summary = new ErrorStatisticsSummary
            {
                TotalErrors = totalErrors,
                Errors = this.dataContext.ErrorStatistics
                    .Select(s =>
                        new ErrorStatisticsDetail
                        {
                            Code = s.Code,
                            Description = s.Error.Description,
                            Total = s.TotalErrors,
                            RatioTotal = s.TotalErrors * 100.0 / totalErrors
                        }),
            };

            if (this.dataContext.MachineStatistics.Any())
            {
                summary.TotalLoadingUnits = this.dataContext.MachineStatistics.First().TotalMovedTrays;
                if (summary.TotalLoadingUnits > 0)
                {
                    summary.TotalLoadingUnitsBetweenErrors = summary.TotalLoadingUnits / totalErrors;
                }

                summary.ReliabilityPercentage = totalErrors * 100.0 / summary.TotalLoadingUnits;
            }

            return summary;
        }

        public Error RecordNew(int code)
        {
            var newError = new Error
            {
                Code = code,
                OccurrenceDate = System.DateTime.UtcNow,
            };

            this.dataContext.Errors.Add(newError);

            this.dataContext.SaveChanges();

            return newError;
        }

        #endregion
    }
}
