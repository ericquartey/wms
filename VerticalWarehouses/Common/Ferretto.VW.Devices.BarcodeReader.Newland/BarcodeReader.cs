using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    internal sealed class BarcodeReader : IBarcodeReaderDriver, IDisposable
    {
        #region Fields

        private const int ReadBufferSize = 2048;

        private readonly SerialPort serialPort = new SerialPort();

        private bool isDisposed;

        private Thread readThread;

        #endregion

        #region Events

        public event EventHandler<ActionEventArgs> BarcodeReceived;

        #endregion

        #region Methods

        public void Connect(IBarcodeConfigurationOptions options)
        {
            if (this.serialPort.IsOpen)
            {
                System.Diagnostics.Debug.WriteLine("Serial port is already open.");
                return;
            }

            if (options is ConfigurationOptions serialOptions)
            {
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
            this.readThread?.Abort();
            this.readThread = null;

            if (this.serialPort.IsOpen)
            {
                System.Diagnostics.Debug.WriteLine("Closing serial port.");
                this.serialPort.Close();
            }
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

        private void Dispose(bool disposing)
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
                    }
                }
                while (this.serialPort.IsOpen);
            }
            catch (ThreadAbortException)
            {
                System.Diagnostics.Debug.WriteLine("Serial port reader thread stopped.");
                return;
            }
        }

        #endregion
    }
}
