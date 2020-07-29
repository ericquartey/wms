namespace Ferretto.ServiceDesk.Telemetry
{
    public interface IMachine
    {
        string ModelName { get; set; }

        string SerialNumber { get; set; }
    }
}
