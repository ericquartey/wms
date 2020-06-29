using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ferretto.VW.Installer.Core
{
    public static class Container
    {
        #region Fields

        private static IInstallationService installationServiceSingleton;

        private static IMachineConfigurationService machineConfigurationServiceSingleton;

        private static ISetupModeService setupModeServiceSingleton;

        #endregion

        #region Methods

        public static IInstallationService GetInstallationService()
        {
            return installationServiceSingleton ??= new InstallationService(GetSetupModeService());
        }

        public static IMachineConfigurationService GetMachineConfigurationService()
        {
            return machineConfigurationServiceSingleton ??= new MachineConfigurationService();
        }

        public static ISetupModeService GetSetupModeService()
        {
            return setupModeServiceSingleton ??= new SetupModeService();
        }

        #endregion
    }
}
