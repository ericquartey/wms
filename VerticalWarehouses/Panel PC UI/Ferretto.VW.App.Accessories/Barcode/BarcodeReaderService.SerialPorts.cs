using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class BarcodeReaderService
    {
        #region Fields

        private const int SerialPortRefreshInterval = 5000;

        private readonly ObservableCollection<string> portNames = new ObservableCollection<string>();

        private Timer serialPortsPollTimer;

        #endregion

        #region Properties

        public ObservableCollection<string> PortNames => this.portNames;

        #endregion

        #region Methods

        public void InitializeSerialPortsTimer()
        {
            this.serialPortsPollTimer?.Dispose();
            this.serialPortsPollTimer = new Timer(this.RefreshSystemPorts, null, 0, SerialPortRefreshInterval);
        }

        private void DisableSerialPortsTimer()
        {
            this.serialPortsPollTimer?.Dispose();
            this.serialPortsPollTimer = null;
        }

        private void RefreshSystemPorts(object state)
        {
            var systemPorts = System.IO.Ports.SerialPort.GetPortNames();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var systemPort in systemPorts)
                {
                    if (!this.portNames.Contains(systemPort))
                    {
                        this.portNames.Add(systemPort);
                    }
                }

                foreach (var knownPort in this.portNames)
                {
                    if (!systemPorts.Contains(knownPort))
                    {
                        this.portNames.Remove(knownPort);
                    }
                }
            });
        }

        #endregion
    }
}
