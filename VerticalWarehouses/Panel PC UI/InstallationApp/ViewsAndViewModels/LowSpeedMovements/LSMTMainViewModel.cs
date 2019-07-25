using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements
{
    public class LSMTMainViewModel : BindableBase, ILSMTMainViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private BindableBase lSMTContentRegionCurrentViewModel;

        private BindableBase lSMTNavigationRegionCurrentViewModel;

        #endregion

        #region Constructors

        public LSMTMainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.LSMTContentRegionCurrentViewModel = null;
            this.LSMTNavigationRegionCurrentViewModel = null;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase LSMTContentRegionCurrentViewModel { get => this.lSMTContentRegionCurrentViewModel; set => this.SetProperty(ref this.lSMTContentRegionCurrentViewModel, value); }

        public BindableBase LSMTNavigationRegionCurrentViewModel { get => this.lSMTNavigationRegionCurrentViewModel; set => this.SetProperty(ref this.lSMTNavigationRegionCurrentViewModel, value); }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.LSMTNavigationRegionCurrentViewModel = (LSMTNavigationButtonsViewModel)this.container.Resolve<ILSMTNavigationButtonsViewModel>();
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
