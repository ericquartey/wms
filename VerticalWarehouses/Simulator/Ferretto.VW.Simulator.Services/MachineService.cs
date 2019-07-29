using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Simulator.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NLog;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services
{
    internal class MachineService : BindableBase, IMachineService
    {
        #region Fields

        private readonly TcpListener listenerInverter = new TcpListener(IPAddress.Any, 17221);

        private readonly TcpListener listenerIoDriver1 = new TcpListener(IPAddress.Any, 19550);

        private readonly TcpListener listenerIoDriver2 = new TcpListener(IPAddress.Any, 19551);

        private readonly TcpListener listenerIoDriver3 = new TcpListener(IPAddress.Any, 19552);

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private CancellationTokenSource cts = new CancellationTokenSource();

        #endregion

        #region Properties

        public bool IsStartedSimulator { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Start simulator, inizialize socket (???)
        /// </summary>
        /// <returns></returns>
        public async Task ProcessStartSimulatorAsync()
        {
            this.Logger.Trace("1:ProcessStartSimulator");

            this.cts = new CancellationTokenSource();
            this.listenerInverter.Start();
            this.listenerIoDriver1.Start();
            this.listenerIoDriver2.Start();
            this.listenerIoDriver3.Start();

            Task.Run(() => this.AcceptClient(this.listenerInverter, this.cts.Token, (client, message) => this.ReplyInverter(client, message)));
            //Task.Run(() => this.AcceptClient(this.listenerIoDriver1, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message)));
            //Task.Run(() => this.AcceptClient(this.listenerIoDriver2, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message)));
            //Task.Run(() => this.AcceptClient(this.listenerIoDriver3, this.cts.Token, (client, message) => this.ReplyIoDriver(client, message)));

            await Task.Delay(100);
            this.IsStartedSimulator = true;

            this.RaisePropertyChanged(nameof(this.IsStartedSimulator));
        }

        public async Task ProcessStopSimulatorAsync()
        {
            this.Logger.Trace("1:ProcessStopSimulator");

            this.cts.Cancel();
            this.listenerInverter.Stop();
            //this.listenerIoDriver1.Stop();
            //this.listenerIoDriver2.Stop();
            //this.listenerIoDriver3.Stop();

            await Task.Delay(100);
            this.IsStartedSimulator = false;

            this.RaisePropertyChanged(nameof(this.IsStartedSimulator));
        }

        private void AcceptClient(TcpListener listener, CancellationToken token, Action<TcpClient, byte[]> messageHandler)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = listener.AcceptTcpClient();
                    Task.Run(() => this.ManageClient(client, this.cts.Token, messageHandler));
                }
            }
            catch (SocketException)
            {
            }
        }

        private void ManageClient(TcpClient client, CancellationToken token, Action<TcpClient, byte[]> messageHandler)
        {
            using (client)
            {
                var buffer = new byte[1024];
                var socket = client.Client;
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (socket != null && socket.Connected)
                        {
                            if (socket.Poll(0, SelectMode.SelectRead))
                            {
                                var bytes = socket.Receive(buffer);
                                if (bytes > 0)
                                {
                                    byte[] message = new byte[bytes];
                                    Array.Copy(buffer, 0, message, 0, message.Length);
                                    messageHandler(client, message);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                        Task.Delay(50);
                    }
                }
                catch (SocketException)
                {
                }

            }
        }

        private void ReplyInverter(TcpClient client, byte[] message)
        {

        }

        private void ReplyIoDriver(TcpClient client, byte[] message)
        {

        }

        #endregion
    }
}
