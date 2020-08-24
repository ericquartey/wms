using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using Ferretto.VW.CommonUtils;
using NLog;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    internal sealed class BarcodeReader : IBarcodeReaderDriver, IQueryableDevice, IDisposable
    {
        #region Fields

        private const int DefaultReadTimeout = 6000;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly SerialPort serialPort = new SerialPort();

        private DeviceInformation information = new DeviceInformation();

        private bool isDisposed;

        private bool isReading;

        #endregion

        #region Events

        public event EventHandler<ActionEventArgs> BarcodeReceived;

        #endregion

        #region Properties

        public DeviceInformation Information => this.information;

        #endregion

        #region Methods

        public void Connect(SerialPortOptions options)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BarcodeReader));
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

            new Thread(this.ReadLoop).Start(options);
        }

        public void Disconnect()
        {
            this.logger.Debug($"Closing serial port {this.serialPort.PortName}.");

            this.serialPort.Close();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void SimulateRead(string barcode)
        {
            this.BarcodeReceived?.Invoke(this, new ActionEventArgs(barcode));
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.Disconnect();
            }

            this.isDisposed = true;
        }

        private void ReadBytes()
        {
            try
            {
                var buffer = new byte[this.serialPort.ReadBufferSize];

                var byteCount = this.serialPort.Read(buffer, 0, buffer.Length);
                if (byteCount > 0)
                {
                    var barcode = Encoding.ASCII.GetString(buffer, 0, byteCount);

                    this.logger.Debug($"Received barcode '{barcode.Replace("\r", "<CR>").Replace("\n", "<LF>")}'.");

                    this.BarcodeReceived?.Invoke(this, new ActionEventArgs(barcode));
                }
            }
            catch (TimeoutException)
            {
                Thread.Sleep(100);
            }
            catch (System.IO.IOException ex)
            {
                this.logger.Trace($"Serial port '{this.serialPort.PortName}': {ex.Message}.");
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private void ReadLoop(object optionsObject)
        {
            while (this.isReading) { Thread.Sleep(100); }

            this.isReading = true;

            if (optionsObject is NewlandSerialPortOptions options
                &&
                options.DeviceModel is DeviceModel.Newland1550)
            {
                this.Retrieve1550DeviceInfo();
            }

            do
            {
                this.ReadBytes();
            }
            while (this.serialPort.IsOpen && !this.isDisposed);

            this.isReading = false;
        }

        private void Retrieve1550DeviceInfo()
        {
            try
            {
                var responseParams = Barcode1550Command.InquireAllInformation.Send(this.serialPort);
                if (responseParams.Any())
                {
                    foreach (var parameter in responseParams)
                    {
                        this.logger.Debug($"Received parameter: '{parameter}'.");

                        const string ModelNumberParameter = "Product Name:";
                        const string SerialNumberParameter = "Product ID:";
                        const string FirmwareVersionParameter = "Firmware";

                        if (parameter.StartsWith(ModelNumberParameter, StringComparison.Ordinal))
                        {
                            this.information.ModelNumber = parameter.Replace(ModelNumberParameter, "").Trim();
                        }
                        else if (parameter.StartsWith(SerialNumberParameter, StringComparison.Ordinal))
                        {
                            this.information.SerialNumber = parameter.Replace(SerialNumberParameter, "").Trim();
                        }
                        else if (parameter.StartsWith(FirmwareVersionParameter, StringComparison.Ordinal))
                        {
                            this.information.FirmwareVersion = parameter.Replace(FirmwareVersionParameter, "").Trim();
                        }
                    }
                }
                else
                {
                    this.logger.Warn("Unable to retrieve device info.");
                    return;
                }

                responseParams = Barcode1550Command.InquireManufactureDate.Send(this.serialPort);
                if (responseParams.Count() == 1
                    &&
                    DateTime.TryParse(responseParams.First(), out var dateTime))
                {
                    this.information.ManufactureDate = dateTime;
                }
                else
                {
                    this.logger.Warn("Unable to parse manufacture date.");
                }
            }
            catch (Exception ex)
            {
                this.logger.Warn($"Unable to retrieve device info: {ex.Message}.");
            }
        }

        #endregion
    }
}
