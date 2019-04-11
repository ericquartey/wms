﻿using Prism.Mvvm;
using Microsoft.Practices.Unity;
using Prism.Events;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;

namespace Ferretto.VW.InstallationApp
{
    public class MainWindowNavigationButtonsViewModel : BindableBase, IMainWindowNavigationButtonsViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private bool isBeltBurnishingButtonActive;

        private bool isCellsControlButtonActive = true;

        private bool isCellsPanelControlButtonActive = true;

        private bool isDownScrollButtonActive = true;

        private bool isGateControlButtonActive = true;

        private bool isGateHeightControlButtonActive = true;

        private bool isInstallationStateButtonActive = true;

        private bool isLowSpeedMovementsTestButtonActive = true;

        private bool isOriginVerticalAxisButtonActive = true;

        private bool isSensorsStateButtonActive = true;

        private bool isSetYResolutionButtonActive = true;

        private bool isUpScrollButtonActive = true;

        private bool isVerticalOffsetCalibrationButtonActive = true;

        private bool isWeightControlButtonActive = true;

        #endregion

        #region Constructors

        public MainWindowNavigationButtonsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { this.SetAllNavigationButtonDisabled(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.EnterView);

            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { this.UpdateDataFromDataManager(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.ExitView);
        }

        #endregion

        #region Properties

        public bool IsBeltBurnishingButtonActive { get => this.isBeltBurnishingButtonActive; set => this.SetProperty(ref this.isBeltBurnishingButtonActive, value); }

        public bool IsCellsControlButtonActive { get => this.isCellsControlButtonActive; set => this.SetProperty(ref this.isCellsControlButtonActive, value); }

        public bool IsCellsPanelControlButtonActive { get => this.isCellsPanelControlButtonActive; set => this.SetProperty(ref this.isCellsPanelControlButtonActive, value); }

        public bool IsDownScrollButtonActive { get => this.isDownScrollButtonActive; set => this.SetProperty(ref this.isDownScrollButtonActive, value); }

        public bool IsGateControlButtonActive { get => this.isGateControlButtonActive; set => this.SetProperty(ref this.isGateControlButtonActive, value); }

        public bool IsGateHeightControlButtonActive { get => this.isGateHeightControlButtonActive; set => this.SetProperty(ref this.isGateHeightControlButtonActive, value); }

        public bool IsInstallationStateButtonActive { get => this.isInstallationStateButtonActive; set => this.SetProperty(ref this.isInstallationStateButtonActive, value); }

        public bool IsLowSpeedMovementsTestButtonActive { get => this.isLowSpeedMovementsTestButtonActive; set => this.SetProperty(ref this.isLowSpeedMovementsTestButtonActive, value); }

        public bool IsOriginVerticalAxisButtonActive { get => this.isOriginVerticalAxisButtonActive; set => this.SetProperty(ref this.isOriginVerticalAxisButtonActive, value); }

        public bool IsSensorsStateButtonActive { get => this.isSensorsStateButtonActive; set => this.SetProperty(ref this.isSensorsStateButtonActive, value); }

        public bool IsSetYResolutionButtonActive { get => this.isSetYResolutionButtonActive; set => this.SetProperty(ref this.isSetYResolutionButtonActive, value); }

        public bool IsUpScrollButtonActive { get => this.isUpScrollButtonActive; set => this.SetProperty(ref this.isUpScrollButtonActive, value); }

        public bool IsVerticalOffsetCalibrationButtonActive { get => this.isVerticalOffsetCalibrationButtonActive; set => this.SetProperty(ref this.isVerticalOffsetCalibrationButtonActive, value); }

        public bool IsWeightControlButtonActive { get => this.isWeightControlButtonActive; set => this.SetProperty(ref this.isWeightControlButtonActive, value); }

        #endregion

        #region Methods

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.UpdateDataFromDataManager();
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

        public void UpdateDataFromDataManager()
        {
            this.IsInstallationStateButtonActive = true;
            this.IsUpScrollButtonActive = true;
            this.IsDownScrollButtonActive = true;
            this.IsSensorsStateButtonActive = true;
            this.IsLowSpeedMovementsTestButtonActive = true;
            this.IsGateControlButtonActive = true;
            this.IsOriginVerticalAxisButtonActive = true;
            this.IsBeltBurnishingButtonActive = true;
            this.IsSetYResolutionButtonActive = true;
            this.IsGateHeightControlButtonActive = true; // TODO: Reference value missing in InstallationInfo file
            this.IsWeightControlButtonActive = true; // TODO: Reference value missing in InstallationInfo file
            this.IsVerticalOffsetCalibrationButtonActive = true; // TODO: Reference value missing in InstallationInfo file
            this.IsCellsPanelControlButtonActive = true; // TODO: Reference value missing in InstallationInfo file
            this.IsCellsControlButtonActive = true; // TODO: Reference value missing in InstallationInfo file
        }

        #endregion
    }
}
