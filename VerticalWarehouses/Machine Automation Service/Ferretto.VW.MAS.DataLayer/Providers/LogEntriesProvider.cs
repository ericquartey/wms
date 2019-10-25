using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class LogEntriesProvider : ILogEntriesProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<LogEntriesProvider> logger;

        #endregion

        #region Constructors

        public LogEntriesProvider(
            ILogger<LogEntriesProvider> logger,
            DataLayerContext dataContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public void Add(CommandMessage command)
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
            };

            this.Create(logEntry);
        }

        public void Add(NotificationMessage notification)
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
            };

            this.Create(logEntry);
        }

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

        private void Create(LogEntry logEntry)
        {
            this.logger.LogDebug(logEntry.ToString());

            //this.dataContext.LogEntries.Add(logEntry);

            //this.dataContext.SaveChanges();
        }

        #endregion
    }
}
