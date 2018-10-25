using Prism.Mvvm;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels
{
    internal class MainWindowNavigationButtonsViewModel : BindableBase
    {
        #region Fields

        private bool isBeltBurnishingButtonActive;
        private bool isSetYResolutionButtonActive;

        #endregion Fields

        #region Constructors

        public MainWindowNavigationButtonsViewModel()
        {
            this.IsBeltBurnishingButtonActive = DataManager.InstallationInfo.Belt_Burnishing;
            this.IsSetYResolutionButtonActive = DataManager.InstallationInfo.Set_Y_Resolution;
        }

        #endregion Constructors

        #region Properties

        public System.Boolean IsBeltBurnishingButtonActive { get => this.isBeltBurnishingButtonActive; set => this.SetProperty(ref this.isBeltBurnishingButtonActive, value); }
        public System.Boolean IsSetYResolutionButtonActive { get => this.isSetYResolutionButtonActive; set => this.SetProperty(ref this.isSetYResolutionButtonActive, value); }

        #endregion Properties
    }
}
