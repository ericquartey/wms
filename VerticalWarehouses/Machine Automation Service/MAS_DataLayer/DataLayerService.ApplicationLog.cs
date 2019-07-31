using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService
    {
        #region Methods

        private static string SerializeMessageData(IMessageData messageData)
        {
            var serializedData = "Message data could not be serialized.";

            try
            {
                serializedData = JsonConvert.SerializeObject(messageData);
            }
            catch (Exception)
            {
                // do nothing
            }

            return serializedData;
        }

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
            while (!this.StoppingToken.IsCancellationRequested);
        }

        private void LogCommandMessage()
        {
            this.Logger.LogTrace("1:Method Start");

            while (this.commandLogQueue.Dequeue(out var message))
            {
                this.Logger.LogTrace($"2:message={message}");

                var serializedData = SerializeMessageData(message.Data);

                var logEntry = new LogEntry
                {
                    Data = serializedData,
                    Description = message.Description,
                    Destination = message.Destination.ToString(),
                    Source = message.Source.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Type = message.Type.ToString(),
                };

                this.SaveEntryToDb(logEntry);
            }
        }

        private void LogNotificationMessage()
        {
            this.Logger.LogTrace("1:Method Start");

            while (this.notificationLogQueue.Dequeue(out var message))
            {
                this.Logger.LogTrace($"2:message={message}");

                var serializedData = SerializeMessageData(message.Data);

                var logEntry = new LogEntry
                {
                    Data = serializedData,
                    Description = message.Description,
                    Destination = message.Destination.ToString(),
                    Source = message.Source.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Type = message.Type.ToString(),
                    ErrorLevel = message.ErrorLevel.ToString(),
                    Status = message.Status.ToString(),
                };

                this.SaveEntryToDb(logEntry);
            }
        }

        private void SaveEntryToDb(LogEntry logEntry)
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                dbContext.LogEntries.Add(logEntry);

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception)
                {
                    this.Logger.LogCritical($"4:Exception: failed to write application log entry into database.");

                    throw;
                }
            }
        }

        #endregion
    }
}
