﻿using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.Devices;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class LaserPointerService : ILaserPointerService
    {
        #region Fields

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IBayManager bayManager;

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly ILaserPointerDriver laserPointerDriver;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineWmsStatusWebService machineWmsStatusWebService;

        private readonly IMachineMissionsWebService missionWebService;

        private readonly int pollingDelay = 200;

        private bool isEnabled;

        private SubscriptionToken machineModeChangedToken;

        private SubscriptionToken missionToken;

        private string PollingStep = "Undefined";

        private Task runningTask;

        private SubscriptionToken socketLinkToken;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public LaserPointerService(
            IEventAggregator eventAggregator,
            ILaserPointerDriver laserPointerDriver,
            IBayManager bayManager,
            IMachineMissionsWebService missionWebService,
            IMachineWmsStatusWebService machineWmsStatusWebService,
            IMachineAccessoriesWebService accessoriesWebService
            )
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.laserPointerDriver = laserPointerDriver ?? throw new ArgumentNullException(nameof(laserPointerDriver));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.missionWebService = missionWebService ?? throw new ArgumentNullException(nameof(missionWebService));
            this.machineWmsStatusWebService = machineWmsStatusWebService ?? throw new ArgumentNullException(nameof(machineWmsStatusWebService));
            this.accessoriesWebService = accessoriesWebService;

            this.bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
        }

        #endregion

        #region Properties

        public Devices.DeviceInformation DeviceInformation
        {
            get
            {
                if (this.laserPointerDriver is IQueryableDevice queryableDevice)
                {
                    return queryableDevice.Information;
                }
                else
                {
                    throw new NotSupportedException("The laser pointer driver does not support querying information");
                }
            }
        }

        #endregion

        #region Methods

        public async Task LaserPointerConfigureAsync()
        {
            try
            {
                var laserPointer = await this.accessoriesWebService.GetLaserPointerAsync(this.bayNumber);
                if (laserPointer != null &&
                    laserPointer.IsEnabledNew)
                {
                    var ipAddress = laserPointer.IpAddress;
                    var port = laserPointer.TcpPort;
                    var xOffset = laserPointer.XOffset;
                    var yOffset = laserPointer.YOffset;
                    var zOffsetLowerPosition = laserPointer.ZOffsetLowerPosition;
                    var zOffsetUpperPosition = laserPointer.ZOffsetUpperPosition;

                    this.laserPointerDriver.Configure(ipAddress, port, xOffset, yOffset, zOffsetLowerPosition, zOffsetUpperPosition);
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

        public void ResetPoint()
        {
            //this.laserPointerDriver.ResetSelectedPoint();
        }

        public Task StartAsync()
        {
            this.missionToken = this.missionToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MissionChangedEventArgs>>()
                    .Subscribe(
                        async e => await this.OnMissionChangedAsync(e),
                        ThreadOption.BackgroundThread,
                        false);

            this.socketLinkToken = this.socketLinkToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<SocketLinkLaserPointerChangeMessageData>>()
                    .Subscribe(
                        async e => await this.OnSocketLinkLaserPointerChangeAsync(e),
                        ThreadOption.BackgroundThread,
                        false);

            this.machineModeChangedToken = this.eventAggregator
               .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
               .Subscribe(
                   async e => await this.OnMachineModeChangedAsync(e),
                   ThreadOption.UIThread,
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
                        await this.PollingLaserPointer(cancellationToken);
                        await Task.Delay(this.pollingDelay, cancellationToken);
                    }
                    while (!cancellationToken.IsCancellationRequested);
                }
                catch (OperationCanceledException)
                {
                    if (this.isEnabled)
                    {
                        this.logger.Debug("StopAsync;Switch off laser pointer");
                        this.laserPointerDriver.ClearCommands();
                        await this.laserPointerDriver.EnabledAsync(false, false);
                        await this.PollingLaserPointer(null);
                        await this.laserPointerDriver.DisconnectAsync();
                    }
                    this.logger.Info("Stop laser pointer service");
                }
            });
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            this.tokenSource?.Cancel();
            this.runningTask?.Wait();
        }

        private void NotifyError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
        }

        private async Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            try
            {
                if (e.MachineMode == MachineMode.Automatic)
                {
                    var activeMission = await this.RetrieveActiveMissionAsync();
                    if (activeMission == null)
                    {
                        this.logger.Debug($"Send Home to Laser pointer.");
                        await this.laserPointerDriver.HomeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private async Task OnMissionChangedAsync(MissionChangedEventArgs e)
        {
            try
            {
                this.logger.Trace($"OnMissionChangedAsync");
                if (e.MachineMission is null || e.WmsOperation is null)
                {
                    await this.LaserPointerConfigureAsync();

                    if (this.isEnabled)
                    {
                        var socketLink = await this.machineWmsStatusWebService.SocketLinkIsEnabledAsync();
                        if (!socketLink)
                        {
                            this.logger.Debug("OnMissionChangedAsync;Switch off laser pointer");
                            await this.laserPointerDriver.EnabledAsync(false, false);
                        }
                    }
                    return;
                }

                if (e.WmsOperation.CompartmentId == 0)
                {
                    return;
                }

                this.logger.Debug($"OnMissionChangedAsync:Id {e.MachineMission.Id}; " +
                    $"Operation {e.WmsOperation.Id}; " +
                    $"Compartment {e.WmsOperation.CompartmentId}; " +
                    $"MissionType {e.MachineMission.MissionType}; " +
                    $"Status {e.MachineMission.Status}");

                if (e.MachineMission.MissionType is MissionType.WMS)
                {
                    await this.LaserPointerConfigureAsync();

                    if (!this.isEnabled)
                    {
                        return;
                    }

                    var activeMission = await this.RetrieveActiveMissionAsync();
                    if (activeMission != null && activeMission.WmsId.HasValue)
                    {
                        var bay = await this.bayManager.GetPositionsBayAsync();
                        var bayPosition = bay.Positions.SingleOrDefault(p => p.LoadingUnit?.Id == e.WmsMission.LoadingUnit.Id);

                        if (bayPosition is null)
                        {
                            this.logger.Trace($"Load unit {e.WmsMission.LoadingUnit.Id} not in Bay");
                            return;
                        }

                        var compartmentSelected = e.WmsMission.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == e.WmsOperation.CompartmentId);

                        // itemHeight priority is:
                        // first check Load Unit Laser offset
                        // only if it is not defined check wms database for Item height
                        double itemHeight = 0;
                        if (bayPosition.LoadingUnit != null && bayPosition.LoadingUnit.IsLaserOffset)
                        {
                            itemHeight = bayPosition.LoadingUnit.LaserOffset;
                        }
                        else if (e.WmsOperation.ItemHeight != null)
                        {
                            itemHeight = e.WmsOperation.ItemHeight.Value;
                        }

                        var point = this.laserPointerDriver.CalculateLaserPoint(
                            e.WmsMission.LoadingUnit.Width,
                            e.WmsMission.LoadingUnit.Depth,
                            compartmentSelected.Width.Value,
                            compartmentSelected.Depth.Value,
                            compartmentSelected.XPosition.Value,
                            compartmentSelected.YPosition.Value,
                            itemHeight,
                            bayPosition.IsUpper,
                            bay.Side);

                        this.logger.Info($"Move and switch on laser pointer; " +
                            $"luW {e.WmsMission.LoadingUnit.Width}; " +
                            $"luD {e.WmsMission.LoadingUnit.Depth}; " +
                            $"cw {compartmentSelected.Width.Value}; " +
                            $"cd {compartmentSelected.Depth.Value}; " +
                            $"cx {compartmentSelected.XPosition.Value}; " +
                            $"cy {compartmentSelected.YPosition.Value}; " +
                            $"z {itemHeight}; " +
                            $"up {bayPosition.IsUpper}; " +
                            $"side {bay.Side}");
                        await this.laserPointerDriver.MoveAndSwitchOnAsync(point, false);
                    }
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private async Task OnSocketLinkLaserPointerChangeAsync(NotificationMessageUI<SocketLinkLaserPointerChangeMessageData> message)
        {
            try
            {
                LaserPoint point;
                var bay = await this.bayManager.GetPositionsBayAsync();

                await this.LaserPointerConfigureAsync();

                if (!this.isEnabled)
                {
                    return;
                }

                if (this.bayNumber.ToString() != message.Data.BayNumber.ToString())
                {
                    return;
                }

                switch (message.Data.CommandCode)
                {
                    case 0: // switch off
                        await this.laserPointerDriver.EnabledAsync(false, false);
                        this.logger.Info("OnSocketLinkLaserPointerChangeAsync, switch 0");
                        break;

                    case 1: // switch on in upper bay position
                        if (message.Data is null)
                        {
                            throw new ArgumentNullException(nameof(message));
                        }
                        if (this.bayManager.Identity is null)
                        {
                            throw new InvalidOperationException("bayManager.Identity");
                        }
                        if (bay is null)
                        {
                            throw new InvalidOperationException("bay");
                        }

                        point = this.laserPointerDriver.CalculateLaserPointForSocketLink(message.Data.X, message.Data.Y, message.Data.Z, this.bayManager.Identity, true, bay.Side);
                        await this.laserPointerDriver.MoveAndSwitchOnAsync(point, false);
                        this.logger.Info($"OnSocketLinkLaserPointerChangeAsync, switch on {message.Data.CommandCode} {point}, up {true}, side {bay.Side}");
                        break;

                    case 2: // switch on in lower bay position
                        if (message.Data is null)
                        {
                            throw new ArgumentNullException(nameof(message));
                        }
                        if (this.bayManager.Identity is null)
                        {
                            throw new InvalidOperationException("bayManager.Identity");
                        }
                        if (bay is null)
                        {
                            throw new InvalidOperationException("bay");
                        }
                        point = this.laserPointerDriver.CalculateLaserPointForSocketLink(message.Data.X, message.Data.Y, message.Data.Z, this.bayManager.Identity, false, bay.Side);
                        await this.laserPointerDriver.MoveAndSwitchOnAsync(point, false);
                        this.logger.Info($"OnSocketLinkLaserPointerChangeAsync, switch on {message.Data.CommandCode} {point}, up {false}, side {bay.Side}");
                        break;
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private async Task PollingLaserPointer(CancellationToken? cancellationToken)
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
                        await this.LaserPointerConfigureAsync();
                        // TODO - bypass laser home command at start
                        //this.PollingStep = "Home";

                        this.PollingStep = "Active";
                        this.logger.Debug($"PollingStep {this.PollingStep}; isEnabled {this.isEnabled}");
                        break;
                    }
                case "Home":
                    {
                        if (this.isEnabled)
                        {
                            await this.laserPointerDriver.HomeAsync();
                            if (await this.laserPointerDriver.ExecuteCommandsAsync(cancellationToken).ConfigureAwait(true))
                            {
                                this.PollingStep = "Active";
                                this.logger.Debug($"PollingStep {this.PollingStep}; isEnabled {this.isEnabled}");
                            }
                        }
                        else
                        {
                            this.PollingStep = "Active";
                            this.logger.Debug($"PollingStep {this.PollingStep}; isEnabled {this.isEnabled}");
                        }
                        break;
                    }
                case "Active":
                    {
                        if (this.isEnabled)
                        {
                            await this.laserPointerDriver.ExecuteCommandsAsync(cancellationToken).ConfigureAwait(true);
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
