using System;
using System.Threading.Tasks;
using Ferretto.VW.MachineAutomationService.Hubs;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.MachineAutomationService.Contracts
{
    public class MachineHubClient : IMachineHubClient
    {
        #region Fields

        private const int DefaultMaxReconnectTimeout = 5000;

        private readonly Uri endpoint;

        private readonly Random random = new Random();

        private HubConnection connection;

        #endregion

        #region Constructors

        public MachineHubClient(Uri uri, int machineId)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            this.endpoint = uri;
            this.MachineId = machineId;
        }

        #endregion

        #region Events

        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        public event EventHandler<ElevatorPositionChangedEventArgs> ElevatorPositionChanged;

        public event EventHandler<LoadingUnitChangedEventArgs> LoadingUnitInBayChanged;

        public event EventHandler<LoadingUnitChangedEventArgs> LoadingUnitInElevatorChanged;

        public event EventHandler<MachineStatusReceivedEventArgs> MachineStatusReceived;

        public event EventHandler<ModeChangedEventArgs> ModeChanged;

        public event EventHandler<UserChangedEventArgs> UserChanged;

        #endregion

        #region Properties

        public int MachineId { get; }

        public int MaxReconnectTimeoutMilliseconds { get; set; } = DefaultMaxReconnectTimeout;

        #endregion

        #region Methods

        public async Task ConnectAsync()
        {
            while (this.connection?.State != HubConnectionState.Connected)
            {
                try
                {
                    this.Initialize();

                    await this.connection.StartAsync();

                    this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(this.MachineId, true));
                }
                catch
                {
                    await this.WaitForReconnectionAsync();
                }
            }
        }

        public async Task DisconnectAsync()
        {
            if (this.connection?.State == HubConnectionState.Connected)
            {
                await this.connection?.StopAsync();

                this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(this.MachineId, false));
            }
        }

        public async Task RequestCurrentStateAsync()
        {
            await this.connection.SendAsync(nameof(IMachineHub.GetCurrentStatus));
        }

        private void Initialize()
        {
            if (this.connection != null)
            {
                return;
            }

            this.connection = new HubConnectionBuilder()
                .WithUrl(this.endpoint.AbsoluteUri)
                .Build();

            this.connection.On<int>(
                nameof(IMachineHub.ElevatorPositionChanged),
                this.OnElevatorPositionChanged);

            this.connection.On<MachineStatus>(
                nameof(IMachineHub.EchoCurrentStatus),
                this.OnMachineStatusReceived);

            this.connection.On<int?>(
                nameof(IMachineHub.LoadingUnitInBayChanged),
                this.OnLoadingUnitInBayChanged);

            this.connection.On<int?>(
                nameof(IMachineHub.LoadingUnitInElevatorChanged),
                this.OnLoadingUnitInElevatorChanged);

            this.connection.On<MachineMode, int?>(
                nameof(IMachineHub.ModeChanged),
                this.OnModeChanged);

            this.connection.On<int, int?>(
                nameof(IMachineHub.UserChanged),
                this.OnUserChanged);

            this.connection.Closed += async (error) =>
            {
                this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(this.MachineId, false));

                await this.ConnectAsync();
            };
        }

        private void OnElevatorPositionChanged(int position)
        {
            this.ElevatorPositionChanged?.Invoke(this, new ElevatorPositionChangedEventArgs(this.MachineId, position));
        }

        private void OnLoadingUnitInBayChanged(int? loadingUnitId)
        {
            this.LoadingUnitInBayChanged?.Invoke(this, new LoadingUnitChangedEventArgs(this.MachineId, loadingUnitId));
        }

        private void OnLoadingUnitInElevatorChanged(int? loadingUnitId)
        {
            this.LoadingUnitInElevatorChanged?.Invoke(this, new LoadingUnitChangedEventArgs(this.MachineId, loadingUnitId));
        }

        private void OnMachineStatusReceived(MachineStatus machineStatus)
        {
            this.MachineStatusReceived?.Invoke(this, new MachineStatusReceivedEventArgs(machineStatus));
        }

        private void OnModeChanged(MachineMode mode, int? faultCode)
        {
            this.ModeChanged?.Invoke(this, new ModeChangedEventArgs(this.MachineId, mode, faultCode));
        }

        private void OnUserChanged(int bayId, int? userId)
        {
            this.UserChanged?.Invoke(this, new UserChangedEventArgs(bayId, userId));
        }

        private async Task WaitForReconnectionAsync()
        {
            var reconnectionTime = this.random.Next(0, this.MaxReconnectTimeoutMilliseconds);

            await Task.Delay(reconnectionTime);
        }

        #endregion
    }
}
