using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

        /// <summary>
        /// Get the raw database content.
        /// </summary>
        /// <returns>
        ///     The raw database contents (raw bytes)
        /// </returns>
        public byte[] GetRawDatabaseContent()
        {
            const int NUMBER_OF_RETRIES = 5;

            using var scope = this.ServiceScopeFactory.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Retrieve the path of primary database file
            //      example: "Database/MachineAutomationService.Simulation.Primary.db"
            var filePath = GetDBFilePath(configuration.GetDataLayerPrimaryConnectionString());
            var exist = File.Exists(filePath);
            if (!exist)
            {
                this.Logger.LogError($"Error: {filePath} does not exist");
                return null;
            }

            byte[] rawDatabase = null;

            for (var i = 0; i < NUMBER_OF_RETRIES; i++)
            {
                try
                {
                    // Get the raw bytes contents
                    using var stream = File.OpenRead(filePath);
                    rawDatabase = new byte[stream.Length];
                    stream.Read(rawDatabase, 0, rawDatabase.Length);

                    break;
                }
                catch (IOException ioExc) when (i < NUMBER_OF_RETRIES)
                {
                    this.Logger.LogDebug($"Try: #{i + 1}. Error reason: {ioExc.Message}");
                    Thread.Sleep(500);
                }
            }

            /*
            // Write the bytes array back to a file
            using (Stream file = File.OpenWrite("Database/trial.db"))
            {
                file.Write(rawDatabase, 0, rawDatabase.Length);
            }
            */

            this.Logger.LogDebug($"Retrieve raw database content from file {filePath}");
            return rawDatabase;
        }

        /// <summary>
        /// Retrieve the path of the primary database.
        /// </summary>
        /// <param name="primaryConnectionString"></param>
        /// <returns>
        ///     The path
        /// </returns>
        private static string GetDBFilePath(string primaryConnectionString)
        {
            try
            {
                var index = primaryConnectionString.IndexOf("'", StringComparison.CurrentCulture);

                var tmp = primaryConnectionString.Remove(0, index + 1);
                index = tmp.IndexOf("'", StringComparison.CurrentCulture);

                var retValue = tmp.Remove(index, 1);
                return retValue;
            }
            catch (Exception)
            {
            }

            return null;
        }

        #endregion
    }
}
