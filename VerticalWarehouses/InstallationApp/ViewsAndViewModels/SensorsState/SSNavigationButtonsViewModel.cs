using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSNavigationButtonsViewModel : BindableBase, IViewModel
    {
        #region Fields

        private ICommand baysButtonCommand;
        private ICommand cradleButtonCommand;
        private ICommand gateButtonCommand;
        private ICommand variousButtonCommand;
        private ICommand verticalButtonCommand;

        #endregion Fields

        #region Properties

        public ICommand BaysButtonCommand => this.baysButtonCommand ?? (this.baysButtonCommand = new DelegateCommand(() => ViewModels.SSMainVMInstance.SSContentRegionCurrentViewModel = ViewModels.SSBaysVMInstance));

        public ICommand CradleButtonCommand => this.cradleButtonCommand ?? (this.cradleButtonCommand = new DelegateCommand(() => ViewModels.SSMainVMInstance.SSContentRegionCurrentViewModel = ViewModels.SSCradleVMInstance));

        public ICommand GateButtonCommand => this.gateButtonCommand ?? (this.gateButtonCommand = new DelegateCommand(() => ViewModels.SSMainVMInstance.SSContentRegionCurrentViewModel = ViewModels.SSGateVMInstance));

        public ICommand VariousButtonCommand => this.variousButtonCommand ?? (this.variousButtonCommand = new DelegateCommand(() => ViewModels.SSMainVMInstance.SSContentRegionCurrentViewModel = ViewModels.SSVariousInputsVMInstance));

        public ICommand VerticalButtonCommand => this.verticalButtonCommand ?? (this.verticalButtonCommand = new DelegateCommand(() => ViewModels.SSMainVMInstance.SSContentRegionCurrentViewModel = ViewModels.SSVerticalAxisVMInstance));

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
