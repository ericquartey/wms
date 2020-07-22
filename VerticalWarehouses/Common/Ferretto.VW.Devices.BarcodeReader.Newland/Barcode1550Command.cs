using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.RegularExpressions;
using NLog;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public class Barcode1550Command : BarcodeCommand
    {
        #region Fields

        public static readonly Barcode1550Command InquireAllInformation = new Barcode1550Command("99900300");

        public static readonly Barcode1550Command InquireFirmwareVersion = new Barcode1550Command("99900301");

        public static readonly Barcode1550Command InquireManufactureDate = new Barcode1550Command("99900303");

        public static readonly Barcode1550Command InquireModelNumber = new Barcode1550Command("99900304");

        public static readonly Barcode1550Command InquireSerialNumber = new Barcode1550Command("99900302");

        private static readonly Regex responseRegex = new Regex(
            @"@@@@!(?<command>[^;]+);(&{(?<params>.+)\|\w+})?\^\^\^\^", RegexOptions.Compiled);

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public Barcode1550Command(string commandCode)
            : base(commandCode)
        {
        }

        #endregion

        #region Methods

        public IEnumerable<string> Send(SerialPort serialPort)
        {
            if (serialPort is null)
            {
                throw new ArgumentNullException(nameof(serialPort));
            }

            if (!serialPort.IsOpen)
            {
                this.logger.Warn("The serial port is not open.");
                return Array.Empty<string>();
            }

            try
            {
                var commandString = $"$$$$#{this.CommandCode};%%%%";
                this.logger.Debug($"Sending command '{commandString}' to barcode reader ...");
                serialPort.Write(commandString);
                this.logger.Debug($"Command '{commandString}' sent. Reading response ...");

                var buffer = new byte[serialPort.ReadBufferSize];
                var response = string.Empty;
                do
                {
                    var byteCount = serialPort.Read(buffer, 0, buffer.Length);
                    response += System.Text.Encoding.ASCII.GetString(buffer, 0, byteCount);
                    this.logger.Debug($"Received: '{response}'.");
                }
                while (!response.EndsWith("^^^^", StringComparison.Ordinal));
                this.logger.Debug($"Received command response: '{response}'.");

                var match = responseRegex.Match(response);
                if (!match.Success)
                {
                    this.logger.Warn("Unable to parse command response.");
                    return Array.Empty<string>();
                }
                else
                {
                    return match.Groups["params"].Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error($"Unable to send command to barcode reader: {ex.Message}.");
                return Array.Empty<string>();
            }
        }

        #endregion
    }
}
