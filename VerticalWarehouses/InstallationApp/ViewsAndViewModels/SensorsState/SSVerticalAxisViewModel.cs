using System.Windows.Media;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSVerticalAxisViewModel : BindableBase, IViewModel, ISSVerticalAxisViewModel
    {
        #region Fields

        private bool brakeResistanceOvertemperature = true;

        private bool emergencyEndRun;

        private bool zeroVerticalSensor = true;

        #endregion

        #region Properties

        public bool BrakeResistanceOvertemperature { get => this.brakeResistanceOvertemperature; set => this.SetProperty(ref this.brakeResistanceOvertemperature, value); }

        public bool EmergencyEndRun { get => this.emergencyEndRun; set => this.SetProperty(ref this.emergencyEndRun, value); }

        public bool ZeroVerticalSensor { get => this.zeroVerticalSensor; set => this.SetProperty(ref this.zeroVerticalSensor, value); }

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
