using System;
using Prism.Ioc;
using Prism.Unity;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public static class UnityExtensions
    {
        private static readonly System.Func<IUnityContainer, RetryHttpClient> DefaultResolveHttpClientFunction = (IUnityContainer c) => c.Resolve<RetryHttpClient>();

        public static IContainerRegistry RegisterMasWebServices(
            this IContainerRegistry container,
            System.Uri webServiceUrl,
            System.Func<IUnityContainer, RetryHttpClient> resolveHttpClientFunction = null)
        {
            _ = container ?? throw new ArgumentNullException(nameof(container));
            _ = webServiceUrl ?? throw new ArgumentNullException(nameof(webServiceUrl));

            var urlString = webServiceUrl.ToString();

            var resolveFunction = resolveHttpClientFunction ?? DefaultResolveHttpClientFunction;

            container.Register<RetryHttpClient, RetryHttpClient>();

            container.GetContainer().RegisterType<IMachineBaysWebService>(
                new InjectionFactory(c => new MachineBaysWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineBeltBurnishingProcedureWebService>(
                new InjectionFactory(c => new MachineBeltBurnishingProcedureWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineCellsWebService>(
                new InjectionFactory(c => new MachineCellsWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineCellPanelsWebService>(
               new InjectionFactory(c => new MachineCellPanelsWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineElevatorWebService>(
                new InjectionFactory(c => new MachineElevatorWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineCarouselWebService>(
                new InjectionFactory(c => new MachineCarouselWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineErrorsWebService>(
                new InjectionFactory(c => new MachineErrorsWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineIdentityWebService>(
                new InjectionFactory(c => new MachineIdentityWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineSetupStatusWebService>(
                new InjectionFactory(c => new MachineSetupStatusWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineLoadingUnitsWebService>(
                new InjectionFactory(c => new MachineLoadingUnitsWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachinePowerWebService>(
                new InjectionFactory(c => new MachinePowerWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineMissionOperationsWebService>(
                new InjectionFactory(c => new MachineMissionOperationsWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineMissionOperationsWebService>(
                new InjectionFactory(c => new MachineMissionOperationsWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineVerticalResolutionCalibrationProcedureWebService>(
                new InjectionFactory(c => new MachineVerticalResolutionCalibrationProcedureWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineSensorsWebService>(
                new InjectionFactory(c => new MachineSensorsWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineModeWebService>(
                new InjectionFactory(c => new MachineModeWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineShuttersWebService>(
                new InjectionFactory(c => new MachineShuttersWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineUsersWebService>(
                new InjectionFactory(c => new MachineUsersWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineVerticalOriginProcedureWebService>(
                new InjectionFactory(c => new MachineVerticalOriginProcedureWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineVerticalOffsetProcedureWebService>(
                new InjectionFactory(c => new MachineVerticalOffsetProcedureWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineDevicesWebService>(
                new InjectionFactory(c => new MachineDevicesWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineWeightAnalysisProcedureWebService>(
                new InjectionFactory(c => new MachineWeightAnalysisProcedureWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineDepositAndPickupProcedureWebService>(
                new InjectionFactory(c => new MachineDepositAndPickupProcedureWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineConfigurationWebService>(
                new InjectionFactory(c => new MachineConfigurationWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineProfileProcedureWebService>(
                new InjectionFactory(c => new MachineProfileProcedureWebService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineCompactingWebService>(
                new InjectionFactory(c => new MachineCompactingWebService(urlString, resolveFunction(c))));

            return container;
        }
    }
}
