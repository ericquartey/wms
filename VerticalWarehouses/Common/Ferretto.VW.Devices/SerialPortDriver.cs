using System;
using System.IO.Ports;
using NLog;

namespace Ferretto.VW.Devices.TokenReader
{
    public abstract class SerialPortDriver : IDisposable
    {
        #region Fields

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly SerialPort serialPort = new SerialPort();

        private bool isDisposed;

        #endregion

        #region Properties

        public DeviceInformation Information { get; protected set; } = new DeviceInformation();

        protected SerialPort SerialPort => this.serialPort;

        #endregion

        #region Methods

        public void Connect(SerialPortOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(SerialPortDriver));
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

            this.Information = new DeviceInformation();

            this.serialPort.PortName = options.PortName;
            this.serialPort.BaudRate = options.BaudRate;
            this.serialPort.Parity = options.Parity;
            this.serialPort.StopBits = options.StopBits;
            if (options.ReadTimeout.HasValue)
            {
                this.serialPort.ReadTimeout = (int)options.ReadTimeout.Value.TotalMilliseconds;
            }

            this.serialPort.Open();

            this.logger.Debug($"Serial port {this.serialPort.PortName} opened.");

            this.OnSerialPortOpened();
        }

        public void Disconnect()
        {
            this.logger.Debug($"Serial port {this.serialPort.PortName}: closing ...");

            this.serialPort.Close();

            this.logger.Debug($"Serial port {this.serialPort.PortName}: closed.");

            this.OnSerialPortClosed();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.Disconnect();

            this.isDisposed = true;
        }

        protected abstract void OnSerialPortClosed();

        protected abstract void OnSerialPortOpened();

        #endregion
    }
}
