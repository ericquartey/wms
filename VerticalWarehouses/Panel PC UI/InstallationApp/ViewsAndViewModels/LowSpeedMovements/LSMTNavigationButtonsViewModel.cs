using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Installation.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements
{
    public class LSMTNavigationButtonsViewModel : BindableBase, ILSMTNavigationButtonsViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand carouselButtonCommand;

        private readonly IUnityContainer container;

        private ICommand horizontalEngineButtonCommand;

        private ICommand shutterEngineButtonCommand;

        private ICommand verticalEngineButtonCommand;

        #endregion

        #region Constructors

        public LSMTNavigationButtonsViewModel(
            IEventAggregator eventAggregator,
            IUnityContainer container) // TODO remove container injection
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (container == null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            this.eventAggregator = eventAggregator;
            this.container = container;
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
