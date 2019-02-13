using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.InverterDriver
{
    public class HostedInverterDriver : BackgroundService, INewInverterDriver
    {
        private const int HEARTBEAT_TIMEOUT = 300;
        private readonly ManualResetEventSlim priorityInverterCommandReceived;
        private readonly ManualResetEventSlim inverterCommandReceived;
        private readonly ManualResetEventSlim driverCommandReceived;

        private readonly ConcurrentQueue<InverterDriver_CommandMessage> priorityQueue;

        private readonly ConcurrentQueue<InverterDriver_CommandMessage> commandQueue;

        private readonly ConcurrentQueue<InverterDriver_CommandMessage> driverCommandQueue;

        private Socket receiveSocket;

        private Task inverterSendTask;

        private Task inverterReceiveTask;

        private Timer heartBeatTimer;

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        public HostedInverterDriver( IEventAggregator eventAggregator, IInverterDriver inverterDriver)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.inverterDriver.Initialize();

            this.priorityInverterCommandReceived = new ManualResetEventSlim( false );
            this.inverterCommandReceived = new ManualResetEventSlim( false );
            this.driverCommandReceived = new ManualResetEventSlim( false );

            this.priorityQueue = new ConcurrentQueue<InverterDriver_CommandMessage>();
            this.commandQueue = new ConcurrentQueue<InverterDriver_CommandMessage>();
            this.driverCommandQueue = new ConcurrentQueue<InverterDriver_CommandMessage>();

        }

        protected override Task ExecuteAsync( CancellationToken stoppingToken )
        {
            //=== create the heartbeat timer
            this.heartBeatTimer?.Dispose();
            this.heartBeatTimer = new Timer( SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds( HEARTBEAT_TIMEOUT ) );

            //=== create and start the sending Task
            this.inverterSendTask?.Dispose();
            this.inverterSendTask = Task.Run( () => SendInverterCommand( stoppingToken ), stoppingToken );

            //=== create and start the receiving Task
            this.inverterReceiveTask?.Dispose();
            this.inverterReceiveTask = Task.Run( () => ReceiveInverterData( stoppingToken ), stoppingToken);

            //=== This will be the command receiving Task from state machine
            do
            {
                try
                {
                    this.driverCommandReceived.Wait( Timeout.Infinite, stoppingToken );
                }
                catch (OperationCanceledException ex)
                {
                    return Task.CompletedTask;
                }

                this.driverCommandReceived.Reset();

                //=== Identify message and start relevant state machine
                InverterDriver_CommandMessage receivedCommand;

                while (this.driverCommandQueue.TryDequeue( out receivedCommand ))
                {
                    switch (receivedCommand.InverterCommand)
                    {
                        case CommandType.HorizontalHoming:
                            //=== Create and Run Horizontal Homing State Machine
                            break;
                    }
                }

            } while (stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        public override Task StopAsync( CancellationToken stoppingToken )
        {
            var returnValue = base.StopAsync( stoppingToken );

            return returnValue;
        }

        private async void ReceiveInverterData( CancellationToken stoppingToken )
        {
            //var inverterAddress = IPAddress.Parse( "169.254.231.248" );
            //var inverterPortNumber = 17221;
            //this.receiveSocket = new Socket( inverterAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
            //this.receiveSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            //var inverterEndPoint = new IPEndPoint( inverterAddress, inverterPortNumber );

            //this.receiveSocket.Connect( inverterEndPoint );

            //Memory<byte> receiveBuffer = new Memory<byte>(new byte[1024]);

            do
            {
                try
                {
                    //await this.receiveSocket.ReceiveAsync( receiveBuffer, SocketFlags.None, stoppingToken );
                    await Task.Delay( 2000 );
                    //=== Parse Buffer
                }
                catch (OperationCanceledException ex)
                {
                    return;
                }
            } while (stoppingToken.IsCancellationRequested);
        }

        private void SendInverterCommand( CancellationToken cancellationToken)
        {
            ManualResetEventSlim cancellationEventSlim = new ManualResetEventSlim(false);

            cancellationToken.Register( () => cancellationEventSlim.Set() );

            //=== Create WaitHandle array
            WaitHandle[] commandHandles = new[]{ this.priorityInverterCommandReceived.WaitHandle,
                                                 this.inverterCommandReceived.WaitHandle,
                                                 cancellationEventSlim.WaitHandle };

            do
            {
                var handleIndex = WaitHandle.WaitAny( commandHandles, Timeout.Infinite );
                switch (handleIndex)
                {
                    case 0:
                        this.priorityInverterCommandReceived.Reset();
                        ProcessPriorityCommand();
                        break;

                    case 1:
                        this.inverterCommandReceived.Reset();
                        ProcessCommand();
                        break;

                    case 2:
                        return;
                }

            } while (!cancellationToken.IsCancellationRequested);
        }

        private void ProcessCommand()
        {
        }

        private void ProcessPriorityCommand()
        {
        }

        private void SendHeartBeat( object state )
        {
            this.priorityQueue.Enqueue( new InverterDriver_CommandMessage {InverterCommand = CommandType.HeartBeat } );
            this.priorityInverterCommandReceived.Set();
        }

        public new void Dispose()
        {
            base.Dispose();

            this.heartBeatTimer?.Dispose();
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        public void ExecuteHorizontalHoming()
        {
            this.driverCommandQueue.Enqueue( new InverterDriver_CommandMessage { InverterCommand = CommandType.HorizontalHoming } );
            this.driverCommandReceived.Set();
        }

        public void ExecuteHorizontalPosition( int target, int speed, int direction, List<ProfilePosition> profile )
        {
            throw new NotImplementedException();
        }

        public void ExecuteVerticalHoming()
        {
            throw new NotImplementedException();
        }

        public void ExecuteVerticalPosition( int targetPosition, float vMax, float acc, float dec, float weight, short offset )
        {
            throw new NotImplementedException();
        }

        public void ExecuteDrawerWeight( int targetPosition, float vMax, float acc, float dec )
        {
            throw new NotImplementedException();
        }

        public float GetDrawerWeight => throw new NotImplementedException();

        public Boolean[] GetSensorsStates()
        {
            throw new NotImplementedException();
        }
    }
}
