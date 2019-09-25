using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal sealed class ErrorsProvider : IErrorsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly NotificationEvent notificationEvent;

        #endregion

        #region Constructors

        public ErrorsProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;

            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion

        #region Methods

        public Error GetCurrent()
        {
            return this.dataContext.Errors
                .Where(e => !e.ResolutionDate.HasValue)
                .OrderBy(e => e.Definition.Severity)
                .ThenBy(e => e.OccurrenceDate)
                .Select(e => new Error
                {
                    Id = e.Id,
                    Code = e.Code,
                    OccurrenceDate = e.OccurrenceDate,
                    ResolutionDate = e.ResolutionDate,
                    Definition = new ErrorDefinition
                    {
                        Code = e.Code,
                        Description = e.Definition.Description,
                        Reason = e.Definition.Reason,
                        Severity = e.Definition.Severity,
                    },
                })
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
                            RatioTotal = s.TotalErrors * 100.0 / totalErrors,
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

        public Error RecordNew(MachineErrors code, BayNumber bayIndex)
        {
            var existingUnresolvedError = this.dataContext.Errors.FirstOrDefault(e => e.Code == (int)code && e.ResolutionDate == null);
            if (existingUnresolvedError != null)
            {
                return existingUnresolvedError;
            }

            var newError = new Error
            {
                Code = (int)code,
                OccurrenceDate = DateTime.UtcNow,
            };

            this.dataContext.Errors.Add(newError);

            var errorStatistics = this.dataContext.ErrorStatistics.SingleOrDefault(e => e.Code == newError.Code);
            if (errorStatistics != null)
            {
                errorStatistics.TotalErrors++;
                this.dataContext.ErrorStatistics.Update(errorStatistics);
            }

            this.dataContext.SaveChanges();

            this.NotifyErrorCreation(newError, bayIndex);

            return newError;
        }

        public Error Resolve(int id)
        {
            var error = this.dataContext.Errors.SingleOrDefault(e => e.Id == id);
            if (error is null)
            {
                throw new EntityNotFoundException(id);
            }

            error.ResolutionDate = DateTime.UtcNow;

            this.dataContext.Errors.Update(error);

            this.dataContext.SaveChanges();

            this.NotifyErrorResolution(error, BayNumber.None);

            return error;
        }

        private void NotifyErrorCreation(Error error, BayNumber bayIndex)
        {
            var message = new NotificationMessage(
                new ErrorStatusMessageData(error.Id),
                $"New error (code: {error.Code})",
                MessageActor.AutomationService,
                MessageActor.Any,
                MessageType.ErrorStatusChanged,
                bayIndex);

            this.notificationEvent.Publish(message);
        }

        private void NotifyErrorResolution(Error error, BayNumber bayIndex)
        {
            var message = new NotificationMessage(
                new ErrorStatusMessageData(error.Id),
                $"Resolved error (code: {error.Code})",
                MessageActor.AutomationService,
                MessageActor.Any,
                MessageType.ErrorStatusChanged,
                bayIndex);

            this.notificationEvent.Publish(message);
        }

        #endregion
    }
}
