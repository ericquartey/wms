using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.Utils.Interfaces;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTNavigationButtonsViewModel : BindableBase, IViewModel, ILSMTNavigationButtonsViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private ICommand carouselButtonCommand;

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        private ICommand horizontalEngineButtonCommand;

        private ICommand shutterEngineButtonCommand;

        private ICommand verticalEngineButtonCommand;

        #endregion

        #region Constructors

        public LSMTNavigationButtonsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public ICommand CarouselButtonCommand => this.carouselButtonCommand ??
            (this.carouselButtonCommand = new DelegateCommand(() =>
            {
                this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (LSMTCarouselViewModel)this.container.Resolve<ILSMTCarouselViewModel>();
                (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as LSMTCarouselViewModel).SubscribeMethodToEvent();
            }
            ));

        public ICommand HorizontalEngineButtonCommand => this.horizontalEngineButtonCommand ??
            (this.horizontalEngineButtonCommand = new DelegateCommand(() =>
            {
                this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (LSMTHorizontalEngineViewModel)this.container.Resolve<ILSMTHorizontalEngineViewModel>();
                (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as LSMTHorizontalEngineViewModel).SubscribeMethodToEvent();
            }
            ));

        public ICommand ShutterEngineButtonCommand => this.shutterEngineButtonCommand ??
            (this.shutterEngineButtonCommand = new DelegateCommand(() =>
            {
                this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (LSMTShutterEngineViewModel)this.container.Resolve<ILSMTShutterEngineViewModel>();
                (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as LSMTShutterEngineViewModel).SubscribeMethodToEvent();
            }
            ));

        public ICommand VerticalEngineButtonCommand => this.verticalEngineButtonCommand ??
            (this.verticalEngineButtonCommand = new DelegateCommand(() =>
            {
                this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel = (LSMTVerticalEngineViewModel)this.container.Resolve<ILSMTVerticalEngineViewModel>();
                (this.container.Resolve<ILSMTMainViewModel>().LSMTContentRegionCurrentViewModel as LSMTVerticalEngineViewModel).SubscribeMethodToEvent();
            }
        ));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.container = _container;
        }

        public void SubscribeMethodToEvent()
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
