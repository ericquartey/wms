using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomShutterControlSensorsThreePositionsViewModel : BindableBase, ICustomShutterControlSensorsThreePositionsViewModel
    {
        #region Fields

        private bool closeSensorState;

        private bool middleSensorState;

        private bool openSensorState;

        #endregion

        #region Properties

        public bool CloseSensorState { get => this.closeSensorState; set => this.SetProperty(ref this.closeSensorState, value); }

        public bool MiddleSensorState { get => this.middleSensorState; set => this.SetProperty(ref this.middleSensorState, value); }

        public BindableBase NavigationViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool OpenSensorState { get => this.openSensorState; set => this.SetProperty(ref this.openSensorState, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public Task OnEnterViewAsync()
        {
            // TODO
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
