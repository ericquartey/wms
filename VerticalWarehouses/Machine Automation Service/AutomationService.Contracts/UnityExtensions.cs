using System.Net.Http;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public static class UnityExtensions
    {
        #region Methods

        public static IUnityContainer RegisterMachineAutomationHubs(
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

        public static IUnityContainer RegisterMachineAutomationServices(
            this IUnityContainer container,
            System.Uri serviceUrl)
        {
            var urlString = serviceUrl.ToString();

            container.RegisterType<IIdentityMachineService>(
                new InjectionFactory(c => new IdentityMachineService(urlString)));

            container.RegisterType<IUsersMachineService>(
                new InjectionFactory(c => new UsersMachineService(urlString)));

            container.RegisterType<IErrorsMachineService>(
                new InjectionFactory(c => new ErrorsMachineService(urlString)));

            container.RegisterType<IMissionOperationsMachineService>(
                new InjectionFactory(c => new MissionOperationsMachineService(urlString)));

            container.RegisterType<IHomingMachineService>(
                new InjectionFactory(c => new HomingMachineService(urlString)));

            container.RegisterType<IPositioningMachineService>(
                new InjectionFactory(c => new PositioningMachineService(urlString)));

            container.RegisterType<IBeltBurnishingMachineService>(
                new InjectionFactory(c => new BeltBurnishingMachineService(urlString)));

            container.RegisterType<IShutterMachineService>(
                new InjectionFactory(c => new ShutterMachineService(urlString)));

            container.RegisterType<IResolutionCalibrationMachineService>(
                new InjectionFactory(c => new ResolutionCalibrationMachineService(urlString)));

            container.RegisterType<IOffsetCalibrationMachineService>(
                new InjectionFactory(c => new OffsetCalibrationMachineService(urlString)));

            container.RegisterType<IInstallationStatusMachineService>(
                new InjectionFactory(c => new InstallationStatusMachineService(urlString)));

            container.RegisterType<IUpdateSensorsMachineService>(
                new InjectionFactory(c => new UpdateSensorsMachineService(urlString)));

            container.RegisterType<ITestMachineService>(
                new InjectionFactory(c => new TestMachineService(urlString)));

            container.RegisterType<IMachineStatusMachineService>(
                new InjectionFactory(c => new MachineStatusMachineService(urlString)));

            container.RegisterType<IMissionOperationsMachineService>(
                new InjectionFactory(c => new MissionOperationsMachineService(urlString)));

            container.RegisterType<ILoadingUnitsMachineService>(
                new InjectionFactory(c => new LoadingUnitsMachineService(urlString)));

            container.RegisterType<ICellsMachineService>(
                new InjectionFactory(c => new CellsMachineService(urlString)));

            container.RegisterType<IErrorsMachineService>(
                new InjectionFactory(c => new ErrorsMachineService(urlString)));

            container.RegisterType<IBaysMachineService>(
                new InjectionFactory(c => new BaysMachineService(urlString)));

            container.RegisterType<IStatisticsMachineService>(
                new InjectionFactory(c => new StatisticsMachineService(urlString)));

            return container;
        }

        #endregion
    }
}
