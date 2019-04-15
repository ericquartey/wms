using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.Utils.Interfaces;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTNavigationButtonsViewModel : BindableBase, IViewModel, ILSMTNavigationButtonsViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand carouselButtonCommand;

        private IUnityContainer container;

        private ICommand horizontalEngineButtonCommand;

        private ICommand shutterEngineButtonCommand;

        private ICommand verticalEngineButtonCommand;

        #endregion

        #region Constructors

        public LSMTNavigationButtonsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand CarouselButtonCommand => this.carouselButtonCommand ??
            (this.carouselButtonCommand = new DelegateCommand(async () =>
            {
                this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (LSMTCarouselViewModel)this.container.Resolve<ILSMTCarouselViewModel>();
                await (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as LSMTCarouselViewModel).OnEnterViewAsync();
            }
            ));

        public ICommand HorizontalEngineButtonCommand => this.horizontalEngineButtonCommand ??
            (this.horizontalEngineButtonCommand = new DelegateCommand(async () =>
            {
                this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (LSMTHorizontalEngineViewModel)this.container.Resolve<ILSMTHorizontalEngineViewModel>();
                await (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as LSMTHorizontalEngineViewModel).OnEnterViewAsync();
            }
            ));

        public BindableBase NavigationViewModel { get; set; }

        public ICommand ShutterEngineButtonCommand => this.shutterEngineButtonCommand ??
            (this.shutterEngineButtonCommand = new DelegateCommand(async () =>
           {
               this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (LSMTShutterEngineViewModel)this.container.Resolve<ILSMTShutterEngineViewModel>();
               await (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as LSMTShutterEngineViewModel).OnEnterViewAsync();
           }
            ));

        public ICommand VerticalEngineButtonCommand => this.verticalEngineButtonCommand ??
            (this.verticalEngineButtonCommand = new DelegateCommand(async () =>
            {
                this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (LSMTVerticalEngineViewModel)this.container.Resolve<ILSMTVerticalEngineViewModel>();
                await (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as LSMTVerticalEngineViewModel).OnEnterViewAsync();
            }
        ));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
