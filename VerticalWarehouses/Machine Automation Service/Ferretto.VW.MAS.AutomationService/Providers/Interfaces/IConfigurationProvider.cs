using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.AutomationService
{
    public interface IConfigurationProvider
    {
        #region Methods

        VertimagConfiguration ConfigurationGet();

        void ConfigurationImport(VertimagConfiguration vertimagConfiguration, IServiceScopeFactory serviceScopeFactory);

        void ConfigurationUpdate(VertimagConfiguration vertimagConfiguration, IServiceScopeFactory serviceScopeFactory);

        void UpdateMachine(Machine machine);

        VertimagConfiguration GetJsonConfiguration();

        #endregion
    }
}
