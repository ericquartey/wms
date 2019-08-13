using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    public class HeaderViewModel : BasePresentationViewModel
    {
        #region Methods

        public override void InitializeData()
        {
            base.InitializeData();
            this.States.Add(this.GetInstance(nameof(PresentationTheme)));
            this.States.Add(this.GetInstance(nameof(PresentationShutdown)));
            this.States.Add(this.GetInstance(nameof(PresentationHelp)));
            this.States.Add(this.GetInstance(nameof(PresentationLogged)));
            this.States.Add(this.GetInstance(nameof(PresentationMachineMode)));
            this.States.Add(this.GetInstance(nameof(PresentationMachineMarch)));
            this.States.Add(this.GetInstance(nameof(PresentationError)));
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

                case PresentationMode.Installator:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Help, true);
                    this.Show(PresentationTypes.Logged, true);
                    this.Show(PresentationTypes.MachineMode, true);
                    this.Show(PresentationTypes.MachineMarch, true);
                    this.Show(PresentationTypes.Error, true);
                    break;

                case PresentationMode.Operator:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Help, true);
                    this.Show(PresentationTypes.Logged, true);
                    this.Show(PresentationTypes.MachineMode, true);
                    this.Show(PresentationTypes.MachineMarch, true);
                    this.Show(PresentationTypes.Error, true);
                    break;

                case PresentationMode.Help:
                    break;
            }
        }

        #endregion
    }
}
