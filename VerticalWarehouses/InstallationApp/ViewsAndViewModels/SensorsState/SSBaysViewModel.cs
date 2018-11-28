using Prism.Mvvm;
using Ferretto.VW.RemoteIODriver.Source;
using System.Collections.Generic;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    internal class SSBaysViewModel : BindableBase
    {
        #region Fields

        private bool luPresentInBay;

        #endregion Fields

        #region Constructors

        public SSBaysViewModel()
        {
            RemoteIOManager.Current.SensorsSyncronizedEventHandler += this.UpdateSensorsState;
        }

        #endregion Constructors

        #region Properties

        public System.Boolean LuPresentInBay { get => this.luPresentInBay; set => this.SetProperty(ref this.luPresentInBay, value); }

        #endregion Properties

        #region Methods

        private void UpdateSensorsState()
        {
            var tmp = new List<bool>();

            for (int i = 0; i < 7; i++)
            {
                tmp.Add(RemoteIOManager.Current.Inputs[i]);
            }
            this.LuPresentInBay = tmp[5];
        }

        #endregion Methods
    }
}
