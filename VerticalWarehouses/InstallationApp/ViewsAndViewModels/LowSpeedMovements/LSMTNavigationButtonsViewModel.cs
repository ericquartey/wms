using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTNavigationButtonsViewModel : BindableBase, IViewModel, ILSMTNavigationButtonsViewModel, IViewModelRequiresContainer
    {
        #region Fields

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

        public ICommand HorizontalEngineButtonCommand => this.horizontalEngineButtonCommand ?? (this.horizontalEngineButtonCommand = new DelegateCommand(() => ((LSMTMainViewModel)this.container.Resolve<ILSMTMainViewModel>()).LSMTContentRegionCurrentViewModel = (LSMTHorizontalEngineViewModel)this.container.Resolve<ILSMTHorizontalEngineViewModel>()));

        public ICommand ShutterEngineButtonCommand => this.shutterEngineButtonCommand ?? (this.shutterEngineButtonCommand = new DelegateCommand(() => ((LSMTMainViewModel)this.container.Resolve<ILSMTMainViewModel>()).LSMTContentRegionCurrentViewModel = (LSMTShutterEngineViewModel)this.container.Resolve<ILSMTShutterEngineViewModel>()));

        public ICommand VerticalEngineButtonCommand => this.verticalEngineButtonCommand ?? (this.verticalEngineButtonCommand = new DelegateCommand(() => ((LSMTMainViewModel)this.container.Resolve<ILSMTMainViewModel>()).LSMTContentRegionCurrentViewModel = (LSMTVerticalEngineViewModel)this.container.Resolve<ILSMTVerticalEngineViewModel>()));

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
