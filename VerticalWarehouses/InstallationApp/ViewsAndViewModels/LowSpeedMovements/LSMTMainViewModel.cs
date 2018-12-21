using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTMainViewModel : BindableBase
    {
        #region Fields

        private BindableBase lSMTContentRegionCurrentViewModel;
        private BindableBase lSMTNavigationRegionCurrentViewModel;

        #endregion Fields

        #region Constructors

        public LSMTMainViewModel()
        {
            this.LSMTContentRegionCurrentViewModel = null;
            this.LSMTNavigationRegionCurrentViewModel = ViewModels.LSMTNavigationButtonsVMInstance;
        }

        #endregion Constructors

        #region Properties

        public BindableBase LSMTContentRegionCurrentViewModel { get => this.lSMTContentRegionCurrentViewModel; set => this.SetProperty(ref this.lSMTContentRegionCurrentViewModel, value); }

        public BindableBase LSMTNavigationRegionCurrentViewModel { get => this.lSMTNavigationRegionCurrentViewModel; set => this.SetProperty(ref this.lSMTNavigationRegionCurrentViewModel, value); }

        #endregion Properties
    }
}
