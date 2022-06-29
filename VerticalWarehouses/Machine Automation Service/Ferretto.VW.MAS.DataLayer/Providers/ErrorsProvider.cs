using System;
using System.Collections.Generic;
using System.Linq;
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
    internal sealed class ErrorsProvider : IErrorsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<DataLayerService> logger;

        private readonly NotificationEvent notificationEvent;

        private readonly IServiceScopeFactory serviceScopeFactory;

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

        public MachineError GetById(int id)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Errors.SingleOrDefault(e => e.Id == id);
            }
        }

        public MachineError GetCurrent()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Errors
                        .Where(e => !e.ResolutionDate.HasValue)
                        .OrderBy(e => e.OccurrenceDate)
                        .FirstOrDefault();
            }
        }

        public List<MachineError> GetErrors()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Errors.ToList();
            }
        }

        public MachineError GetLast()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Errors
                        .OrderBy(e => e.OccurrenceDate)
                        .LastOrDefault();
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
                                Description = s.Description,
                                Total = s.TotalErrors,
                                RatioTotal = s.TotalErrors * 100.0 / totalErrors,
                            }),
                };

                if (this.dataContext.MachineStatistics.Any())
                {
                    var statistics = this.dataContext.LastOrNull(this.dataContext.MachineStatistics, o => o.Id)?.Entity;
                    summary.TotalLoadingUnits = statistics.TotalLoadUnitsInBay1 + statistics.TotalLoadUnitsInBay2 + statistics.TotalLoadUnitsInBay3;
                    if (summary.TotalLoadingUnits > 0)
                    {
                        summary.TotalLoadingUnitsBetweenErrors = summary.TotalLoadingUnits / totalErrors;
                    }

                    summary.ReliabilityPercentage = totalErrors * 100.0 / summary.TotalLoadingUnits;
                }

                return summary;
            }
        }

        public bool IsErrorSmall()
        {
            var error = this.GetCurrent();

            if (error is null)
            {
                return true;
            }
            return (error.Code == (int)MachineErrorCode.SecurityWasTriggered
                || error.Code == (int)MachineErrorCode.SecurityBarrierWasTriggered
                || error.Code == (int)MachineErrorCode.SecurityButtonWasTriggered
                || error.Code == (int)MachineErrorCode.SecurityLeftSensorWasTriggered
                || error.Code == (int)MachineErrorCode.SecurityRightSensorWasTriggered
                || error.Code == (int)MachineErrorCode.ElevatorOverrunDetected
                || error.Code == (int)MachineErrorCode.ElevatorUnderrunDetected);
        }

        public bool NeedsHoming()
        {
            var error = this.GetCurrent();

            if (error is null)
            {
                return false;
            }
            return error.Code == (int)MachineErrorCode.VerticalZeroHighError
                || error.Code == (int)MachineErrorCode.VerticalZeroLowError;
        }

        // removes resolved errors older than 30 days
        public int PurgeErrors()
        {
            lock (this.dataContext)
            {
                var count = 0;
                var errors = this.dataContext.Errors
                    .AsEnumerable()
                    .Where(x => x.ResolutionDate.HasValue
                        && DateTime.UtcNow.Subtract(x.ResolutionDate.Value).Days > 31
                        )
                    .ToList();
                if (errors.Any())
                {
                    count = errors.Count;
                    this.dataContext.Errors.RemoveRange(errors);
                    this.dataContext.SaveChanges();
                    this.logger.LogInformation($"Deleted {count} Errors.");
                }
                return count;
            }
        }

        /// <summary>
        /// Add a machine error to the db and notify the Automation Service about the error.
        /// </summary>
        /// <param name="code">The error code id.</param>
        /// <param name="bayNumber">The bay number.</param>
        /// <returns></returns>
        public MachineError RecordNew(MachineErrorCode code, BayNumber bayNumber = BayNumber.None, string additionalText = null)
        {
            var newError = new MachineError
            {
                Code = (int)code,
                OccurrenceDate = DateTime.Now,
                InverterIndex = 0,
                DetailCode = 0,
                BayNumber = bayNumber,
                AdditionalText = additionalText
            };

            lock (this.dataContext)
            {
                try
                {
                    var existingUnresolvedError = this.dataContext.Errors.Where(
                        e => e.ResolutionDate == null
                            &&
                            e.BayNumber == bayNumber)
                        .ToList();

                    var errorStatistics = this.dataContext.ErrorStatistics.SingleOrDefault(e => e.Code == newError.Code);

                    if (existingUnresolvedError.Any())
                    {
                        // discard only the same error
                        if (newError.Severity >= (int)MachineErrorSeverity.High && existingUnresolvedError.Any(e => e.Code == (int)code))
                        {
                            this.logger.LogWarning($"Machine error {code} ({(int)code}) for {bayNumber} was not triggered because already active.");
                            return existingUnresolvedError.First(e => e.Code == (int)code);
                        }

                        // there are active errors different from code

                        //// TODO enable this loop to discard subsequent errors of lower severity
                        //foreach (var activeError in existingUnresolvedError)
                        //{
                        //    if (activeError.Severity < errorStatistics.Severity && activeError.Code > 0)
                        //    {
                        //        this.logger.LogWarning($"Machine error {code} ({(int)code}) for {bayNumber} was not triggered because a higher severity error is already active.");
                        //        return newError;
                        //    }
                        //}

                        if (newError.Severity == (int)MachineErrorSeverity.High)
                        {
                            // resolve low priority errors
                            foreach (var activeError in existingUnresolvedError.Where(e => e.Severity == (int)MachineErrorSeverity.Low))
                            {
                                this.logger.LogTrace($"Machine error {activeError.Code} ({activeError.Code}) for {bayNumber} was resolved by higher priority error {code}.");
                                this.Resolve(activeError.Id, force: true);
                            }
                        }

                        if (newError.Severity < (int)MachineErrorSeverity.High)
                        {
                            // discard all subsequent errors
                            this.logger.LogWarning($"Machine error {code} ({(int)code}) for {bayNumber} was not triggered because another high priority error is already active.");
                            return newError;
                        }
                    };

                    this.dataContext.Errors.Add(newError);

                    if (errorStatistics != null)
                    {
                        errorStatistics.TotalErrors++;
                        this.dataContext.ErrorStatistics.Update(errorStatistics);
                    }
                    else
                    {
                        this.dataContext.ErrorStatistics.Add(new ErrorStatistic { Code = newError.Code, TotalErrors = 1 });
                    }

                    this.dataContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex.Message);
                }
            }

            this.NotifyErrorCreation(newError, bayNumber);

            this.logger.LogError($"Error: {code} ({(int)code}); Bay {bayNumber}; {newError.Description}; {newError.AdditionalText}");

            if (newError.Severity == (int)MachineErrorSeverity.NeedsHoming)
            {
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var machineVolatile = scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                    machineVolatile.IsHomingExecuted = false;
                }
            }

            return newError;
        }

        /// <summary>
        /// Add a machine error to the db and notify the Automation Service about the error.
        /// Reserved for the MachineErrorCode.InverterFaultStateDetected.
        /// </summary>
        /// <param name="inverterIndex">The inverter index.</param>
        /// <param name="detailCode">The detail code of the inverter error.</param>
        /// <param name="bayNumber">The bay number.</param>
        /// <returns></returns>
        public MachineError RecordNew(int inverterIndex, ushort detailCode, BayNumber bayNumber = BayNumber.None, string detailText = null)
        {
            var newError = new MachineError
            {
                Code = (int)MachineErrorCode.InverterFaultStateDetected,
                OccurrenceDate = DateTime.Now,
                InverterIndex = inverterIndex,
                BayNumber = bayNumber,
                DetailCode = detailCode,
                AdditionalText = detailText
            };

            lock (this.dataContext)
            {
                //var existingUnresolvedError = this.dataContext.Errors.FirstOrDefault(
                //    e =>
                //        e.Code == (int)MachineErrorCode.InverterFaultStateDetected
                //        &&
                //        e.ResolutionDate == null
                //        &&
                //        e.InverterIndex == inverterIndex
                //        &&
                //        e.DetailCode == detailCode);
                var existingUnresolvedError = this.dataContext.Errors.Where(
                    e => e.ResolutionDate == null
                        &&
                        e.BayNumber == bayNumber)
                    .ToList();
                var code = (int)MachineErrorCode.InverterFaultStateDetected;
                if (existingUnresolvedError.Any(e => e.Code == code && e.InverterIndex == inverterIndex && e.DetailCode == detailCode))
                {
                    this.logger.LogWarning($"Machine error {MachineErrorCode.InverterFaultStateDetected} (InverterIndex: {inverterIndex}; detail error code: {detailCode}) was not triggered because already present and still unresolved.");
                    return existingUnresolvedError.First(e => e.Code == code);
                }
                // resolve low priority errors
                foreach (var activeError in existingUnresolvedError.Where(e => e.Severity == (int)MachineErrorSeverity.Low || e.Code == (int)MachineErrorCode.InverterErrorBaseCode))
                {
                    this.logger.LogDebug($"Machine error {activeError.Code} ({activeError.Code}) for {bayNumber} was resolved by higher priority error {code}.");
                    this.Resolve(activeError.Id, force: true);
                }

                this.dataContext.Errors.Add(newError);

                var errorStatistics = this.dataContext.ErrorStatistics.SingleOrDefault(e => e.Code == newError.Code);
                if (errorStatistics != null)
                {
                    errorStatistics.TotalErrors++;
                    this.dataContext.ErrorStatistics.Update(errorStatistics);
                }
                else
                {
                    this.dataContext.ErrorStatistics.Add(new ErrorStatistic { Code = newError.Code, TotalErrors = 1 });
                }

                this.dataContext.SaveChanges();
            }

            this.NotifyErrorCreation(newError, bayNumber);

            this.logger.LogError($"Error: {MachineErrorCode.InverterFaultStateDetected} ({newError.Code}); Inverter Fault: 0x{detailCode:X4} {newError.AdditionalText}; index {inverterIndex}; Bay {bayNumber}; {newError.Description}");

            return newError;
        }

        public MachineError Resolve(int id, bool force = false)
        {
            lock (this.dataContext)
            {
                var error = this.dataContext.Errors.SingleOrDefault(e => e.Id == id);
                if (error is null)
                {
                    throw new EntityNotFoundException(id);
                }

                if (force || !this.IsErrorStillActive(error.Code, error.BayNumber))
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

        public void ResolveAll(bool force = false)
        {
            IEnumerable<int> errors;
            lock (this.dataContext)
            {
                errors = this.dataContext.Errors.AsNoTracking()
                   .Where(e => e.ResolutionDate == null)
                   .Select(e => e.Id)
                   .ToArray();
            }
            errors.ToList().ForEach(id => this.Resolve(id, force));
        }

        private bool IsErrorStillActive(int code, BayNumber bayNumber)
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

                    isErrorStillActive &= !condition.IsSatisfied(bayNumber);
                }
            }

            return isErrorStillActive;
        }

        private void NotifyErrorCreation(MachineError error, BayNumber bayNumber)
        {
            var message = new NotificationMessage(
                new ErrorStatusMessageData(error.Id),
                $"New error (code: {error.Code})",
                MessageActor.Any,
                MessageActor.AutomationService,
                MessageType.ErrorStatusChanged,
                bayNumber,
                status: MessageStatus.OperationInverterFault);

            this.notificationEvent.Publish(message);
        }

        private void NotifyErrorResolution(MachineError error)
        {
            var message = new NotificationMessage(
                new ErrorStatusMessageData(error.Id),
                $"Resolved error (code: {error.Code})",
                MessageActor.Any,
                MessageActor.AutomationService,
                MessageType.ErrorStatusChanged,
                BayNumber.None);

            this.notificationEvent.Publish(message);
        }

        #endregion
    }
}
