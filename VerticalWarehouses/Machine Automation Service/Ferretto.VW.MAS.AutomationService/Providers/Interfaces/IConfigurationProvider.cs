using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.AutomationService
{
    public interface IConfigurationProvider
    {
        #region Methods

        VertimagConfiguration ConfigurationGet();

        void ConfigurationImport(VertimagConfiguration vertimagConfiguration);

        void ConfigurationUpdate(VertimagConfiguration vertimagConfiguration);

        #endregion
    }
}
