using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class LogEntriesProvider : ILogEntriesProvider
    {
        #region Fields

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

        private readonly ILogger<LogEntriesProvider> logger;

        #endregion

        #region Constructors

        static LogEntriesProvider()
        {
            SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        public LogEntriesProvider(ILogger<LogEntriesProvider> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void Add(CommandMessage command)
        {
            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                var serializedData = SerializeMessageData(command.Data);

                var logEntry = new LogEntry
                {
                    BayNumber = command.RequestingBay,
                    Data = serializedData,
                    Description = command.Description,
                    Destination = command.Destination,
                    Source = command.Source,
                    TimeStamp = DateTime.Now,
                    Type = command.Type,
                    TargetBay = command.TargetBay,
                };

                this.Create(logEntry);
            }
        }

        public void Add(NotificationMessage notification)
        {
            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                var serializedData = SerializeMessageData(notification.Data);

                var logEntry = new LogEntry
                {
                    BayNumber = notification.RequestingBay,
                    Data = serializedData,
                    Description = notification.Description,
                    Destination = notification.Destination,
                    Source = notification.Source,
                    TimeStamp = DateTime.Now,
                    Type = notification.Type,
                    ErrorLevel = notification.ErrorLevel,
                    Status = notification.Status,
                    TargetBay = notification.TargetBay,
                };

                this.Create(logEntry);
            }
        }

        private static string SerializeMessageData(IMessageData messageData)
        {
            try
            {
                return JsonConvert.SerializeObject(messageData, SerializerSettings);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void Create(LogEntry logEntry)
        {
            this.logger.LogDebug(logEntry.ToString());
        }

        #endregion
    }
}
