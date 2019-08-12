using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Ioc;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public static class UnityExtensions
    {
        #region Methods

        public static IUnityContainer RegisterMachineAutomationHubs( // TODO remove this
            this IUnityContainer container,
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

        public static IUnityContainer RegisterMachineAutomationServices(// TODO remove this
            this IUnityContainer container,
            System.Uri serviceUrl)
        {
            var urlString = serviceUrl.ToString();

            container.RegisterType<IMachineIdentityService>(
                new InjectionFactory(c => new MachineIdentityService(urlString)));

            container.RegisterType<IMachineUsersService>(
                new InjectionFactory(c => new MachineUsersService(urlString)));

            container.RegisterType<IMachineErrorsService>(
                new InjectionFactory(c => new MachineErrorsService(urlString)));

            container.RegisterType<IMachineMissionOperationsService>(
                new InjectionFactory(c => new MachineMissionOperationsService(urlString)));

            container.RegisterType<IMachineHomingService>(
                new InjectionFactory(c => new MachineHomingService(urlString)));

            container.RegisterType<IMachineElevatorService>(
                new InjectionFactory(c => new MachineElevatorService(urlString)));

            container.RegisterType<IMachineBeltBurnishingService>(
                new InjectionFactory(c => new MachineBeltBurnishingService(urlString)));

            container.RegisterType<IMachineShutterService>(
                new InjectionFactory(c => new MachineShutterService(urlString)));

            container.RegisterType<IMachineResolutionCalibrationService>(
                new InjectionFactory(c => new MachineResolutionCalibrationService(urlString)));

            container.RegisterType<IMachineVerticalOffsetService>(
                new InjectionFactory(c => new MachineVerticalOffsetService(urlString)));

            container.RegisterType<IMachineInstallationStatusService>(
                new InjectionFactory(c => new MachineInstallationStatusService(urlString)));

            container.RegisterType<IMachineSensorsService>(
                new InjectionFactory(c => new MachineSensorsService(urlString)));

            container.RegisterType<IMachineTestService>(
                new InjectionFactory(c => new MachineTestService(urlString)));

            container.RegisterType<IMachineMachineStatusService>(
                new InjectionFactory(c => new MachineMachineStatusService(urlString)));

            container.RegisterType<IMachineMissionOperationsService>(
                new InjectionFactory(c => new MachineMissionOperationsService(urlString)));

            container.RegisterType<IMachineLoadingUnitsService>(
                new InjectionFactory(c => new MachineLoadingUnitsService(urlString)));

            container.RegisterType<IMachineCellsService>(
                new InjectionFactory(c => new MachineCellsService(urlString)));

            container.RegisterType<IMachineErrorsService>(
                new InjectionFactory(c => new MachineErrorsService(urlString)));

            container.RegisterType<IMachineBaysService>(
                new InjectionFactory(c => new MachineBaysService(urlString)));

            container.RegisterType<IMachineStatisticsService>(
                new InjectionFactory(c => new MachineStatisticsService(urlString)));

            container.RegisterType<IMachineMoveDrawerService>(
                new InjectionFactory(c => new MachineMoveDrawerService(urlString)));

            return container;
        }

        #endregion

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

        public static IContainerRegistry RegisterMachineAutomationServices(
            this IContainerRegistry container,
            System.Uri serviceUrl)
        {
            var urlString = serviceUrl.ToString();

            container.RegisterInstance<IMachineBaysService>(new MachineBaysService(urlString));
            container.RegisterInstance<IMachineBeltBurnishingService>(new MachineBeltBurnishingService(urlString));
            container.RegisterInstance<IMachineCarouselService>(new MachineCarouselService(urlString));
            container.RegisterInstance<IMachineCellsService>(new MachineCellsService(urlString));
            container.RegisterInstance<IMachineElevatorService>(new MachineElevatorService(urlString));
            container.RegisterInstance<IMachineErrorsService>(new MachineErrorsService(urlString));
            container.RegisterInstance<IMachineErrorsService>(new MachineErrorsService(urlString));
            container.RegisterInstance<IMachineHomingService>(new MachineHomingService(urlString));
            container.RegisterInstance<IMachineIdentityService>(new MachineIdentityService(urlString));
            container.RegisterInstance<IMachineInstallationStatusService>(new MachineInstallationStatusService(urlString));
            container.RegisterInstance<IMachineLoadingUnitsService>(new MachineLoadingUnitsService(urlString));
            container.RegisterInstance<IMachineMachineStatusService>(new MachineMachineStatusService(urlString));
            container.RegisterInstance<IMachineMissionOperationsService>(new MachineMissionOperationsService(urlString));
            container.RegisterInstance<IMachineMissionOperationsService>(new MachineMissionOperationsService(urlString));
            container.RegisterInstance<IMachineMoveDrawerService>(new MachineMoveDrawerService(urlString));
            container.RegisterInstance<IMachineResolutionCalibrationService>(new MachineResolutionCalibrationService(urlString));
            container.RegisterInstance<IMachineSensorsService>(new MachineSensorsService(urlString));
            container.RegisterInstance<IMachineShutterService>(new MachineShutterService(urlString));
            container.RegisterInstance<IMachineStatisticsService>(new MachineStatisticsService(urlString));
            container.RegisterInstance<IMachineTestService>(new MachineTestService(urlString));
            container.RegisterInstance<IMachineUsersService>(new MachineUsersService(urlString));
            container.RegisterInstance<IMachineVerticalOffsetService>(new MachineVerticalOffsetService(urlString));

            return container;
        }



    }
}
