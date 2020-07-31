using System;
using System.Linq;

namespace Ferretto.VW.Devices.TokenReader
{
    internal static class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Console.WriteLine("Available serial ports:");
            foreach (var portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                Console.WriteLine($"- {portName}");
            }
            Console.WriteLine();

            var port = new System.IO.Ports.SerialPort()
            {
                PortName = "COM9",
                BaudRate = 9600,
                DataBits = 8,
                Parity = System.IO.Ports.Parity.Even,
                StopBits = System.IO.Ports.StopBits.One,
                // ReadTimeout = 2000,
            };

            Console.WriteLine($"Port {port.PortName}: opening ...");

            try
            {
                port.Open();
                Console.WriteLine($"Port {port.PortName}: open.");

                do
                {
                    while (!port.CtsHolding)
                    {
                        Console.WriteLine($"Port {port.PortName}: waiting for token ...");
                        System.Threading.Thread.Sleep(500);
                    }

                    Console.WriteLine($"Port {port.PortName}: token is inserted.");

                    var querySerialNumberMessage = new Message(
                        Command.TL,
                        deviceAddress: 1,
                        userDataByteCount: Message.SerialNumberByteCount,
                        userDataStartAddress: Message.SerialNumberDataAddress);

                    var serialNumberResponseMessage = port.SendMessage(querySerialNumberMessage);
                    if (serialNumberResponseMessage is DataMessage dataMessage)
                    {
                        var serialNumber = BitConverter.ToUInt64(dataMessage.UserData.ToArray(), 0);
                        Console.WriteLine($"Port {port.PortName}: token serial number is {serialNumber}.");
                    }

                    while (port.CtsHolding)
                    {
                        Console.WriteLine($"Port {port.PortName}: waiting for token to be removed.");
                        System.Threading.Thread.Sleep(500);
                    }
                } while (true);
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"Port {port.PortName}: timeout.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Port {port.PortName}: error: {ex.Message}");
            }
            finally
            {
                if (port.IsOpen)
                {
                    Console.WriteLine($"Port {port.PortName}: closing ...");

                    port.Close();

                    Console.WriteLine($"Port {port.PortName}: closed.");
                }
            }

            Console.WriteLine($"Program terminated.");

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine($"Press ENTER to exit.");
                Console.ReadLine();
            }
        }

        #endregion
    }
}
