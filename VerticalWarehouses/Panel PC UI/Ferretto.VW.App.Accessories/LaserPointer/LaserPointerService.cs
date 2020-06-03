﻿using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.Devices;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed class LaserPointerService : ILaserPointerService
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineMissionsWebService missionWebService;

        private ILaserPointerDriver laserPointerDriver;

        private SubscriptionToken loadingUnitToken;

        private SubscriptionToken missionToken;

        #endregion

        #region Constructors

        public LaserPointerService(
            IEventAggregator eventAggregator,
            ILaserPointerDriver laserPointerDriver,
            IBayManager bayManager,
            IMachineMissionsWebService missionWebService)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.laserPointerDriver = laserPointerDriver ?? throw new ArgumentNullException(nameof(laserPointerDriver));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.missionWebService = missionWebService ?? throw new ArgumentNullException(nameof(missionWebService));
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

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            //this.logger.Info("StopAsync;Switch off laser pointer");
            //await this.laserPointerDriver.EnabledAsync(false, false);
        }

        private async Task LaserPointerConfigureAsync()
        {
            try
            {
                var accessories = await this.bayManager.GetBayAccessoriesAsync();

                if (accessories is null)
                {
                    return;
                }

                var laserPointer = accessories.LaserPointer;
                if (laserPointer.IsEnabledNew)
                {
                    var ipAddress = laserPointer.IpAddress;
                    var port = laserPointer.TcpPort;
                    var xOffset = 0;
                    var yOffset = laserPointer.YOffset;
                    var zOffsetLowerPosition = laserPointer.ZOffsetLowerPosition;
                    var zOffsetUpperPosition = laserPointer.ZOffsetUpperPosition;

                    this.laserPointerDriver.Configure(ipAddress, port, xOffset, yOffset, zOffsetLowerPosition, zOffsetUpperPosition);
                }
                else
                {
                    this.laserPointerDriver = null;
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
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
                    var bay = await this.bayManager.GetBayAsync();
                    if (bay.CurrentMission is null)
                    {
                        this.logger.Debug("OnMissionChangeAsync;Switch off laser pointer");
                        await this.laserPointerDriver.EnabledAsync(false, false);
                    }
                    return;
                }

                if (e.WmsOperation.CompartmentId == 0)
                {
                    return;
                }

                this.logger.Debug($"OnMissionChangeAsync:Id {e.MachineMission.Id} MissionType {e.MachineMission.MissionType} Status {e.MachineMission.Status}");

                if (e.MachineMission.MissionType is MissionType.WMS)
                {
                    await this.LaserPointerConfigureAsync();

                    if (this.laserPointerDriver is null)
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

                        var compartmentSelected = e.WmsMission.LoadingUnit.Compartments.SingleOrDefault(c => c.Id == e.WmsOperation.CompartmentId);

                        double itemHeight = 0;
                        if (e.WmsOperation.ItemHeight != null)
                        {
                            itemHeight = e.WmsOperation.ItemHeight.Value;
                        }

                        var point = this.laserPointerDriver.CalculateLaserPoint(e.WmsMission.LoadingUnit.Width, e.WmsMission.LoadingUnit.Depth, compartmentSelected.Width.Value, compartmentSelected.Depth.Value, compartmentSelected.XPosition.Value, compartmentSelected.YPosition.Value, itemHeight, bayPosition.IsUpper, bay.Side);

                        this.logger.Info("Move and switch on laser pointer");
                        await this.laserPointerDriver.MoveAndSwitchOnAsync(point);
                    }
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
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
