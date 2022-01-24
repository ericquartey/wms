using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Ferretto.VW.App.Services
{
    internal sealed class SerialPortsService : ISerialPortsService, IDisposable
    {
        #region Fields

        private const int SerialPortRefreshInterval = 5000;

        private readonly ObservableCollection<string> portNames = new ObservableCollection<string>();

        private readonly object portsSyncRoot = new object();

        private readonly Timer serialPortsPollTimer;

        private bool isDisposed;

        #endregion

        #region Constructors

        public SerialPortsService()
        {
            this.serialPortsPollTimer = new Timer(this.RefreshSystemPorts);
        }

        #endregion

        #region Properties

        public ObservableCollection<string> PortNames => this.portNames;

        #endregion

        #region Methods

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.serialPortsPollTimer.Dispose();

            this.isDisposed = true;
        }

        public void Start()
        {
            this.serialPortsPollTimer.Change(0, SerialPortRefreshInterval);
        }

        public void Stop()
        {
            this.serialPortsPollTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void RefreshSystemPorts(object state)
        {
            lock (this.portsSyncRoot)
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

                    foreach (var knownPort in this.portNames.ToArray())
                    {
                        if (!systemPorts.Contains(knownPort))
                        {
                            this.portNames.Remove(knownPort);
                        }
                    }
                });
            }
        }

        #endregion
    }
}
