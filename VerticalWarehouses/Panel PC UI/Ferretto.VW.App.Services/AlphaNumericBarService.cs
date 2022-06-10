using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.Devices;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class AlphaNumericBarService : IAlphaNumericBarService
    {
        #region Fields

        private readonly IAlphaNumericBarDriver alphaNumericBarDriver;

        private readonly IBayManager bayManager;

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly ILaserPointerService laserPointerService;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineWmsStatusWebService machineWmsStatusWebService;

        private readonly IMachineMissionsWebService missionWebService;

        private readonly int pollingDelay = 200;

        private bool isEnabled;

        private SubscriptionToken missionToken;

        private string PollingStep = "Undefined";

        private Task runningTask;

        private SubscriptionToken socketLinkToken;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public AlphaNumericBarService(
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IAlphaNumericBarDriver alphaNumericBarDriver,
            IMachineMissionsWebService missionWebService,
            ILaserPointerService laserPointerService,
            IMachineWmsStatusWebService machineWmsStatusWebService
            )
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.alphaNumericBarDriver = alphaNumericBarDriver ?? throw new ArgumentNullException(nameof(alphaNumericBarDriver));
            this.missionWebService = missionWebService ?? throw new ArgumentNullException(nameof(missionWebService));
            this.laserPointerService = laserPointerService ?? throw new ArgumentNullException(nameof(laserPointerService));
            this.machineWmsStatusWebService = machineWmsStatusWebService ?? throw new ArgumentNullException(nameof(machineWmsStatusWebService));

            this.bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
        }

        #endregion

        #region Properties

        public Devices.DeviceInformation DeviceInformation
        {
            get
            {
                if (this.alphaNumericBarDriver is IQueryableDevice queryableDevice)
                {
                    return queryableDevice.Information;
                }
                else
                {
                    throw new NotSupportedException("The alpha numeric bar driver does not support querying information");
                }
            }
        }

        #endregion

        #region Methods

        public async Task AlphaNumericBarConfigureAsync()
        {
            try
            {
                var accessories = await this.bayManager.GetBayAccessoriesAsync();

                if (accessories is null)
                {
                    this.isEnabled = false;
                    return;
                }

                var alphaNumericBar = accessories.AlphaNumericBar;

                if (alphaNumericBar != null &&
                    alphaNumericBar.IsEnabledNew)
                {
                    var ipAddress = alphaNumericBar.IpAddress;
                    var port = alphaNumericBar.TcpPort;
                    var size = alphaNumericBar.Size;

                    var bay = await this.bayManager.GetBayAsync();

                    this.alphaNumericBarDriver.Configure(ipAddress, port, size, bay.IsExternal, alphaNumericBar.MaxMessageLength);
                    this.isEnabled = true;
                }
                else
                {
                    this.isEnabled = false;
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        public Task StartAsync()
        {
            this.missionToken = this.missionToken
            ??
            this.eventAggregator
                .GetEvent<PubSubEvent<MissionChangedEventArgs>>()
                .Subscribe(
                    async e => await this.OnMissionChangeAsync(e),
                    ThreadOption.BackgroundThread,
                    false);

            this.socketLinkToken = this.socketLinkToken
            ??
            this.eventAggregator
                .GetEvent<NotificationEventUI<SocketLinkAlphaNumericBarChangeMessageData>>()
                .Subscribe(
                    async e => await this.OnSocketLinkAlphaNumericBarChangeAsync(e),
                    ThreadOption.BackgroundThread,
                    false);

            this.tokenSource?.Cancel();
            this.tokenSource = new CancellationTokenSource();

            this.runningTask = Task.Run(async () =>
            {
                var cancellationToken = this.tokenSource.Token;

                try
                {
                    do
                    {
                        await this.PollingAlphaNumericBar(cancellationToken);
                        await Task.Delay(this.pollingDelay, cancellationToken);
                    }
                    while (!cancellationToken.IsCancellationRequested);
                }
                catch (OperationCanceledException)
                {
                    if (this.isEnabled)
                    {
                        this.logger.Debug("StopAsync;Switch off alpha numeric bar");
                        this.alphaNumericBarDriver.ClearCommands();
                        await this.alphaNumericBarDriver.EnabledAsync(false);
                        await this.PollingAlphaNumericBar(null);

                        this.alphaNumericBarDriver.Disconnect();
                    }
                    this.logger.Info("Stop alpha numeric bar service");
                }
            });
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            this.tokenSource.Cancel();
            if (this.runningTask != null)
            {
                this.runningTask.Wait();
            }
        }

        private string GetMessageFromMissionChangedEventArg(MissionChangedEventArgs e)
        {
            var message = "?";

            switch (e.WmsOperation.Type)
            {
                case MissionOperationType.Pick:
                    message = "-";
                    break;

                case MissionOperationType.Put:
                    message = "+";
                    break;
            }

            message += (e.WmsOperation.RequestedQuantity - e.WmsOperation.DispatchedQuantity) + " " + e.WmsOperation.ItemCode + " " + e.WmsOperation.ItemDescription;
            return message;
        }

        private void NotifyError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
        }

        private async Task OnMissionChangeAsync(MissionChangedEventArgs e)
        {
            try
            {
                if (e.MachineMission is null || e.WmsOperation is null)
                {
                    await this.AlphaNumericBarConfigureAsync();

                    if (this.alphaNumericBarDriver != null)
                    {
                        var socketLink = await this.machineWmsStatusWebService.SocketLinkIsEnabledAsync();
                        if (!socketLink)
                        {
                            this.logger.Debug("OnMissionChangeAsync;Switch off alpha numeric bar");
                            await this.alphaNumericBarDriver.EnabledAsync(false);
                            //await this.alphaNumericBarDriver.EnabledAsync(false);

                            this.alphaNumericBarDriver.SelectedMessage = string.Empty;
                            this.alphaNumericBarDriver.SelectedPosition = null;
                        }
                    }

                    return;
                }

                if (e.WmsOperation.CompartmentId == 0)
                {
                    return;
                }

                if (e.MachineMission.MissionType is MissionType.OUT || e.MachineMission.MissionType is MissionType.WMS)
                {
                    await this.AlphaNumericBarConfigureAsync();

                    if (this.alphaNumericBarDriver is null)
                    {
                        return;
                    }

                    var activeMission = await this.RetrieveActiveMissionAsync();
                    if (activeMission != null && activeMission.WmsId.HasValue)
                    {
                        var bay = await this.bayManager.GetBayAsync();
                        var bayPosition = bay.Positions.SingleOrDefault(p => p.LoadingUnit?.Id == e.WmsMission.LoadingUnit.Id);

                        if (bayPosition is null)
                        {
                            return;
                        }

                        var compartmentSelected = e.WmsMission.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == e.WmsOperation.CompartmentId);

                        var offsetArrow = 0;
                        var offsetMessage = 0;
                        var message = this.GetMessageFromMissionChangedEventArg(e);

                        if (this.alphaNumericBarDriver.SelectedMessage != message
                            || this.alphaNumericBarDriver.SelectedPosition != compartmentSelected.XPosition
                            )
                        {
                            this.alphaNumericBarDriver.SelectedPosition = compartmentSelected.XPosition;
                            this.alphaNumericBarDriver.SelectedMessage = message;
                            this.logger.Debug($"OnMissionChangeAsync; Compartment {e.WmsOperation.CompartmentId}; SelectedPosition {compartmentSelected.XPosition}; message {message}");

                            await this.alphaNumericBarDriver.EnabledAsync(false);

                            this.alphaNumericBarDriver.GetOffsetArrowAndMessageFromCompartment(compartmentSelected.Width.Value, compartmentSelected.XPosition.Value, message, e.WmsMission.LoadingUnit.Width, bay.Side, out offsetArrow, out offsetMessage, out var scrollEnd);

                            if (!await this.alphaNumericBarDriver.SetAndWriteArrowAsync(offsetArrow, true))
                            {
                                this.alphaNumericBarDriver.SelectedMessage = string.Empty;
                                return;
                            }

                            if (message.Length > 0)
                            {
                                if (scrollEnd > 0)
                                {
                                    await this.alphaNumericBarDriver.SetAndWriteMessageScrollAsync(message, offsetMessage, scrollEnd, false);
                                }
                                else
                                {
                                    await this.alphaNumericBarDriver.SetAndWriteMessageAsync(message, offsetMessage, false);
                                }
                            }
                        }

                        //var arrowPosition = this.alphaNumericBarDriver.CalculateArrowPosition(compartmentSelected.Width.Value, compartmentSelected.XPosition.Value);
                        //this.logger.Debug($"AlphaNumericService;OnMissionChangeAsync; width {compartmentSelected.Width.Value} X {compartmentSelected.XPosition.Value} bar position {arrowPosition}");
                        //await this.alphaNumericBarDriver.EnabledAsync(false);                               // force disable to reset bar
                        //await this.alphaNumericBarDriver.SetAndWriteArrowAsync(arrowPosition, true);        // show the arrow in the rigth position

                        //var message = this.GetMessageFromMissionChangedEventArg(e);
                        ////var message = "?";
                        ////switch (e.WmsOperation.Type)
                        ////{
                        ////    case MissionOperationType.Pick:
                        ////        message = "-";
                        ////        break;

                        ////    case MissionOperationType.Put:
                        ////        message = "+";
                        ////        break;
                        ////}

                        ////message += (e.WmsOperation.RequestedQuantity - e.WmsOperation.DispatchedQuantity) + " " + e.WmsOperation.ItemCode + " " + e.WmsOperation.ItemDescription;
                        //message = this.alphaNumericBarDriver.NormalizeMessageCharacters(message);

                        //var offset = this.alphaNumericBarDriver.CalculateOffset(arrowPosition + 6, message);
                        //if (offset > 0)
                        //{
                        //    await this.alphaNumericBarDriver.SetAndWriteMessageAsync(message, offset, false);
                        //}
                        //else if (offset == -1)
                        //{
                        //    await this.alphaNumericBarDriver.SetAndWriteMessageScrollAsync(message, 0, arrowPosition, false);
                        //}
                        //else
                        //{
                        //    var start = arrowPosition + 6;
                        //    await this.alphaNumericBarDriver.SetAndWriteMessageScrollAsync(message, start, (this.alphaNumericBarDriver.NumberOfLeds - start) / 6, false);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task OnSocketLinkAlphaNumericBarChangeAsync(NotificationMessageUI<SocketLinkAlphaNumericBarChangeMessageData> socketLinkMessage)
        {
            try
            {
                await this.AlphaNumericBarConfigureAsync();

                if (this.alphaNumericBarDriver is null)
                {
                    return;
                }

                await this.alphaNumericBarDriver.EnabledAsync(false);

                var offsetArrow = 0;
                var offsetMessage = 0;
                string message;
                switch (socketLinkMessage.Data.CommandCode)
                {
                    case 0: // switch off
                        this.logger.Info($"OnSocketLinkAlphaNumericBarChangeAsync, switch off {socketLinkMessage.Data.CommandCode}");
                        break;

                    case 1: //  switch on and show text without arrow down
                        message = this.alphaNumericBarDriver.NormalizeMessageCharacters(socketLinkMessage.Data.TextMessage);
                        if (message.Length > 0)
                        {
                            this.alphaNumericBarDriver.GetOffsetMessage(socketLinkMessage.Data.X, message, out offsetMessage);

                            this.logger.Debug($"OnSocketLinkAlphaNumericBarChangeAsync; position {socketLinkMessage.Data.X}; message {message}");
                            await this.alphaNumericBarDriver.SetAndWriteMessageAsync(message, offsetMessage, false);
                        }
                        break;

                    case 2: // switch on and show arrow down at the start of the text
                        message = this.alphaNumericBarDriver.NormalizeMessageCharacters(socketLinkMessage.Data.TextMessage);
                        this.alphaNumericBarDriver.GetOffsetArrowAndMessage(socketLinkMessage.Data.X, message, out offsetArrow, out offsetMessage, out var scrollEnd);

                        this.logger.Debug($"OnSocketLinkAlphaNumericBarChangeAsync; set arrow {offsetArrow}");
                        await this.alphaNumericBarDriver.EnabledAsync(false);
                        await this.alphaNumericBarDriver.SetAndWriteArrowAsync(offsetArrow, true);

                        if (message.Length > 0)
                        {
                            this.logger.Debug($"OnSocketLinkAlphaNumericBarChangeAsync; position {socketLinkMessage.Data.X}; message {message}");
                            if (scrollEnd > 0)
                            {
                                await this.alphaNumericBarDriver.SetAndWriteMessageScrollAsync(message, offsetMessage, scrollEnd, false);
                            }
                            else
                            {
                                await this.alphaNumericBarDriver.SetAndWriteMessageAsync(message, offsetMessage, false);
                            }
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task PollingAlphaNumericBar(CancellationToken? cancellationToken)
        {
            switch (this.PollingStep)
            {
                case "Undefined":
                    {
                        try
                        {
                            await this.bayManager.GetBayAccessoriesAsync();
                        }
                        catch (Exception)
                        {
                            break;
                        }
                        await this.AlphaNumericBarConfigureAsync();
                        this.PollingStep = "Active";
                        this.logger.Debug($"PollingStep {this.PollingStep}; isEnabled {this.isEnabled}");
                        break;
                    }
                case "Active":
                    {
                        if (this.isEnabled)
                        {
                            await this.alphaNumericBarDriver.ExecuteCommandsAsync(cancellationToken).ConfigureAwait(true);
                        }
                        break;
                    }
            }
        }

        private async Task<Mission> RetrieveActiveMissionAsync()
        {
            try
            {
                var machineMissions = await this.missionWebService.GetAllAsync();

                var activeMissions = machineMissions.Where(m =>
                    m.Step is MissionStep.WaitPick
                    &&
                    m.TargetBay == this.bayNumber)
                    .OrderBy(o => o.LoadUnitDestination);

                this.logger.Debug(!activeMissions.Any()
                    ? "No active mission on bay."
                    : $"Active mission has id {activeMissions.FirstOrDefault().Id}.");

                return activeMissions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return null;
        }

        #endregion
    }
}
