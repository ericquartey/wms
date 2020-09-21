namespace Ferretto.ServiceDesk.Telemetry
{
    public interface IMachine
    {
        #region Properties

        string ModelName { get; set; }

        string SerialNumber { get; set; }

        string Version { get; set; }

        #endregion
    }
}
