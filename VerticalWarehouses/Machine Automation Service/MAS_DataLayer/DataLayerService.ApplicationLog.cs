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
