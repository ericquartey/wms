using Prism.Mvvm;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.Navigation;
using System;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels
{
    internal class MainWindowNavigationButtonsViewModel : BindableBase
    {
        #region Fields

        private bool isBeltBurnishingButtonActive;
        private bool isGateHeightControlButtonActive = true;
        private bool isSetYResolutionButtonActive;
        private bool isWeightControlButtonActive = true;

        #endregion Fields

        #region Constructors

        public MainWindowNavigationButtonsViewModel()
        {
            this.UpdateDataFromDataManager();
            NavigationService.InstallationInfoChangedEventHandler += this.UpdateDataFromDataManager;
        }

        #endregion Constructors

        #region Properties

        public Boolean IsBeltBurnishingButtonActive { get => this.isBeltBurnishingButtonActive; set => this.SetProperty(ref this.isBeltBurnishingButtonActive, value); }
        public Boolean IsGateHeightControlButtonActive { get => this.isGateHeightControlButtonActive; set => this.SetProperty(ref this.isGateHeightControlButtonActive, value); }
        public Boolean IsSetYResolutionButtonActive { get => this.isSetYResolutionButtonActive; set => this.SetProperty(ref this.isSetYResolutionButtonActive, value); }
        public Boolean IsWeightControlButtonActive { get => this.isWeightControlButtonActive; set => this.SetProperty(ref this.isWeightControlButtonActive, value); }

        #endregion Properties

        #region Methods

        private void UpdateDataFromDataManager()
        {
            this.IsBeltBurnishingButtonActive = DataManager.CurrentData.InstallationInfo.Belt_Burnishing;
            this.IsSetYResolutionButtonActive = DataManager.CurrentData.InstallationInfo.Set_Y_Resolution;
        }

        #endregion Methods
    }
}
