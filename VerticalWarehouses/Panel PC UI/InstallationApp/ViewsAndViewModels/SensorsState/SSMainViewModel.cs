using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSMainViewModel : BindableBase, ISSMainViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        private BindableBase sSContentRegionCurrentViewModel;

        private BindableBase sSNavigationRegionCurrentViewModel;

        #endregion

        #region Constructors

        public SSMainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.SSContentRegionCurrentViewModel = null;
            this.SSNavigationRegionCurrentViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase SSContentRegionCurrentViewModel { get => this.sSContentRegionCurrentViewModel; set => this.SetProperty(ref this.sSContentRegionCurrentViewModel, value); }

        public BindableBase SSNavigationRegionCurrentViewModel { get => this.sSNavigationRegionCurrentViewModel; set => this.SetProperty(ref this.sSNavigationRegionCurrentViewModel, value); }

        #endregion

        #region Methods

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.container = _container;
            this.SSNavigationRegionCurrentViewModel = (SSNavigationButtonsViewModel)this.container.Resolve<ISSNavigationButtonsViewModel>();
        }

        #endregion
    }
}
