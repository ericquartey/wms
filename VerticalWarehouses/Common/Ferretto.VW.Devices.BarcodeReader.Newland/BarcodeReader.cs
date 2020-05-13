using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    internal sealed class BarcodeReader : IBarcodeReaderDriver, IQueryableDevice, IDisposable
    {
        #region Fields

        private const int ReadBufferSize = 2048;

        private readonly DeviceInformation information = new DeviceInformation();

        private readonly SerialPort serialPort = new SerialPort();

        private bool isDisposed;

        private Thread readThread;

        #endregion

        #region Events

        public event EventHandler<ActionEventArgs> BarcodeReceived;

        #endregion

        #region Properties

        public DeviceInformation Information => this.information;

        #endregion

        #region Methods

        public void Connect(ConfigurationOptions options)
        {
            if (this.serialPort.IsOpen)
            {
                System.Diagnostics.Debug.WriteLine("Serial port is already open.");
                return;
            }

            this.serialPort.PortName = options.PortName;
            this.serialPort.BaudRate = options.BaudRate;
            this.serialPort.Parity = options.Parity;
            this.serialPort.StopBits = options.StopBits;

            this.serialPort.Open();
            this.readThread = new Thread(this.ReadLoop);
            this.readThread.Start();
        }

        public void Disconnect()
        {
            this.readThread?.Abort();
            this.readThread = null;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing)
            this.Dispose(true);
        }

        public void SendCommand(string command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandString = new CommandString(command);
            this.serialPort.Write(commandString.ToString());
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
            var buffer = new byte[ReadBufferSize];

            try
            {
                var byteCount = this.serialPort.Read(buffer, 0, buffer.Length);

                var barcode = Encoding.ASCII.GetString(buffer, 0, byteCount);

                System.Diagnostics.Debug.WriteLine($"Received barcode '{barcode}'.");

                this.BarcodeReceived?.Invoke(this, new ActionEventArgs(barcode));
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error while reading from serial port {this.serialPort.PortName}: {ex.Message}");

                this.serialPort.Open();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error while reading from serial port {this.serialPort.PortName}: {ex.Message}");
                Thread.Sleep(1000);
                if (!this.serialPort.IsOpen)
                {
                    System.Diagnostics.Debug.WriteLine("Trying to reconnect to serial port {this.serialPort.PortName}.");

                    this.serialPort.Open();
                }
            }
        }

        private void ReadLoop()
        {
            try
            {
                do
                {
                    this.ReadBytes();
                }
                while (this.serialPort.IsOpen);
            }
            catch (ThreadAbortException)
            {
                System.Diagnostics.Debug.WriteLine("Serial port reader thread stopped.");
                if (this.serialPort.IsOpen)
                {
                    System.Diagnostics.Debug.WriteLine("Closing serial port.");
                    this.serialPort.Close();
                }
            }
        }

        #endregion
    }
}
