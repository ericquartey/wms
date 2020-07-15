using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Realms;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class ErrorLogProvider : IErrorLogProvider
    {
        #region Fields

        private readonly Realm realm;

        #endregion

        #region Constructors

        public ErrorLogProvider(Realm realm)
        {
            this.realm = realm;
        }

        #endregion

        #region Methods

        public IEnumerable<IErrorLog> GetAll()
        {
            return this.realm.All<Models.ErrorLog>().ToArray();
        }

        public async Task SaveAsync(string serialNumber, IErrorLog errorLog)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (errorLog is null)
            {
                throw new System.ArgumentNullException(nameof(errorLog));
            }

            var machine = this.realm.All<Models.Machine>().SingleOrDefault(m => m.SerialNumber == serialNumber);
            if (machine is null)
            {
                throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
            }

            var logEntry = new Models.ErrorLog
            {
                Machine = machine,
                AdditionalText = errorLog.AdditionalText,
                BayNumber = errorLog.BayNumber,
                Code = errorLog.Code,
                DetailCode = errorLog.DetailCode,
                OccurrenceDate = errorLog.OccurrenceDate,
                ResolutionDate = errorLog.ResolutionDate
            };

            await this.realm.WriteAsync(r => r.Add(logEntry));
        }

        #endregion
    }
}
