using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Ferretto.VW.Devices.WeightingScale
{
    public sealed class WeightingScaleDriver : IWeightingScaleDriver, IDisposable
    {
        #region Fields

        private const int DefaultReadTimeout = 6000;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly SerialPort serialPort = new SerialPort();

        private readonly object syncRoot = new object();

        private DeviceInformation information = new DeviceInformation();

        private bool isDisposed;

        #endregion

        #region Methods

        public async Task ClearMessageAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleDriver));
            }

            if (!this.serialPort.IsOpen)
            {
                this.logger.Warn($"Serial port {this.serialPort.PortName}: cannot send command because the port is not open.");
            }

            var displayIdentifier = 1;
            await Task.Run(() =>
            {
                lock (this.syncRoot)
                {
                    this.SendCommand($"DINT{displayIdentifier:00}0001");
                }
            });
        }

        public void Connect(SerialPortOptions options)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleDriver));
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

        public async Task DisplayMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The message cannot be null or whitespace.", nameof(message));
            }

            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleDriver));
            }

            if (!this.serialPort.IsOpen)
            {
                this.logger.Warn($"Serial port {this.serialPort.PortName}: cannot send command because the port is not open.");
            }

            var displayIdentifier = 1;
            await Task.Run(() =>
            {
                lock (this.syncRoot)
                {
                    this.SendCommand($"DINT{displayIdentifier:00}0000");
                    this.SendCommand($"DISP{displayIdentifier:00}{message}");
                }
            });
        }

        public async Task DisplayMessageAsync(string message, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The message cannot be null or whitespace.", nameof(message));
            }

            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleDriver));
            }

            if (!this.serialPort.IsOpen)
            {
                this.logger.Warn($"Serial port {this.serialPort.PortName}: cannot send command because the port is not open.");
            }

            var totalMilliseconds = duration.TotalMilliseconds;
            if (totalMilliseconds <= 0)
            {
                throw new ArgumentNullException("The duration must be strictly positive.", nameof(duration));
            }

            await Task.Run(() =>
            {
                lock (this.syncRoot)
                {
                    this.SendCommand($"DINT01{totalMilliseconds:X4}");
                    this.SendCommand($"DISP01{message}");
                }
            });
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

        public async Task<IWeightSample> MeasureWeightAsync()
        {
            WeightSample acquiredSample = null;
            await Task.Run(() =>
            {
                lock (this.syncRoot)
                {
                    var line = this.SendCommand($"REXT");

                    if (WeightSample.TryParse(line, out var sample))
                    {
                        acquiredSample = sample;
                    }
                }
            });

            return acquiredSample;
        }

        public async Task ResetAverageUnitaryWeightAsync()
        {
            await Task.Run(() =>
            {
                lock (this.syncRoot)
                {
                    this.SendCommand($"X0.0");
                }
            });
        }

        public async Task<string> RetrieveVersionAsync()
        {
            string versionString = null;
            await Task.Run(() =>
            {
                lock (this.syncRoot)
                {
                    var line = this.SendCommand("VER");
                    versionString = line.Replace("VER", "");
                }
            });

            return versionString;
        }

        public async Task SetAverageUnitaryWeightAsync(float weight)
        {
            await Task.Run(() =>
            {
                lock (this.syncRoot)
                {
                    var line = this.SendCommand($"X{weight:0.0}");
                }
            });
        }

        private string SendCommand(string command)
        {
            this.logger.Debug($"Port {this.serialPort.PortName}: sending command '{command}'.");

            this.serialPort.Write($"{command}\r\n");
            var response = this.serialPort.ReadLine();

            this.logger.Debug($"Port {this.serialPort.PortName}: received '{response}'.");

            switch (response)
            {
                case "ERR01": throw new Exception($"The string '{command}' is a valid command but it is followed by unexpected characters.");
                case "ERR02": throw new Exception($"The command '{command}' contains invalid data.");
                case "ERR03": throw new Exception($"The command '{command}' is not valid in the current context.");
                case "ERR04": throw new Exception($"The string '{command}' is not a valid command.");
                default: return response;
            };
        }

        #endregion
    }
}
