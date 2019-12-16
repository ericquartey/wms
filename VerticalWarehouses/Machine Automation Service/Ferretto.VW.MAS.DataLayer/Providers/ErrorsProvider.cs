using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Extensions;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ErrorsProvider : IErrorsProvider, IDisposable
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<DataLayerService> logger;

        private readonly NotificationEvent notificationEvent;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool isDisposed;

        #endregion

        #region Constructors

        public ErrorsProvider(
            DataLayerContext dataContext,
            IServiceScopeFactory serviceScopeFactory,
            IEventAggregator eventAggregator,
            ILogger<DataLayerService> logger)
        {
            _ = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        public MachineError GetCurrent()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Errors
                        .Where(e => !e.ResolutionDate.HasValue)
                        .OrderBy(e => e.Definition.Severity)
                        .ThenBy(e => e.OccurrenceDate)
                        .Select(e => new MachineError
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
        }

        public ErrorStatisticsSummary GetStatistics()
        {
            lock (this.dataContext)
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
        }

        public MachineError RecordNew(MachineErrorCode code, BayNumber bayNumber = BayNumber.None)
        {
            var newError = new MachineError
            {
                Code = (int)code,
                OccurrenceDate = DateTime.Now,
                BayNumber = bayNumber,
            };

            lock (this.dataContext)
            {
                var existingUnresolvedError = this.dataContext.Errors.FirstOrDefault(
                    e =>
                        e.Code == (int)code
                        &&
                        e.ResolutionDate == null
                        &&
                        e.BayNumber == bayNumber);

                if (existingUnresolvedError != null)
                {
                    this.logger.LogWarning($"Machine error {code} ({(int)code}) for {bayNumber} was not triggered because already present and still unresolved.");
                    return existingUnresolvedError;
                }

                // TODO: per il momento prendiamo il testo dell'errore dal database: si può utilizzare la risorsa ErrorDescriptions?
                var errorDefinition = this.dataContext.ErrorDefinitions.FirstOrDefault(e => e.Code == newError.Code);
                this.logger.LogError($"Machine error {errorDefinition?.Description ?? code.ToString()} ({(int)code}) for {bayNumber} was triggered.");
                if (errorDefinition != null)
                {
                    newError.Definition = new ErrorDefinition
                    {
                        Code = errorDefinition.Code,
                        Description = errorDefinition.Description
                    };
                }
                this.dataContext.Errors.Add(newError);

                var errorStatistics = this.dataContext.ErrorStatistics.SingleOrDefault(e => e.Code == newError.Code);
                if (errorStatistics != null)
                {
                    errorStatistics.TotalErrors++;
                    this.dataContext.ErrorStatistics.Update(errorStatistics);
                }

                this.dataContext.SaveChanges();
            }

            this.NotifyErrorCreation(newError, bayNumber);

            return newError;
        }

        public MachineError Resolve(int id)
        {
            lock (this.dataContext)
            {
                var error = this.dataContext.Errors.SingleOrDefault(e => e.Id == id);
                if (error is null)
                {
                    throw new EntityNotFoundException(id);
                }

                if (!this.IsErrorStillActive(error.Code))
                {
                    error.ResolutionDate = DateTime.Now;
                    this.logger.LogDebug($"User error {error.Code} for {error.BayNumber} marked as resolved.");

                    this.dataContext.Errors.Update(error);

                    this.dataContext.SaveChanges();
                }

                this.NotifyErrorResolution(error);

                return error;
            }
        }

        public void ResolveAll()
        {
            IEnumerable<int> errors;
            lock (this.dataContext)
            {
                errors = this.dataContext.Errors.AsNoTracking()
                   .Where(e => e.ResolutionDate == null)
                   .Select(e => e.Id)
                   .ToArray();
            }
            errors.ToList().ForEach(id => this.Resolve(id));
        }

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
        }

        private bool IsErrorStillActive(int code)
        {
            var machineError = (MachineErrorCode)code;

            var enumField = typeof(MachineErrorCode).GetField(machineError.ToString());

            var attributes = enumField
                .GetCustomAttributes(typeof(ErrorConditionAttribute), false)
                .Cast<ErrorConditionAttribute>();

            var isErrorStillActive = attributes.Any();

            foreach (var attribute in attributes)
            {
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var condition = scope.ServiceProvider.GetConditionEvaluator(attribute);

                    isErrorStillActive &= !condition.IsSatisfied();
                }
            }

            return isErrorStillActive;
        }

        private void NotifyErrorCreation(MachineError error, BayNumber bayNumber)
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

        private void NotifyErrorResolution(MachineError error)
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
