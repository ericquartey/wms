using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Ferretto.VW.Devices.TokenReader
{
    public sealed class TokenReaderDriver : ITokenReaderDriver, IDisposable
    {
        #region Fields

        private const int DefaultReadTimeout = 6000;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly SerialPort serialPort = new SerialPort();

        private readonly object syncRoot = new object();

        private DeviceInformation information = new DeviceInformation();

        private bool isDisposed;

        #endregion

        #region Events

        public event EventHandler<TokenStatusChangedEventArgs> TokenStatusChanged;

        #endregion

        #region Methods

        public void Connect(SerialPortOptions options)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(TokenReaderDriver));
            }

            this.logger.Debug($"Opening serial port {options.PortName} ...");

            if (this.serialPort.IsOpen)
            {
                if (options.PortName.Equals(this.serialPort.PortName, StringComparison.OrdinalIgnoreCase))
                {
                    this.logger.Warn($"Serial port {this.serialPort.PortName} is already open.");

                    return;
                }
                else
                {
                    this.Disconnect();
                }
            }

            this.information = new DeviceInformation();

            this.serialPort.PortName = options.PortName;
            this.serialPort.BaudRate = options.BaudRate;
            this.serialPort.Parity = options.Parity;
            this.serialPort.StopBits = options.StopBits;
            this.serialPort.ReadTimeout = DefaultReadTimeout;

            this.serialPort.Open();

            this.logger.Debug($"Serial port {this.serialPort.PortName} opened.");
        }

        public void Disconnect()
        {
            this.logger.Debug($"Serial port {this.serialPort.PortName}: closing ...");

            this.serialPort.Close();

            this.logger.Debug($"Serial port {this.serialPort.PortName}: closed.");
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.Disconnect();

            this.isDisposed = true;
        }

        #endregion
    }
}
