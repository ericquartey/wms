using System.Windows.Media;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSVerticalAxisViewModel : BindableBase, IViewModel
    {
        #region Fields

        private bool brakeResistanceOvertemperature = true;
        private bool emergencyEndRun;
        private bool zeroVerticalSensor = true;

        #endregion Fields

        #region Properties

        public System.Boolean BrakeResistanceOvertemperature { get => this.brakeResistanceOvertemperature; set => this.SetProperty(ref this.brakeResistanceOvertemperature, value); }

        public System.Boolean EmergencyEndRun { get => this.emergencyEndRun; set => this.SetProperty(ref this.emergencyEndRun, value); }

        public System.Boolean ZeroVerticalSensor { get => this.zeroVerticalSensor; set => this.SetProperty(ref this.zeroVerticalSensor, value); }

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
