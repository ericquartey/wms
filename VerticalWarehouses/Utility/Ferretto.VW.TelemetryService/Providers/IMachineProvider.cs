using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IMachineProvider
    {
        #region Methods

        IMachine? Get();

        IMachine GetRaw();

        void SaveAsync(IMachine machine);

        void SaveRawDatabaseContent(byte[]? rawDatabaseContent);

        #endregion
    }
}
