using Ferretto.VW.CustomControls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CustomShutterControlSensorsTwoPositionsViewModel : BindableBase, ICustomShutterControlSensorsTwoPositionsViewModel
    {
        #region Fields

        private bool closeSensorState;

        private bool openSensorState;

        #endregion

        #region Properties

        public bool CloseSensorState { get => this.closeSensorState; set => this.SetProperty(ref this.closeSensorState, value); }

        public bool OpenSensorState { get => this.openSensorState; set => this.SetProperty(ref this.openSensorState, value); }

        #endregion
    }
}
