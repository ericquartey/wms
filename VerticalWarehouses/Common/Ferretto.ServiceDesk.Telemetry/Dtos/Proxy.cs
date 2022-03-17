namespace Ferretto.ServiceDesk.Telemetry
{
    public class Proxy : IProxy
    {
        #region Constructors

        public Proxy()
        {
        }

        #endregion

        #region Properties

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public string Url { get; set; }

        public string User { get; set; }

        #endregion
    }
}
