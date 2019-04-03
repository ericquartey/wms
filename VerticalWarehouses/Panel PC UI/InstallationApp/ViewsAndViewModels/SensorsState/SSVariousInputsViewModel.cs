using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSVariousInputsViewModel : BindableBase, ISSVariousInputsViewModel
    {
        #region Fields

        private bool antiIntrusionShutterBay1;

        private bool cradleEngineSelected = true;

        private bool elevatorEngineSelected;

        private IEventAggregator eventAggregator;

        private bool microCarterLeftSideBay1 = true;

        private bool microCarterRightSideBay1;

        private bool mushroomHeadButtonBay1;

        private bool securityFunctionActive = true;

        #endregion

        #region Constructors

        public SSVariousInputsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public bool AntiIntrusionShutterBay1 { get => this.antiIntrusionShutterBay1; set => this.SetProperty(ref this.antiIntrusionShutterBay1, value); }

        public bool CradleEngineSelected { get => this.cradleEngineSelected; set => this.SetProperty(ref this.cradleEngineSelected, value); }

        public bool ElevatorEngineSelected { get => this.elevatorEngineSelected; set => this.SetProperty(ref this.elevatorEngineSelected, value); }

        public bool MicroCarterLeftSideBay1 { get => this.microCarterLeftSideBay1; set => this.SetProperty(ref this.microCarterLeftSideBay1, value); }

        public bool MicroCarterRightSideBay1 { get => this.microCarterRightSideBay1; set => this.SetProperty(ref this.microCarterRightSideBay1, value); }

        public bool MushroomHeadButtonBay1 { get => this.mushroomHeadButtonBay1; set => this.SetProperty(ref this.mushroomHeadButtonBay1, value); }

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
