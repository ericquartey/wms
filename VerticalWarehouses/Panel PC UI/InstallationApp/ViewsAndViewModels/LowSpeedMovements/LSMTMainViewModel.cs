using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTMainViewModel : BindableBase, ILSMTMainViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        private BindableBase lSMTContentRegionCurrentViewModel;

        private BindableBase lSMTNavigationRegionCurrentViewModel;

        #endregion

        #region Constructors

        public LSMTMainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.LSMTContentRegionCurrentViewModel = null;
            this.LSMTNavigationRegionCurrentViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase LSMTContentRegionCurrentViewModel { get => this.lSMTContentRegionCurrentViewModel; set => this.SetProperty(ref this.lSMTContentRegionCurrentViewModel, value); }

        public BindableBase LSMTNavigationRegionCurrentViewModel { get => this.lSMTNavigationRegionCurrentViewModel; set => this.SetProperty(ref this.lSMTNavigationRegionCurrentViewModel, value); }

        #endregion

        #region Methods

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.container = _container;
            this.LSMTNavigationRegionCurrentViewModel = (LSMTNavigationButtonsViewModel)this.container.Resolve<ILSMTNavigationButtonsViewModel>();
        }

        #endregion
    }
}
