using System;

namespace Ferretto.VW.Devices.TokenReader
{
    internal static class SerialPortExtensions
    {
        #region Fields

        private const int CharacterDelayTime = 100;

        private const byte DataLinkEscape = 0x10;

        private const byte NegativeAcknowledgement = 0x15;

        private const byte StartOfText = 0x02;

        #endregion

        #region Methods

        internal static void EnsureSuccessResponse(this System.IO.Ports.SerialPort port, int response)
        {
            if (response is DataLinkEscape)
            {
                Console.WriteLine($"Port {port.PortName}: positive response.");

                return;
            }
            else if (response is NegativeAcknowledgement)
            {
                throw new ApplicationException("Negative acknowledgement from device.");
            }
            else if (response is StartOfText)
            {
                Console.WriteLine($"Port {port.PortName}: received Start-of-Text, sending Data-Link-Escape ...");

                port.WriteByte(DataLinkEscape);
            }
            else
            {
                System.Threading.Thread.Sleep(CharacterDelayTime);

                port.WriteByte(NegativeAcknowledgement);

                throw new ApplicationException($"Unknown response from device (0x{response:X2}).");
            }
        }

        internal static void WriteByte(this System.IO.Ports.SerialPort port, byte value)
        {
            port.Write(new byte[] { value }, 0, 1);
        }

        #endregion
    }
}
