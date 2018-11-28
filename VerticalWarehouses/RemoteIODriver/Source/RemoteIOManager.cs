using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.RemoteIODriver.Source
{
    public class RemoteIOManager
    {
        #region Fields

        public static RemoteIOManager Current;
        public List<bool> Inputs = new List<bool>();
        private const int CICLE_PERIOD_MS = 20;
        private readonly RemoteIO remoteIO;

        #endregion Fields

        #region Constructors

        public RemoteIOManager()
        {
            this.remoteIO = new RemoteIO();
            this.SensorsSyncronizedEventHandler += this.Initializer;
            this.SyncDevice();
        }

        #endregion Constructors

        #region Delegates

        public delegate void SensorsSyncronizedEvent();

        #endregion Delegates

        #region Events

        public event SensorsSyncronizedEvent SensorsSyncronizedEventHandler;

        #endregion Events

        #region Methods

        private void Initializer()
        {
        }

        private void RaiseSensorsSyncronizedEvent()
        {
            RemoteIOManager.Current.SensorsSyncronizedEventHandler();
        }

        private async void SyncDevice()
        {
            while (true)
            {
                this.Inputs.Clear();
                this.Inputs = this.remoteIO.ReadData();
                //this.remoteIO.WriteData();
                await Task.Delay(20);
                this.RaiseSensorsSyncronizedEvent();
            }
        }

        #endregion Methods
    }
}
