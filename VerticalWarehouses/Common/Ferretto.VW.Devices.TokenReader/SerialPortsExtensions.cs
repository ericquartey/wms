using System;
using System.IO.Ports;
using System.Linq;

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

        public static Message SendMessage(this SerialPort port, Message message)
        {
            Console.WriteLine($"Port {port.PortName}: initiating message transfer ...");
            port.WriteByte(StartOfText);
            var response = port.ReadByte();

            EnsureSuccessResponse(response, port);

            var messageBytes = message.ToByteArray();
            port.Write(messageBytes, 0, messageBytes.Length);

            Console.WriteLine($"Port {port.PortName}: waiting for response ...");
            response = port.ReadByte();

            EnsureSuccessResponse(response, port);

            Console.WriteLine($"Port {port.PortName}: positive response, waiting for response message transfer initiation ...");

            response = port.ReadByte();

            EnsureSuccessResponse(response, port);

            Console.WriteLine($"Port {port.PortName}: positive response, waiting for message ...");

            var buffer = new byte[port.ReadBufferSize];
            var totalReadBytes = 0;
            int readBytes;
            do
            {
                readBytes = port.Read(buffer, totalReadBytes, buffer.Length - totalReadBytes);

                if (readBytes == 1 && buffer[totalReadBytes] is StartOfText)
                {
                    port.WriteByte(DataLinkEscape);
                    break;
                }

                totalReadBytes += readBytes;
            } while (readBytes > 0);

            Console.WriteLine($"Port {port.PortName}: received a total of {totalReadBytes} bytes.");

            try
            {
                var responseMessage = Message.FromBytes(buffer, 0, totalReadBytes);

                Console.WriteLine($"Port {port.PortName}: sending end-of-message acknowledgement.");

                port.WriteByte(DataLinkEscape);

                // response = port.ReadByte();
                // Console.WriteLine($"Port {port.PortName}: received response 0x{response:X2}.");
                totalReadBytes = 0;

                do
                {
                    readBytes = port.Read(buffer, totalReadBytes, buffer.Length - totalReadBytes);
                    totalReadBytes += readBytes;

                    Console.WriteLine($"Port {port.PortName}: len={totalReadBytes} buffer {new string(buffer.Take(totalReadBytes).SelectMany(b => $" {(int)b}").ToArray())}.");
                    Console.WriteLine();

                    if (readBytes == 1 && buffer[totalReadBytes] is StartOfText)
                    {
                        Console.WriteLine($"Port {port.PortName}: received STX.");

                        port.WriteByte(DataLinkEscape);
                        break;
                    }
                } while (readBytes > 0);

                return responseMessage;
            }
            catch
            {
                port.WriteByte(NegativeAcknowledgement);
                throw;
            }
        }

        public static void WriteByte(this System.IO.Ports.SerialPort port, byte value)
        {
            port.Write(new byte[] { value }, 0, 1);
        }

        private static void EnsureSuccessResponse(int response, System.IO.Ports.SerialPort port)
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

        #endregion
    }
}
