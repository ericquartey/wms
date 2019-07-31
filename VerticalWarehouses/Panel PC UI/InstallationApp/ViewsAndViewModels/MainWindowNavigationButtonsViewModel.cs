using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Installation.Resources;
using Ferretto.VW.App.Installation.Resources.Enumerables;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels
{
    public class MainWindowNavigationButtonsViewModel : BindableBase, IMainWindowNavigationButtonsViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInstallationStatusMachineService installationStatusService;

        private bool isBayControlButtonActive;

        private bool isBeltBurnishingButtonActive;

        private bool isCellsControlButtonActive;

        private bool isCellsPanelControlButtonActive;

        private bool isCellsSideControlButtonActive;

        private bool isDownScrollButtonActive = true;

        private bool isDrawerLoadingUnloadingTestButtonActive;

        private bool isGateControlButtonActive = true;

        private bool isGateHeightControlButtonActive = true;

        private bool isInstallationStateButtonActive = true;

        private bool isLoadFirstDrawerButtonActive;

        private bool isLoadingDrawersButtonActive;

        private bool isLowSpeedMovementsTestButtonActive = true;

        private bool isManualDrawerStoreRecallButtonActive = true;  // remove assignment

        private bool isOriginVerticalAxisButtonActive = true;

        private bool isSaveRestoreConfigButtonActive = true;

        private bool isSensorsStateButtonActive = true;

        private bool isSetYResolutionButtonActive = true;

        private bool isUpScrollButtonActive = true;

        private bool isVerticalOffsetCalibrationButtonActive;

        private bool isWeightControlButtonActive;

        private BindableBase navigationViewModel;

        #endregion

        #region Constructors

        public MainWindowNavigationButtonsViewModel(
            IEventAggregator eventAggregator,
            IInstallationStatusMachineService installationStatusService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (installationStatusService == null)
            {
                throw new System.ArgumentNullException(nameof(installationStatusService));
            }

            this.eventAggregator = eventAggregator;
            this.installationStatusService = installationStatusService;

            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { this.SetAllNavigationButtonDisabled(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.EnterView);

            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                async (message) => { await this.UpdateButtonsEnableStateAsync(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.ExitView);
        }

        #endregion

        #region Properties

        public bool IsBayControlButtonActive { get => this.isBayControlButtonActive; set => this.SetProperty(ref this.isBayControlButtonActive, value); }

        public bool IsBeltBurnishingButtonActive { get => this.isBeltBurnishingButtonActive; set => this.SetProperty(ref this.isBeltBurnishingButtonActive, value); }

        public bool IsCellsControlButtonActive { get => this.isCellsControlButtonActive; set => this.SetProperty(ref this.isCellsControlButtonActive, value); }

        public bool IsCellsPanelControlButtonActive { get => this.isCellsPanelControlButtonActive; set => this.SetProperty(ref this.isCellsPanelControlButtonActive, value); }

        public bool IsCellsSideControlButtonActive { get => this.isCellsSideControlButtonActive; set => this.SetProperty(ref this.isCellsSideControlButtonActive, value); }

        public bool IsDownScrollButtonActive { get => this.isDownScrollButtonActive; set => this.SetProperty(ref this.isDownScrollButtonActive, value); }

        public bool IsDrawerLoadingUnloadingTestButtonActive
        {
            get => this.isDrawerLoadingUnloadingTestButtonActive;
            set => this.SetProperty(ref this.isDrawerLoadingUnloadingTestButtonActive, value);
        }

        public bool IsGateControlButtonActive { get => this.isGateControlButtonActive; set => this.SetProperty(ref this.isGateControlButtonActive, value); }

        public bool IsGateHeightControlButtonActive { get => this.isGateHeightControlButtonActive; set => this.SetProperty(ref this.isGateHeightControlButtonActive, value); }

        public bool IsInstallationStateButtonActive { get => this.isInstallationStateButtonActive; set => this.SetProperty(ref this.isInstallationStateButtonActive, value); }

        public bool IsLoadFirstDrawerButtonActive { get => this.isLoadFirstDrawerButtonActive; set => this.SetProperty(ref this.isLoadFirstDrawerButtonActive, value); }

        public bool IsLoadingDrawersButtonActive { get => this.isLoadingDrawersButtonActive; set => this.SetProperty(ref this.isLoadingDrawersButtonActive, value); }

        public bool IsLowSpeedMovementsTestButtonActive { get => this.isLowSpeedMovementsTestButtonActive; set => this.SetProperty(ref this.isLowSpeedMovementsTestButtonActive, value); }

        public bool IsManualDrawerStoreRecallButtonActive { get => this.isManualDrawerStoreRecallButtonActive; set => this.SetProperty(ref this.isManualDrawerStoreRecallButtonActive, value); }

        public bool IsOriginVerticalAxisButtonActive { get => this.isOriginVerticalAxisButtonActive; set => this.SetProperty(ref this.isOriginVerticalAxisButtonActive, value); }

        public bool IsSaveRestoreConfigButtonActive { get => this.isSaveRestoreConfigButtonActive; set => this.SetProperty(ref this.isSaveRestoreConfigButtonActive, value); }

        public bool IsSensorsStateButtonActive { get => this.isSensorsStateButtonActive; set => this.SetProperty(ref this.isSensorsStateButtonActive, value); }

        public bool IsSetYResolutionButtonActive { get => this.isSetYResolutionButtonActive; set => this.SetProperty(ref this.isSetYResolutionButtonActive, value); }

        public bool IsUpScrollButtonActive { get => this.isUpScrollButtonActive; set => this.SetProperty(ref this.isUpScrollButtonActive, value); }

        public bool IsVerticalOffsetCalibrationButtonActive { get => this.isVerticalOffsetCalibrationButtonActive; set => this.SetProperty(ref this.isVerticalOffsetCalibrationButtonActive, value); }

        public bool IsWeightControlButtonActive { get => this.isWeightControlButtonActive; set => this.SetProperty(ref this.isWeightControlButtonActive, value); }

        public BindableBase NavigationViewModel
        {
            get => this.navigationViewModel;
            set => this.SetProperty(ref this.navigationViewModel, value);
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
        {
            await this.UpdateButtonsEnableStateAsync();
        }

        public void SetAllNavigationButtonDisabled()
        {
            this.IsBeltBurnishingButtonActive = false;
            this.IsCellsControlButtonActive = false;
            this.IsCellsPanelControlButtonActive = false;
            this.IsGateControlButtonActive = false;
            this.IsGateHeightControlButtonActive = false;
            this.IsInstallationStateButtonActive = false;
            this.IsLowSpeedMovementsTestButtonActive = false;
            this.IsOriginVerticalAxisButtonActive = false;
            this.IsSensorsStateButtonActive = false;
            this.IsSetYResolutionButtonActive = false;
            this.IsVerticalOffsetCalibrationButtonActive = false;
            this.IsWeightControlButtonActive = false;
            this.IsUpScrollButtonActive = false;
            this.IsDownScrollButtonActive = false;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        private async Task UpdateButtonsEnableStateAsync()
        {
            var installationStatus = await this.installationStatusService.GetStatusAsync();
            var checkHomingDone = installationStatus.FirstOrDefault();

            this.IsInstallationStateButtonActive = true;
            this.IsUpScrollButtonActive = true;
            this.IsDownScrollButtonActive = true;
            this.IsSensorsStateButtonActive = true;
            this.IsLowSpeedMovementsTestButtonActive = true;
            this.IsGateControlButtonActive = true;
            this.IsOriginVerticalAxisButtonActive = true;
            this.IsBeltBurnishingButtonActive = checkHomingDone;
            this.IsSetYResolutionButtonActive = checkHomingDone;

            this.IsGateHeightControlButtonActive = true;                     // TODO: Reference value missing in InstallationInfo file
            this.IsWeightControlButtonActive = checkHomingDone;              // TODO: Reference value missing in InstallationInfo file
            this.IsVerticalOffsetCalibrationButtonActive = checkHomingDone;  // TODO: Reference value missing in InstallationInfo file
            this.IsCellsPanelControlButtonActive = checkHomingDone;          // TODO: Reference value missing in InstallationInfo file
            this.IsCellsControlButtonActive = checkHomingDone;               // TODO: Reference value missing in InstallationInfo file
            this.IsCellsSideControlButtonActive = checkHomingDone;
            this.IsBayControlButtonActive = true;
            this.IsDrawerLoadingUnloadingTestButtonActive = checkHomingDone;
            this.IsLoadFirstDrawerButtonActive = checkHomingDone;
            this.IsLoadingDrawersButtonActive = checkHomingDone;
            this.IsSaveRestoreConfigButtonActive = true;
            this.IsManualDrawerStoreRecallButtonActive = true; //checkHomingDone;
        }

        #endregion
    }
}
