namespace Ferretto.ServiceDesk.Telemetry
{
    public interface IProxy
    {
        #region Properties

        string PasswordHash { get; set; }

        string PasswordSalt { get; set; }

        string Url { get; set; }

        string User { get; set; }

        #endregion
    }
}
