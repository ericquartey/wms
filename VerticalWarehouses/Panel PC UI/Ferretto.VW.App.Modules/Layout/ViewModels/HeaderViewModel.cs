using System.Linq;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Modules.Login;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    internal sealed class HeaderViewModel : BasePresentationViewModel
    {
        #region Fields

        private readonly IMachineErrorsService machineErrorsService;

        private DevExpress.Mvvm.DelegateCommand goToMenuCommand;

        #endregion

        #region Constructors

        public HeaderViewModel(IMachineErrorsService machineErrorsService)
        {
            this.machineErrorsService = machineErrorsService;
        }

        #endregion

        #region Properties

        public ICommand GoToMenuCommand =>
            this.goToMenuCommand
            ??
            (this.goToMenuCommand = new DelegateCommand(
                this.GoToMenu, this.CanGoToMenu));

        #endregion

        #region Methods

        public override void InitializeData()
        {
            base.InitializeData();
            this.States.Add(this.GetInstance<PresentationTheme>());
            this.States.Add(this.GetInstance<PresentationShutdown>());
            this.States.Add(this.GetInstance<PresentationHelp>());
            this.States.Add(this.GetInstance<PresentationLogged>());
            this.States.Add(this.GetInstance<PresentationMachineModeSwitch>());
            this.States.Add(this.GetInstance<PresentationMachinePowerSwitch>());
            this.States.Add(this.GetInstance<PresentationError>());
            this.States.Add(this.GetInstance<PresentationDebug>());
        }

        public override void UpdateChanges(PresentationChangedMessage presentation)
        {
            base.UpdateChanges(presentation);

            if (presentation.States == null)
            {
                return;
            }

            var actualStates = this.States.Where(s => presentation.States.Any(ps => ps.Type == s.Type));

            foreach (var state in actualStates)
            {
                var presentationState = presentation.States.Single(s => s.Type == state.Type);

                // Succede che se mentre stai effettuando il logout arriva un messaggio per far visualizzare l'errore allora viene mostrato comunque il pulsante di errore
                if (this.CurrentPresentation == PresentationMode.Login &&
                    state.Type == PresentationTypes.Error)
                {
                    state.IsVisible = false;
                }
                else
                {
                    state.IsVisible = presentationState.IsVisible;
                }

                state.IsEnabled = presentationState.IsEnabled;
            }
        }

        public override void UpdatePresentation(PresentationMode mode)
        {
            if (mode == PresentationMode.None ||
                this.CurrentPresentation == mode)
            {
                return;
            }

            this.CurrentPresentation = mode;
            switch (mode)
            {
                case PresentationMode.Login:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Error, false);
                    this.Show(PresentationTypes.Help, false);
                    this.Show(PresentationTypes.Logged, false);
                    this.Show(PresentationTypes.MachineMode, false);
                    this.Show(PresentationTypes.MachineMarch, false);
                    this.Show(PresentationTypes.Theme, false);
// #if DEBUG
                    this.Show(PresentationTypes.Shutdown, true);
// #endif
                    break;

                case PresentationMode.Menu:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Help, true);
                    this.Show(PresentationTypes.Logged, true);
                    this.Show(PresentationTypes.MachineMode, true);
                    this.Show(PresentationTypes.MachineMarch, true);
                    this.Show(PresentationTypes.Error, this.machineErrorsService.ActiveError != null);
                    break;

                case PresentationMode.Installer:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Help, true);
                    this.Show(PresentationTypes.Logged, true);
                    this.Show(PresentationTypes.MachineMode, true);
                    this.Show(PresentationTypes.MachineMarch, true);
                    this.Show(PresentationTypes.Error, this.machineErrorsService.ActiveError != null);
                    // this.Show(PresentationTypes.Debug, true);

                    break;

                case PresentationMode.Operator:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Help, true);
                    this.Show(PresentationTypes.Logged, true);
                    this.Show(PresentationTypes.MachineMode, true);
                    this.Show(PresentationTypes.MachineMarch, true);
                    this.Show(PresentationTypes.Error, this.machineErrorsService.ActiveError != null);
                    break;

                case PresentationMode.Help:
                    break;
            }
        }

        private bool CanGoToMenu()
        {
            return ScaffolderUserAccesLevel.IsLogged;
        }

        private void GoToMenu()
        {
            this.NavigationService.Appear(
                    nameof(Utils.Modules.Menu),
                    Utils.Modules.Menu.MAIN_MENU,
                    data: this.Data,
                    trackCurrentView: true);
        }

        #endregion
    }
}
