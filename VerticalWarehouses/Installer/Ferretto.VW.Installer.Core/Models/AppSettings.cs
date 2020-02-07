namespace Ferretto.VW.Installer.Core
{
    internal class AppSettings
    {
        #region Properties

        public ConnectionStrings ConnectionStrings { get; set; }

        #endregion
    }

    internal class ConnectionStrings
    {
        #region Constructors

        public ConnectionStrings(string automationServicePrimary, string automationServiceSecondary)
        {
            this.AutomationServicePrimary = automationServicePrimary;
            this.AutomationServiceSecondary = automationServiceSecondary;
        }

        #endregion

        #region Properties

        public string AutomationServicePrimary { get; }

        public string AutomationServiceSecondary { get; }

        public string PrimaryDbPath => this.AutomationServicePrimary.Split('=')[1]?.Trim('\'');

        public string SecondaryDbPath => this.AutomationServiceSecondary.Split('=')[1]?.Trim('\'');

        #endregion
    }
}
