using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTNavigationButtonsViewModel : BindableBase, IViewModel
    {
        #region Fields

        private ICommand gateEngineButtonCommand;
        private ICommand horizontalEngineButtonCommand;
        private ICommand verticalEngineButtonCommand;

        #endregion Fields

        #region Properties

        public ICommand GateEngineButtonCommand => this.gateEngineButtonCommand ?? (this.gateEngineButtonCommand = new DelegateCommand(() => ViewModels.LSMTMainVMInstance.LSMTContentRegionCurrentViewModel = ViewModels.LSMTGateEngineVMInstance));

        public ICommand HorizontalEngineButtonCommand => this.horizontalEngineButtonCommand ?? (this.horizontalEngineButtonCommand = new DelegateCommand(() => ViewModels.LSMTMainVMInstance.LSMTContentRegionCurrentViewModel = ViewModels.LSMTHorizontalEngineVMInstance));

        public ICommand VerticalEngineButtonCommand => this.verticalEngineButtonCommand ?? (this.verticalEngineButtonCommand = new DelegateCommand(() => ViewModels.LSMTMainVMInstance.LSMTContentRegionCurrentViewModel = ViewModels.LSMTVerticalEngineVMInstance));

        #endregion Properties

        #region Methods

        public void ExitFromViewMethod()
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeMethodToEvent()
        {
            throw new System.NotImplementedException();
        }

        public void UnSubscribeMethodFromEvent()
        {
            throw new System.NotImplementedException();
        }

        #endregion Methods
    }
}
