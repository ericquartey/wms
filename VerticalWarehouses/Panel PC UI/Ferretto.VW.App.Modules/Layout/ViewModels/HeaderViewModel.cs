using System.Linq;
using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    internal sealed class HeaderViewModel : BasePresentationViewModel
    {
        #region Fields

        private readonly IMachineErrorsService machineErrorsService;

        #endregion

        #region Constructors

        public HeaderViewModel(IMachineErrorsService machineErrorsService)
        {
            this.machineErrorsService = machineErrorsService;
        }

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

                state.IsVisible = presentationState.IsVisible;
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
                    this.Show(PresentationTypes.Theme, true);
                    this.Show(PresentationTypes.Shutdown, true);
                    break;

                case PresentationMode.Installer:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Help, true);
                    this.Show(PresentationTypes.Logged, true);
                    this.Show(PresentationTypes.MachineMode, true);
                    this.Show(PresentationTypes.MachineMarch, true);
                    this.Show(PresentationTypes.Error, this.machineErrorsService.ActiveError != null);
                    this.Show(PresentationTypes.Debug, true);
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

        #endregion
    }
}
