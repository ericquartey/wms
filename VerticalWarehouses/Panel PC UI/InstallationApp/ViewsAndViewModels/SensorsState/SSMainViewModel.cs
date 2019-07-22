namespace Ferretto.VW.InstallationApp
{
    using System.Threading.Tasks;
    using Prism.Events;
    using Prism.Mvvm;
    using Unity;

    public class SSMainViewModel : BindableBase, ISSMainViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private BindableBase sSContentRegionCurrentViewModel;

        private BindableBase sSNavigationRegionCurrentViewModel;

        #endregion

        #region Constructors

        public SSMainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.SSContentRegionCurrentViewModel = null;
            this.SSNavigationRegionCurrentViewModel = null;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        public BindableBase SSContentRegionCurrentViewModel { get => this.sSContentRegionCurrentViewModel; set => this.SetProperty(ref this.sSContentRegionCurrentViewModel, value); }

        public BindableBase SSNavigationRegionCurrentViewModel { get => this.sSNavigationRegionCurrentViewModel; set => this.SetProperty(ref this.sSNavigationRegionCurrentViewModel, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.SSNavigationRegionCurrentViewModel = (SSNavigationButtonsViewModel)this.container.Resolve<ISSNavigationButtonsViewModel>();
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
