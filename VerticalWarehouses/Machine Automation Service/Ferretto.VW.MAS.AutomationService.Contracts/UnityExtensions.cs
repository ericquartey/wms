using System.Net.Http;
using Prism.Ioc;
using Prism.Unity;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public static class UnityExtensions
    {
        private static readonly System.Func<IUnityContainer, HttpClient> DefaultResolveHttpClientFunction = (IUnityContainer c) => c.Resolve<HttpClient>();

        public static IContainerRegistry RegisterMasWebServices(
            this IContainerRegistry containerRegistry,
            System.Uri webServiceUrl,
            System.Func<IUnityContainer, HttpClient> resolveHttpClientFunction = null)
        {
            _ = containerRegistry ?? throw new System.ArgumentNullException(nameof(containerRegistry));
            _ = webServiceUrl ?? throw new System.ArgumentNullException(nameof(webServiceUrl));

            var urlString = webServiceUrl.ToString();

            var resolveFunction = resolveHttpClientFunction ?? DefaultResolveHttpClientFunction;

            var container = containerRegistry.GetContainer();

            container.RegisterType<IMachineBaysWebService>(
                new InjectionFactory(c => new MachineBaysWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineBeltBurnishingProcedureWebService>(
                new InjectionFactory(c => new MachineBeltBurnishingProcedureWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineCellsWebService>(
                new InjectionFactory(c => new MachineCellsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineCellPanelsWebService>(
                new InjectionFactory(c => new MachineCellPanelsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineElevatorWebService>(
                new InjectionFactory(c => new MachineElevatorWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineCarouselWebService>(
                new InjectionFactory(c => new MachineCarouselWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineErrorsWebService>(
                new InjectionFactory(c => new MachineErrorsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineStatisticsWebService>(
                new InjectionFactory(c => new MachineStatisticsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineAccessoriesWebService>(
                new InjectionFactory(c => new MachineAccessoriesWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineServicingWebService>(
                new InjectionFactory(c => new MachineServicingWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineAboutWebService>(
                new InjectionFactory(c => new MachineAboutWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachinePutToLightWebService>(
                new InjectionFactory(c => new MachinePutToLightWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineIdentityWebService>(
                new InjectionFactory(c => new MachineIdentityWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineSetupStatusWebService>(
                new InjectionFactory(c => new MachineSetupStatusWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineLoadingUnitsWebService>(
                new InjectionFactory(c => new MachineLoadingUnitsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachinePowerWebService>(
                new InjectionFactory(c => new MachinePowerWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineMissionOperationsWebService>(
                new InjectionFactory(c => new MachineMissionOperationsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineMissionOperationsWebService>(
                new InjectionFactory(c => new MachineMissionOperationsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineVerticalResolutionCalibrationProcedureWebService>(
                new InjectionFactory(c => new MachineVerticalResolutionCalibrationProcedureWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineSensorsWebService>(
                new InjectionFactory(c => new MachineSensorsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineModeWebService>(
                new InjectionFactory(c => new MachineModeWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineShuttersWebService>(
                new InjectionFactory(c => new MachineShuttersWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineUsersWebService>(
                new InjectionFactory(c => new MachineUsersWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineVerticalOriginProcedureWebService>(
                new InjectionFactory(c => new MachineVerticalOriginProcedureWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineVerticalOffsetProcedureWebService>(
                new InjectionFactory(c => new MachineVerticalOffsetProcedureWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineDevicesWebService>(
                new InjectionFactory(c => new MachineDevicesWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineWeightAnalysisProcedureWebService>(
                new InjectionFactory(c => new MachineWeightAnalysisProcedureWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineWmsStatusWebService>(
               new InjectionFactory(c => new MachineWmsStatusWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineDepositAndPickupProcedureWebService>(
                new InjectionFactory(c => new MachineDepositAndPickupProcedureWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineConfigurationWebService>(
                new InjectionFactory(c => new MachineConfigurationWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineUtcTimeWebService>(
                new InjectionFactory(c => new MachineUtcTimeWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineProfileProcedureWebService>(
                new InjectionFactory(c => new MachineProfileProcedureWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineCompactingWebService>(
                new InjectionFactory(c => new MachineCompactingWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineMissionsWebService>(
                new InjectionFactory(c => new MachineMissionsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineFullTestWebService>(
                new InjectionFactory(c => new MachineFullTestWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineBarcodesWebService>(
                new InjectionFactory(c => new MachineBarcodesWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineCompartmentsWebService>(
                new InjectionFactory(c => new MachineCompartmentsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineAreasWebService>(
                new InjectionFactory(c => new MachineAreasWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineImagesWebService>(
                new InjectionFactory(c => new MachineImagesWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineItemListsWebService>(
                new InjectionFactory(c => new MachineItemListsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineItemsWebService>(
                new InjectionFactory(c => new MachineItemsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineFirstTestWebService>(
                new InjectionFactory(c => new MachineFirstTestWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineEnduranceTestWebService>(
                new InjectionFactory(c => new MachineEnduranceTestWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineExternalBayWebService>(
                new InjectionFactory(c => new MachineExternalBayWebService(urlString, resolveFunction(c))));
                
            container.RegisterType<IMachineDatabaseBackupWebService>(
                new InjectionFactory(c => new MachineDatabaseBackupWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineLogoutSettingsWebService>(
                new InjectionFactory(c => new MachineLogoutSettingsWebService(urlString, resolveFunction(c))));

            container.RegisterType<IMachineAutoCompactingSettingsWebService>(
                new InjectionFactory(c => new MachineAutoCompactingSettingsWebService(urlString, resolveFunction(c))));

            return containerRegistry;
        }
    }
}
