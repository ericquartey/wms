using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTNavigationButtonsViewModel : BindableBase, IViewModel, ILSMTNavigationButtonsViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private IUnityContainer container;

        private ICommand gateEngineButtonCommand;

        private ICommand horizontalEngineButtonCommand;

        private ICommand verticalEngineButtonCommand;

        #endregion

        #region Properties

        public ICommand GateEngineButtonCommand => this.gateEngineButtonCommand ?? (this.gateEngineButtonCommand = new DelegateCommand(() => ((LSMTMainViewModel)this.container.Resolve<ILSMTMainViewModel>()).LSMTContentRegionCurrentViewModel = (LSMTGateEngineViewModel)this.container.Resolve<ILSMTGateEngineViewModel>()));

        public ICommand HorizontalEngineButtonCommand => this.horizontalEngineButtonCommand ?? (this.horizontalEngineButtonCommand = new DelegateCommand(() => ((LSMTMainViewModel)this.container.Resolve<ILSMTMainViewModel>()).LSMTContentRegionCurrentViewModel = (LSMTHorizontalEngineViewModel)this.container.Resolve<ILSMTHorizontalEngineViewModel>()));

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
