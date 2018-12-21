using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class MainWindowBackToIAPPButtonViewModel : BindableBase
    {
        #region Fields

        private bool isBackButtonActive = true;
        private bool isCancelButtonActive;

        #endregion Fields

        #region Properties

        public CompositeCommand BackButtonCommand { get; set; }

        public CompositeCommand CancelButtonCommand { get; set; }

        public Boolean IsBackButtonActive { get => this.isBackButtonActive; set => this.SetProperty(ref this.isBackButtonActive, value); }

        public Boolean IsCancelButtonActive { get => this.isCancelButtonActive; set => this.SetProperty(ref this.isCancelButtonActive, value); }

        #endregion Properties

        #region Methods

        public void FinalizeBottomButtons()
        {
            this.BackButtonCommand = null;
        }

        public void InitializeLowSpeedMovementsViewBottomButtons() => this.InitializeMultiViewBottomButtons(ViewModels.MainWindowVMInstance.LowSpeedMovementsTestButtonCommand);

        public void InitializeMultiViewBottomButtons(ICommand command)
        {
            this.BackButtonCommand = new CompositeCommand();
            this.BackButtonCommand.RegisterCommand(command);
        }

        public void InitializeSensorsStatesViewBottomButtons() => this.InitializeMultiViewBottomButtons(ViewModels.MainWindowVMInstance.SSNavigationButtonsButtonCommand);

        public void InitializeSingleViewBottomButtons()
        {
            this.BackButtonCommand = new CompositeCommand();
            this.BackButtonCommand.RegisterCommand(ViewModels.MainWindowVMInstance.BackToMainWindowNavigationButtonsViewButtonCommand);
        }

        #endregion Methods
    }
}
