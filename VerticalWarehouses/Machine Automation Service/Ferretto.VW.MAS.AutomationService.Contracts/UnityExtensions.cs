using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Ioc;
using Prism.Unity;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public static class UnityExtensions
    {
        public static IContainerRegistry RegisterMachineAutomationHubs(
            this IContainerRegistry container,
            System.Uri serviceUrl,
            string operatorHubPath,
            string installationHubPath)
        {
            var urlString = serviceUrl.ToString();

            var operatorHubUrl = new System.Uri(serviceUrl, operatorHubPath);

            container.RegisterInstance<IOperatorHubClient>(new OperatorHubClient(operatorHubUrl));
            container.RegisterInstance<IInstallationHubClient>(new InstallationHubClient(urlString, installationHubPath));

            return container;
        }

        private static readonly System.Func<IUnityContainer, RetryHttpClient> defaultResolveHttpClientFunction = (IUnityContainer c) => c.Resolve<RetryHttpClient>();

        public static IContainerRegistry RegisterMachineAutomationServices(
            this IContainerRegistry container,
            System.Uri serviceUrl,
            System.Func<IUnityContainer, RetryHttpClient> resolveHttpClientFunction = null)
        {
            var urlString = serviceUrl.ToString();

            var resolveFunction = resolveHttpClientFunction ?? defaultResolveHttpClientFunction;

            container.Register<RetryHttpClient, RetryHttpClient>();

            container.GetContainer().RegisterType<IMachineBaysService>(
                new InjectionFactory(c => new MachineBaysService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineBeltBurnishingProcedureService>(
                new InjectionFactory(c => new MachineBeltBurnishingProcedureService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineCellsService>(
                new InjectionFactory(c => new MachineCellsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineCellPanelsService>(
               new InjectionFactory(c => new MachineCellPanelsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineElevatorService>(
                new InjectionFactory(c => new MachineElevatorService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineCarouselService>(
                new InjectionFactory(c => new MachineCarouselService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineErrorsService>(
                new InjectionFactory(c => new MachineErrorsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineErrorsService>(
                new InjectionFactory(c => new MachineErrorsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineIdentityService>(
                new InjectionFactory(c => new MachineIdentityService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineSetupStatusService>(
                new InjectionFactory(c => new MachineSetupStatusService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineLoadingUnitsService>(
                new InjectionFactory(c => new MachineLoadingUnitsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineMachineStatusService>(
                new InjectionFactory(c => new MachineMachineStatusService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineMissionOperationsService>(
                new InjectionFactory(c => new MachineMissionOperationsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineMissionOperationsService>(
                new InjectionFactory(c => new MachineMissionOperationsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineResolutionCalibrationProcedureService>(
                new InjectionFactory(c => new MachineResolutionCalibrationProcedureService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineSensorsService>(
                new InjectionFactory(c => new MachineSensorsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineShuttersService>(
                new InjectionFactory(c => new MachineShuttersService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineStatisticsService>(
                new InjectionFactory(c => new MachineStatisticsService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineTestService>(
                new InjectionFactory(c => new MachineTestService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineUsersService>(
                new InjectionFactory(c => new MachineUsersService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineVerticalOriginProcedureService>(
                new InjectionFactory(c => new MachineVerticalOriginProcedureService(urlString, resolveFunction(c))));

            container.GetContainer().RegisterType<IMachineVerticalOffsetProcedureService>(
                new InjectionFactory(c => new MachineVerticalOffsetProcedureService(urlString, resolveFunction(c))));

            return container;
        }

        public static IContainerProvider UseMachineAutomationHubs(this IContainerProvider containerProvider)
        {
            if (containerProvider is null)
            {
                throw new System.ArgumentNullException(nameof(containerProvider));
            }

            containerProvider
                .Resolve<IInstallationHubClient>()
                .ConnectAsync();

            containerProvider
                .Resolve<IOperatorHubClient>()
                .ConnectAsync();

            return containerProvider;
        }

    }
}
