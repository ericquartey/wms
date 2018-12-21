using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSMainViewModel : BindableBase
    {
        #region Fields

        private BindableBase sSContentRegionCurrentViewModel;
        private BindableBase sSNavigationRegionCurrentViewModel;

        #endregion Fields

        #region Constructors

        public SSMainViewModel()
        {
            this.SSContentRegionCurrentViewModel = null;
            this.SSNavigationRegionCurrentViewModel = ViewModels.SSNavigationButtonsVMInstance;
        }

        #endregion Constructors

        #region Properties

        public BindableBase SSContentRegionCurrentViewModel { get => this.sSContentRegionCurrentViewModel; set => this.SetProperty(ref this.sSContentRegionCurrentViewModel, value); }

        public BindableBase SSNavigationRegionCurrentViewModel { get => this.sSNavigationRegionCurrentViewModel; set => this.SetProperty(ref this.sSNavigationRegionCurrentViewModel, value); }

        #endregion Properties
    }
}
