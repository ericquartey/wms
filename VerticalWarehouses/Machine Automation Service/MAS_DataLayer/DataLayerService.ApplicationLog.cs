using System;
using System.Threading;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService
    {
        #region Methods

        private void ApplicationLogWriterTaskFunction()
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
                        this.LogCommandMessage();
                        break;

                    case 1:
                        this.LogNotificationMessage();
                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void LogCommandMessage()
        {
            this.logger.LogTrace("1:Method Start");

            while (this.commandLogQueue.Dequeue(out var message))
            {
                this.logger.LogTrace($"2:message={message}");

                string serializedData = "Data Not Serializable";

                try
                {
                    serializedData = JsonConvert.SerializeObject(message.Data);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Exception {ex.Message} while serializing {message.Type} data");
                }

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
                    using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                    {
                        primaryDataContext.LogEntries.Add(logEntry);
                        primaryDataContext.SaveChanges();
                    }
                }
                catch
                {
                    primaryPartitionError = true;
                }

                if (!this.suppressSecondary)
                {
                    try
                    {
                        using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                        {
                            secondaryDataContext.LogEntries.Add(logEntry);
                            secondaryDataContext.SaveChanges();
                        }
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

        private void LogNotificationMessage()
        {
            this.logger.LogTrace("1:Method Start");

            while (this.notificationLogQueue.Dequeue(out var message))
            {
                this.logger.LogTrace($"2:message={message}");

                string serializedData = "Data Not Serializable";

                try
                {
                    serializedData = JsonConvert.SerializeObject(message.Data);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Exception {ex.Message} while serializing {message.Type} data");
                }

                var logEntry = new LogEntry();

                logEntry.Data = serializedData;
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
                    using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                    {
                        primaryDataContext.LogEntries.Add(logEntry);
                        primaryDataContext.SaveChanges();
                    }
                }
                catch
                {
                    primaryPartitionError = true;
                }

                if (!this.suppressSecondary)
                {
                    try
                    {
                        using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                        {
                            secondaryDataContext.LogEntries.Add(logEntry);
                            secondaryDataContext.SaveChanges();
                        }
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
