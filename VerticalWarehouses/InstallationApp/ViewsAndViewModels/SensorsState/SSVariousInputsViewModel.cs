using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSVariousInputsViewModel : BindableBase, IViewModel, ISSVariousInputsViewModel
    {
        #region Fields

        private bool antiIntrusionGate;

        private bool cradleEngineSelected = true;

        private bool elevatorEngineSelected;

        private bool microCarterLeftSide = true;

        private bool microCarterRightSide;

        private bool mushroomHeadButton;

        private bool securityFunctionActive = true;

        #endregion

        #region Properties

        public bool AntiIntrusionGate { get => this.antiIntrusionGate; set => this.SetProperty(ref this.antiIntrusionGate, value); }

        public bool CradleEngineSelected { get => this.cradleEngineSelected; set => this.SetProperty(ref this.cradleEngineSelected, value); }

        public bool ElevatorEngineSelected { get => this.elevatorEngineSelected; set => this.SetProperty(ref this.elevatorEngineSelected, value); }

        public bool MicroCarterLeftSide { get => this.microCarterLeftSide; set => this.SetProperty(ref this.microCarterLeftSide, value); }

        public bool MicroCarterRightSide { get => this.microCarterRightSide; set => this.SetProperty(ref this.microCarterRightSide, value); }

        public bool MushroomHeadButton { get => this.mushroomHeadButton; set => this.SetProperty(ref this.mushroomHeadButton, value); }

        public bool SecurityFunctionActive { get => this.securityFunctionActive; set => this.SetProperty(ref this.securityFunctionActive, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
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
