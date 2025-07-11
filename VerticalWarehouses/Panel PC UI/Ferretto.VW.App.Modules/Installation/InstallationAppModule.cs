﻿using Ferretto.VW.App.Installation.Views;
using Ferretto.VW.App.Modules.Installation.Views;
using Ferretto.VW.App.Services;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace Ferretto.VW.App.Modules.Installation
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This is a container initialization class, so it is ok to be coupled to many types.")]
    [Module(ModuleName = nameof(Utils.Modules.Installation), OnDemand = true)]
    [ModuleDependency(nameof(Utils.Modules.Errors))]
    public class InstallationAppModule : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public InstallationAppModule(IUnityContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            //containerProvider.UseMachineAutomationHubs();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AlphaNumericBarSettingsView>();
            containerRegistry.RegisterForNavigation<BarcodeReaderSettingsView>();
            containerRegistry.RegisterForNavigation<BarcodeReaderConfigurationView>();
            containerRegistry.RegisterForNavigation<LabelPrinterSettingsView>();
            containerRegistry.RegisterForNavigation<LaserPointerSettingsView>();
            containerRegistry.RegisterForNavigation<CardReaderSettingsView>();
            containerRegistry.RegisterForNavigation<TokenReaderSettingsView>();
            containerRegistry.RegisterForNavigation<WeightingScaleSettingsView>();

            containerRegistry.RegisterForNavigation<BayCheckView>();
            containerRegistry.RegisterForNavigation<DepositAndPickUpTestView>();
            containerRegistry.RegisterForNavigation<CarouselCalibrationView>();
            containerRegistry.RegisterForNavigation<ExternalBayCalibrationView>();
            containerRegistry.RegisterForNavigation<BEDTestView>();

            containerRegistry.RegisterForNavigation<BaysSensorsView>();
            containerRegistry.RegisterForNavigation<Bay1DeviceIOView>();
            containerRegistry.RegisterForNavigation<Bay2DeviceIOView>();
            containerRegistry.RegisterForNavigation<Bay3DeviceIOView>();
            containerRegistry.RegisterForNavigation<VerticalAxisSensorsView>();
            containerRegistry.RegisterForNavigation<OtherSensorsView>();

            containerRegistry.RegisterForNavigation<MovementsView>();

            containerRegistry.RegisterForNavigation<ElevatorWeightAnalysisView>();
            containerRegistry.RegisterForNavigation<ElevatorWeightCheckStep1View>();
            containerRegistry.RegisterForNavigation<ElevatorWeightCheckStep2View>();
            containerRegistry.RegisterForNavigation<VerticalOffsetCalibrationView>();
            containerRegistry.RegisterForNavigation<VerticalOriginCalibrationView>();
            containerRegistry.RegisterForNavigation<VerticalResolutionCalibrationView>();
            containerRegistry.RegisterForNavigation<SaveRestoreConfigView>();
            containerRegistry.RegisterForNavigation<DrawerLoadingUnloadingTestView>();
            containerRegistry.RegisterForNavigation<CellsSideControlView>();

            containerRegistry.RegisterForNavigation<HorizontalChainCalibrationView>();
            containerRegistry.RegisterForNavigation<HorizontalResolutionCalibrationView>();
            containerRegistry.RegisterForNavigation<WeightCalibrationView>();

            containerRegistry.RegisterForNavigation<LoadFirstDrawerView>();
            containerRegistry.RegisterForNavigation<FixBackDrawersView>();
            containerRegistry.RegisterForNavigation<LoadingUnitFromBayToCellView>();
            containerRegistry.RegisterForNavigation<LoadingUnitFromBayToBayView>();
            containerRegistry.RegisterForNavigation<LoadingUnitFromCellToBayView>();
            containerRegistry.RegisterForNavigation<LoadingUnitFromCellToCellView>();
            containerRegistry.RegisterForNavigation<FullTestView>();

            containerRegistry.RegisterForNavigation<ProfileHeightCheckView>();
            containerRegistry.RegisterForNavigation<ProfileResolutionCalibrationView>();

            containerRegistry.RegisterForNavigation<ShutterEnduranceTestView>();
            containerRegistry.RegisterForNavigation<SensitiveAlarmView>();

            containerRegistry.RegisterForNavigation<BeltBurnishingView>();

            containerRegistry.RegisterForNavigation<CellsHeightCheckView>();
            containerRegistry.RegisterForNavigation<CellPanelsCheckView>();
            containerRegistry.RegisterForNavigation<LogoutSettingsView>();

            containerRegistry.RegisterForNavigation<ParametersView>();
            containerRegistry.RegisterForNavigation<ParametersImportView>();
            containerRegistry.RegisterForNavigation<ParametersExportView>();
            containerRegistry.RegisterForNavigation<ParametersImportStep1View>();
            containerRegistry.RegisterForNavigation<ParametersImportStep2View>();

            containerRegistry.RegisterForNavigation<CellsLoadingUnitsMenuView>();
            containerRegistry.RegisterForNavigation<CellsView>();
            containerRegistry.RegisterForNavigation<LoadingUnitsView>();
            containerRegistry.RegisterForNavigation<UpdateStep1View>();
            containerRegistry.RegisterForNavigation<UpdateStep2View>();
            containerRegistry.RegisterForNavigation<UsersView>();
            containerRegistry.RegisterForNavigation<ParameterInverterView>();
            containerRegistry.RegisterForNavigation<ParametersInverterDetailsView>();
            containerRegistry.RegisterForNavigation<ErrorParametersInverterDetailsView>();
            containerRegistry.RegisterForNavigation<ErrorParameterInverterView>();
            containerRegistry.RegisterForNavigation<InvertersParametersImportView>();
            containerRegistry.RegisterForNavigation<InvertersParametersExportView>();
            containerRegistry.RegisterForNavigation<DateTimeView>();
            containerRegistry.RegisterForNavigation<WmsSettingsView>();
            containerRegistry.RegisterForNavigation<DatabaseBackupView>();

            containerRegistry.Register<INavigableView, DevicesView>(nameof(DevicesView));
        }

        #endregion
    }
}
