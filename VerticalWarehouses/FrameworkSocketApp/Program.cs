using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkSocketApp
{
    internal class Program
    {
        #region Methods

        public static async Task StartClientAsync()
        {
            byte[] receiveBuffer = new byte[1024];

            IPAddress inverterAddress = IPAddress.Parse("169.254.231.248");

            int sendPort = 17221;

            Socket transportSocket;

            transportSocket = new Socket(inverterAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await transportSocket.ConnectAsync(inverterAddress, sendPort);

            do
            {
                var memoryBuffer = new ArraySegment<byte>(receiveBuffer);
                try
                {
                    var readBytes = await transportSocket.ReceiveAsync(memoryBuffer, SocketFlags.None);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            } while (true);
        }

        private static void Main(string[] args)
        {
            try
            {
                StartClientAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}
