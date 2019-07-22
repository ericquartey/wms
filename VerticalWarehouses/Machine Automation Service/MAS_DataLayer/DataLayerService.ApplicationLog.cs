using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayerService
    {
        #region Methods

        private async Task ApplicationLogWriterTaskFunction()
        {
            //INFO Create WaitHandle array to wait for multiple events
            var commandHandles = new[]
            {
                this.commandLogQueue.WaitHandle,
                this.notificationLogQueue.WaitHandle
            };

            do
            {
                var handleIndex = WaitHandle.WaitAny(commandHandles);

                this.logger.LogTrace($"1:handleIndex={handleIndex}");

                switch (handleIndex)
                {
                    case 0:
                        await this.LogCommandMessageAsync();
                        break;

                    case 1:
                        await this.LogNotificationMessageAsync();
                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task LogCommandMessageAsync()
        {
            this.logger.LogTrace("1:Method Start");

            while (this.commandLogQueue.Dequeue(out var message))
            {
                this.logger.LogTrace($"2:message={message}");

                var serializedData = JsonConvert.SerializeObject(message.Data);

                var logEntry = new LogEntry();

                logEntry.Data = serializedData;
                logEntry.Description = message.Description;
                logEntry.Destination = message.Destination.ToString();
                logEntry.Source = message.Source.ToString();
                logEntry.TimeStamp = DateTime.Now;
                logEntry.Type = message.Type.ToString();

                var primaryPartitionError = false;
                var secondaryPartitionError = false;

                try
                {
                    await this.primaryDataContext.LogEntries.AddAsync(logEntry, this.stoppingToken);
                    await this.primaryDataContext.SaveChangesAsync(this.stoppingToken);
                }
                catch
                {
                    primaryPartitionError = true;
                }

                if (!this.suppressSecondary)
                {
                    try
                    {
                        await this.secondaryDataContext.LogEntries.AddAsync(logEntry, this.stoppingToken);
                        await this.secondaryDataContext.SaveChangesAsync(this.stoppingToken);
                    }
                    catch
                    {
                        secondaryPartitionError = true;
                    }
                }

                if (primaryPartitionError && secondaryPartitionError)
                {
                    this.logger.LogCritical($"3:Exception: failed to write application log entry in the primary and secondary partition - Exception Code: {DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure}");

                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure);
                }

                if (primaryPartitionError)
                {
                    this.logger.LogCritical($"4:Exception: failed to write application log entry in the primary partition - Exception Code: {DataLayerPersistentExceptionCode.PrimaryPartitionFailure}");

                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.PrimaryPartitionFailure);
                }

                if (secondaryPartitionError)
                {
                    this.logger.LogCritical($"5:Exception: failed to write application log entry in the secondary partition - Exception Code: {DataLayerPersistentExceptionCode.SecondaryPartitionFailure}");

                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.SecondaryPartitionFailure);
                }
            }
        }

        private async Task LogNotificationMessageAsync()
        {
            this.logger.LogTrace("1:Method Start");

            while (this.notificationLogQueue.Dequeue(out var message))
            {
                this.logger.LogTrace($"2:message={message}");

                var logEntry = new LogEntry();

                try
                {
                    var serializedData = JsonConvert.SerializeObject(message.Data);
                    logEntry.Data = serializedData;
                }
                catch
                {
                    logEntry.Data = "Data Not Serializable";
                }

                logEntry.Description = message.Description;
                logEntry.Destination = message.Destination.ToString();
                logEntry.ErrorLevel = message.ErrorLevel.ToString();
                logEntry.Source = message.Source.ToString();
                logEntry.Status = message.Status.ToString();
                logEntry.TimeStamp = DateTime.Now;
                logEntry.Type = message.Type.ToString();

                var primaryPartitionError = false;
                var secondaryPartitionError = false;

                try
                {
                    this.primaryDataContext.LogEntries.Add(logEntry);
                    await this.primaryDataContext.SaveChangesAsync(this.stoppingToken);
                }
                catch
                {
                    primaryPartitionError = true;
                }

                if (!this.suppressSecondary)
                {
                    try
                    {
                        this.secondaryDataContext.LogEntries.Add(logEntry);
                        await this.secondaryDataContext.SaveChangesAsync(this.stoppingToken);
                    }
                    catch
                    {
                        secondaryPartitionError = true;
                    }
                }

                if (primaryPartitionError && secondaryPartitionError)
                {
                    this.logger.LogCritical($"3:Exception: failed to write application log entry in the primary and secondary partition - Exception Code: {DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure}");

                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure);
                }

                if (primaryPartitionError)
                {
                    this.logger.LogCritical($"4:Exception: failed to write application log entry in the primary partition - Exception Code: {DataLayerPersistentExceptionCode.PrimaryPartitionFailure}");

                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.PrimaryPartitionFailure);
                }

                if (secondaryPartitionError)
                {
                    this.logger.LogCritical($"5:Exception: failed to write application log entry in the secondary partition - Exception Code: {DataLayerPersistentExceptionCode.SecondaryPartitionFailure}");

                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.SecondaryPartitionFailure);
                }
            }
        }

        #endregion
    }
}
