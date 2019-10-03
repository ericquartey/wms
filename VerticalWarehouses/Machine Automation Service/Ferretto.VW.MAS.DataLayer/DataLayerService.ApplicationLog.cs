using System;
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

        private void SaveEntryToDb(LogEntry logEntry)
        {
            this.Logger.LogTrace($"Saving log entry '{logEntry.Description}' (id={logEntry.Id}) to database.");

            using (var dbContext = this.scope.ServiceProvider.GetRequiredService<DataLayerContext>())
            {
                dbContext.LogEntries.Add(logEntry);

                dbContext.SaveChanges();
            }
        }

        #endregion
    }
}
