using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Extensions;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ErrorsProvider : IErrorsProvider, IDisposable
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly NotificationEvent notificationEvent;

        private readonly IServiceScope scope;

        private bool isDisposed;

        #endregion

        #region Constructors

        public ErrorsProvider(
            DataLayerContext dataContext,
            IServiceScopeFactory serviceScopeFactory,
            IEventAggregator eventAggregator)
        {
            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));

            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();

            this.scope = serviceScopeFactory.CreateScope();
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

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

        public Error RecordNew(MachineErrors code, BayNumber bayNumber = BayNumber.None)
        {
            var existingUnresolvedError = this.dataContext.Errors.FirstOrDefault(e => e.Code == (int)code && e.ResolutionDate == null);
            if (existingUnresolvedError != null)
            {
                return existingUnresolvedError;
            }

            var newError = new Error
            {
                Code = (int)code,
                OccurrenceDate = DateTime.Now,
            };

            this.dataContext.Errors.Add(newError);

            var errorStatistics = this.dataContext.ErrorStatistics.SingleOrDefault(e => e.Code == newError.Code);
            if (errorStatistics != null)
            {
                errorStatistics.TotalErrors++;
                this.dataContext.ErrorStatistics.Update(errorStatistics);
            }

            this.dataContext.SaveChanges();

            this.NotifyErrorCreation(newError, bayNumber);

            return newError;
        }

        public Error Resolve(int id)
        {
            var error = this.dataContext.Errors.SingleOrDefault(e => e.Id == id);
            if (error is null)
            {
                throw new EntityNotFoundException(id);
            }

            if (!this.IsErrorStillActive(error.Code))
            {
                error.ResolutionDate = DateTime.Now;

                this.dataContext.Errors.Update(error);

                this.dataContext.SaveChanges();

                this.NotifyErrorResolution(error);
            }

            return error;
        }

        public IEnumerable<Error> ResolveAll()
        {
            var errorIdsToResolve = this.dataContext.Errors
                .Where(e => e.ResolutionDate == null)
                .Select(e => e.Id)
                .ToArray();

            return errorIdsToResolve.Select(id => this.Resolve(id));
        }

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.scope?.Dispose();
            }

            this.isDisposed = true;
        }

        private bool IsErrorStillActive(int code)
        {
            var machineError = (MachineErrors)code;

            var enumField = typeof(MachineErrors).GetField(machineError.ToString());

            var attributes = enumField
                .GetCustomAttributes(typeof(ErrorConditionAttribute), false)
                .Cast<ErrorConditionAttribute>();

            var isErrorStillActive = attributes.Any();

            foreach (var attribute in attributes)
            {
                var condition = this.scope.ServiceProvider.GetConditionEvaluator(attribute);

                isErrorStillActive &= !condition.IsSatisfied();
            }

            return isErrorStillActive;
        }

        private void NotifyErrorCreation(Error error, BayNumber bayNumber)
        {
            var message = new NotificationMessage(
                new ErrorStatusMessageData(error.Id),
                $"New error (code: {error.Code})",
                MessageActor.AutomationService,
                MessageActor.Any,
                MessageType.ErrorStatusChanged,
                bayNumber);

            this.notificationEvent.Publish(message);
        }

        private void NotifyErrorResolution(Error error)
        {
            var message = new NotificationMessage(
                new ErrorStatusMessageData(error.Id),
                $"Resolved error (code: {error.Code})",
                MessageActor.AutomationService,
                MessageActor.Any,
                MessageType.ErrorStatusChanged,
                BayNumber.None);

            this.notificationEvent.Publish(message);
        }

        #endregion
    }
}
