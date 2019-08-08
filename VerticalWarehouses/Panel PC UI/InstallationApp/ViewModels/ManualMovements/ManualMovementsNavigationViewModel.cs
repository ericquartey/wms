using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ManualMovementsNavigationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly ICommand carouselButtonCommand;

        private readonly ICommand horizontalEngineButtonCommand;

        private readonly ICommand shutterEngineButtonCommand;

        private readonly ICommand verticalEngineButtonCommand;

        #endregion

        #region Constructors

        public ManualMovementsNavigationViewModel(
            IUnityContainer container) // TODO remove container injection
            : base(Services.PresentationMode.Installator)
        {
        }

        #endregion

        /*
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
                        this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (HorizontalEngineManualMovementsViewModel)this.container.Resolve<ILSMTHorizontalEngineViewModel>();
                        await (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as HorizontalEngineManualMovementsViewModel).OnEnterViewAsync();
                    }
                    ));
                    */

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        /*
        public ICommand ShutterEngineButtonCommand => this.shutterEngineButtonCommand ??
            (this.shutterEngineButtonCommand = new DelegateCommand(async () =>
           {
               this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (ShutterEngineManualMovementsViewModel)this.container.Resolve<ILSMTShutterEngineViewModel>();
               await (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as ShutterEngineManualMovementsViewModel).OnEnterViewAsync();
           }
            ));

        public ICommand VerticalEngineButtonCommand => this.verticalEngineButtonCommand ??
            (this.verticalEngineButtonCommand = new DelegateCommand(async () =>
            {
                this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (VerticalEngineManualMovementsViewModel)this.container.Resolve<ILSMTVerticalEngineViewModel>();
                await (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as VerticalEngineManualMovementsViewModel).OnEnterViewAsync();
            }
        ));
        */

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
