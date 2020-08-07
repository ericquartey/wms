using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Ferretto.VW.Devices.WeightingScale
{
    public sealed class WeightingScaleDriver : SerialPortDriver, IWeightingScaleDriver
    {
        #region Fields

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly object syncRoot = new object();

        #endregion

        #region Methods

        public async Task ClearMessageAsync()
        {
            if (!this.SerialPort.IsOpen)
            {
                this.logger.Warn($"Serial port {this.SerialPort.PortName}: cannot send command because the port is not open.");
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

        public async Task DisplayMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The message cannot be null or whitespace.", nameof(message));
            }

            if (!this.SerialPort.IsOpen)
            {
                this.logger.Warn($"Serial port {this.SerialPort.PortName}: cannot send command because the port is not open.");
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

            if (!this.SerialPort.IsOpen)
            {
                this.logger.Warn($"Serial port {this.SerialPort.PortName}: cannot send command because the port is not open.");
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
                    this.SendCommand($"SPMU0000.0");
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
                    versionString = line.Replace("VER,", string.Empty);
                }
            });

            return versionString;
        }

        public async Task SetAverageUnitaryWeightAsync(float weight)
        {
            if (weight <= 0)
            {
                throw new ArgumentNullException(nameof(weight), "Average unitary weight must be strictly positive.");
            }

            await Task.Run(() =>
            {
                lock (this.syncRoot)
                {
                    var line = this.SendCommand($"SPMU{weight:0.0}");
                }
            });
        }

        protected override Task OnSerialPortClosedAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        protected override Task OnSerialPortOpenedAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        private string SendCommand(string command)
        {
            this.logger.Debug($"Port {this.SerialPort.PortName}: sending command '{command}'.");

            this.SerialPort.Write($"{command}\r\n");
            var response = this.SerialPort.ReadLine();

            this.logger.Debug($"Port {this.SerialPort.PortName}: received '{response}'.");

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
