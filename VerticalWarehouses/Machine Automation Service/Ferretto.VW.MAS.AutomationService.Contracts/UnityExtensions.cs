using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Ioc;
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

        public static IContainerRegistry RegisterMachineAutomationServices(
            this IContainerRegistry container,
            System.Uri serviceUrl)
        {
            var urlString = serviceUrl.ToString();

            container.RegisterInstance<IMachineBaysService>(new MachineBaysService(urlString));
            container.RegisterInstance<IMachineBeltBurnishingProcedureService>(new MachineBeltBurnishingProcedureService(urlString));
            container.RegisterInstance<IMachineCarouselService>(new MachineCarouselService(urlString));
            container.RegisterInstance<IMachineCellsService>(new MachineCellsService(urlString));
            container.RegisterInstance<IMachineElevatorService>(new MachineElevatorService(urlString));
            container.RegisterInstance<IMachineErrorsService>(new MachineErrorsService(urlString));
            container.RegisterInstance<IMachineErrorsService>(new MachineErrorsService(urlString));
            container.RegisterInstance<IMachineIdentityService>(new MachineIdentityService(urlString));
            container.RegisterInstance<IMachineSetupStatusService>(new MachineSetupStatusService(urlString));
            container.RegisterInstance<IMachineLoadingUnitsService>(new MachineLoadingUnitsService(urlString));
            container.RegisterInstance<IMachineMachineStatusService>(new MachineMachineStatusService(urlString));
            container.RegisterInstance<IMachineMissionOperationsService>(new MachineMissionOperationsService(urlString));
            container.RegisterInstance<IMachineMissionOperationsService>(new MachineMissionOperationsService(urlString));
            container.RegisterInstance<IMachineResolutionCalibrationProcedureService>(new MachineResolutionCalibrationProcedureService(urlString));
            container.RegisterInstance<IMachineSensorsService>(new MachineSensorsService(urlString));
            container.RegisterInstance<IMachineShuttersService>(new MachineShuttersService(urlString));
            container.RegisterInstance<IMachineStatisticsService>(new MachineStatisticsService(urlString));
            container.RegisterInstance<IMachineTestService>(new MachineTestService(urlString));
            container.RegisterInstance<IMachineUsersService>(new MachineUsersService(urlString));
            container.RegisterInstance<IMachineVerticalOriginProcedureService>(new MachineVerticalOriginProcedureService(urlString));
            container.RegisterInstance<IMachineVerticalOffsetProcedureService>(new MachineVerticalOffsetProcedureService(urlString));

            return container;
        }



    }
}
