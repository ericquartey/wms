using Prism.Mvvm;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.Navigation;
using System;

namespace Ferretto.VW.InstallationApp
{
    public class MainWindowNavigationButtonsViewModel : BindableBase
    {
        #region Fields

        private bool isBeltBurnishingButtonActive;
        private bool isCellsControlButtonActive;
        private bool isCellsPanelControlButtonActive;
        private bool isDownScrollButtonActive = true;
        private bool isGateControlButtonActive = true;
        private bool isGateHeightControlButtonActive = true;
        private bool isInstallationStateButtonActive = true;
        private bool isLowSpeedMovementsTestButtonActive = true;
        private bool isOriginVerticalAxisButtonActive = true;
        private bool isSensorsStateButtonActive = true;
        private bool isSetYResolutionButtonActive;
        private bool isUpScrollButtonActive = true;
        private bool isVerticalOffsetCalibrationButtonActive = true;
        private bool isWeightControlButtonActive = true;

        #endregion Fields

        #region Constructors

        public MainWindowNavigationButtonsViewModel()
        {
            this.UpdateDataFromDataManager();
            NavigationService.ExitViewEventHandler += this.UpdateDataFromDataManager;
            NavigationService.GoToViewEventHandler += this.SetAllNavigationButtonDisabled;
            NavigationService.ExitViewEventHandler += this.UpdateDataFromDataManager;
        }

        #endregion Constructors

        #region Properties

        public Boolean IsBeltBurnishingButtonActive { get => this.isBeltBurnishingButtonActive; set => this.SetProperty(ref this.isBeltBurnishingButtonActive, value); }

        public Boolean IsCellsControlButtonActive { get => this.isCellsControlButtonActive; set => this.SetProperty(ref this.isCellsControlButtonActive, value); }

        public Boolean IsCellsPanelControlButtonActive { get => this.isCellsPanelControlButtonActive; set => this.SetProperty(ref this.isCellsPanelControlButtonActive, value); }

        public Boolean IsDownScrollButtonActive { get => this.isDownScrollButtonActive; set => this.SetProperty(ref this.isDownScrollButtonActive, value); }

        public Boolean IsGateControlButtonActive { get => this.isGateControlButtonActive; set => this.SetProperty(ref this.isGateControlButtonActive, value); }

        public Boolean IsGateHeightControlButtonActive { get => this.isGateHeightControlButtonActive; set => this.SetProperty(ref this.isGateHeightControlButtonActive, value); }

        public Boolean IsInstallationStateButtonActive { get => this.isInstallationStateButtonActive; set => this.SetProperty(ref this.isInstallationStateButtonActive, value); }

        public Boolean IsLowSpeedMovementsTestButtonActive { get => this.isLowSpeedMovementsTestButtonActive; set => this.SetProperty(ref this.isLowSpeedMovementsTestButtonActive, value); }

        public Boolean IsOriginVerticalAxisButtonActive { get => this.isOriginVerticalAxisButtonActive; set => this.SetProperty(ref this.isOriginVerticalAxisButtonActive, value); }

        public Boolean IsSensorsStateButtonActive { get => this.isSensorsStateButtonActive; set => this.SetProperty(ref this.isSensorsStateButtonActive, value); }

        public Boolean IsSetYResolutionButtonActive { get => this.isSetYResolutionButtonActive; set => this.SetProperty(ref this.isSetYResolutionButtonActive, value); }

        public Boolean IsUpScrollButtonActive { get => this.isUpScrollButtonActive; set => this.SetProperty(ref this.isUpScrollButtonActive, value); }

        public Boolean IsVerticalOffsetCalibrationButtonActive { get => this.isVerticalOffsetCalibrationButtonActive; set => this.SetProperty(ref this.isVerticalOffsetCalibrationButtonActive, value); }

        public Boolean IsWeightControlButtonActive { get => this.isWeightControlButtonActive; set => this.SetProperty(ref this.isWeightControlButtonActive, value); }

        #endregion Properties

        #region Methods

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

        public void UpdateDataFromDataManager()
        {
            this.IsInstallationStateButtonActive = true;
            this.IsUpScrollButtonActive = true;
            this.IsDownScrollButtonActive = true;
            this.IsSensorsStateButtonActive = true;
            this.IsLowSpeedMovementsTestButtonActive = true;
            this.IsGateControlButtonActive = true;
            this.IsOriginVerticalAxisButtonActive = true;
            this.IsBeltBurnishingButtonActive = DataManager.CurrentData.InstallationInfo.Belt_Burnishing;
            this.IsSetYResolutionButtonActive = DataManager.CurrentData.InstallationInfo.Set_Y_Resolution;
            this.IsGateHeightControlButtonActive = true; //TODO: Reference value missing in InstallationInfo file
            this.IsWeightControlButtonActive = true; //TODO: Reference value missing in InstallationInfo file
            this.IsVerticalOffsetCalibrationButtonActive = true; //TODO: Reference value missing in InstallationInfo file
            this.IsCellsPanelControlButtonActive = true; //TODO: Reference value missing in InstallationInfo file
            this.IsCellsControlButtonActive = true; //TODO: Reference value missing in InstallationInfo file
        }

        #endregion Methods
    }
}
