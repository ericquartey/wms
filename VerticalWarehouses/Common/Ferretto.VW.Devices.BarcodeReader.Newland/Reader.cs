using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public class Reader : IBarcodeReader, IDisposable
    {
        #region Fields

        private const int ReadBufferSize = 2048;

        private readonly SerialPort serialPort = new SerialPort();

        private bool isDisposed;

        private Thread readThread;

        #endregion

        #region Events

        public event EventHandler<BarcodeEventArgs> BarcodeReceived;

        #endregion

        #region Methods

        public void Connect(IBarcodeConfigurationOptions options)
        {
            if (options is ConfigurationOptions serialOptions)
            {
                this.Disconnect();

                this.serialPort.PortName = serialOptions.PortName;
                this.serialPort.BaudRate = serialOptions.BaudRate;
                this.serialPort.Parity = serialOptions.Parity;
                this.serialPort.StopBits = serialOptions.StopBits;

                this.serialPort.Open();
                this.readThread = new Thread(this.Readbytes);
                this.readThread.Start();
            }
            else
            {
                throw new InvalidEnumArgumentException();
            }
        }

        public void Disconnect()
        {
            if (this.serialPort.IsOpen)
            {
                this.serialPort.Close();
            }

            this.readThread?.Abort();
            this.readThread = null;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
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

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.Disconnect();
                }

                this.isDisposed = true;
            }
        }

        private void Readbytes()
        {
            try
            {
                var buffer = new byte[ReadBufferSize];
                do
                {
                    try
                    {
                        var byteCount = this.serialPort.Read(buffer, 0, buffer.Length);

                        var barcode = Encoding.ASCII.GetString(buffer, 0, byteCount);

                        System.Diagnostics.Debug.WriteLine($"Received barcode '{barcode}'.");

                        this.BarcodeReceived?.Invoke(this, new BarcodeEventArgs(barcode));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error while reading from serial port {this.serialPort.PortName}: {ex.Message}");
                        return;
                    }
                }
                while (this.serialPort.IsOpen);
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        #endregion
    }
}
