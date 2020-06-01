﻿using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.Devices;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Accessories.AlphaNumericBar
{
    internal sealed class AlphaNumericBarService : IAlphaNumericBarService
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineMissionsWebService missionWebService;

        private IAlphaNumericBarDriver alphaNumericBarDriver;

        private SubscriptionToken loadingUnitToken;

        private SubscriptionToken missionToken;

        #endregion

        #region Constructors

        public AlphaNumericBarService(
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IAlphaNumericBarDriver alphaNumericBarDriver,
            IMachineMissionsWebService missionWebService)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.alphaNumericBarDriver = alphaNumericBarDriver ?? throw new ArgumentNullException(nameof(missionWebService));
            this.missionWebService = missionWebService ?? throw new ArgumentNullException(nameof(missionWebService));

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

        public Task StartAsync()
        {
            this.loadingUnitToken = this.loadingUnitToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        async e => await this.OnLoadingUnitMovedAsync(e),
                        ThreadOption.BackgroundThread,
                        false);

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
            this.logger.Info("Switch off alpha numeric bar.");

            await this.alphaNumericBarDriver.EnabledAsync(false);
        }

        private async Task AlphaNumericBarConfigureAsync()
        {
            try
            {
                var accessories = await this.bayManager.GetBayAccessoriesAsync();

                if (accessories is null)
                {
                    this.alphaNumericBarDriver = null;
                    return;
                }

                var alphaNumericBar = accessories.AlphaNumericBar;

                if (alphaNumericBar.IsEnabledNew)
                {
                    var ipAddress = alphaNumericBar.IpAddress;
                    var port = alphaNumericBar.TcpPort;
                    var size = (MAS.DataModels.AlphaNumericBarSize)alphaNumericBar.Size;

                    this.alphaNumericBarDriver.Configure(ipAddress, port, size);
                }
                else
                {
                    this.alphaNumericBarDriver = null;
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

        private async Task OnLoadingUnitMovedAsync(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            await this.AlphaNumericBarConfigureAsync();

            if (this.alphaNumericBarDriver is null)
            {
                return;
            }

            if (message.Data.MissionType is CommonUtils.Messages.Enumerations.MissionType.IN
                &&
                message.Status is CommonUtils.Messages.Enumerations.MessageStatus.OperationExecuting)
            {
                try
                {
                    this.logger.Debug("Switch off alpha numeric bar");
                    await this.alphaNumericBarDriver.EnabledAsync(false);
                }
                catch (Exception ex)
                {
                    this.NotifyError(ex);
                }
            }
        }

        private async Task OnMissionChangeAsync(MissionChangedEventArgs e)
        {
            try
            {
                if (e.MachineMission is null || e.WmsOperation is null)
                {
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
                        var compartmentSelected = e.WmsMission.LoadingUnit.Compartments.SingleOrDefault(c => c.Id == e.WmsOperation.CompartmentId);

                        var arrowPosition = this.alphaNumericBarDriver.CalculateArrowPosition(compartmentSelected.Width.Value, compartmentSelected.XPosition.Value);
                        this.logger.Debug($"AlphaNumericService;OnMissionChangeAsync; width {compartmentSelected.Width.Value} X {compartmentSelected.XPosition.Value} bar position {arrowPosition}");
                        //var arrowPosition = this.alphaNumericBarDriver.CalculateArrowPosition(e.WmsMission.LoadingUnit.Width, compartmentSelected.XPosition.Value);
                        await this.alphaNumericBarDriver.SetAndWriteArrowAsync(arrowPosition, true);        // show the arrow in the rigth position

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

                        message += e.WmsOperation.RequestedQuantity + " " + e.WmsOperation.ItemCode + " " + e.WmsOperation.ItemDescription;
                        message = message.Trim();

                        var offset = this.alphaNumericBarDriver.CalculateOffset(arrowPosition + 6, message);
                        if (offset > 0)
                        {
                            await this.alphaNumericBarDriver.SetAndWriteMessageAsync(message, offset, false);
                        }
                        else if (offset == -1)
                        {
                            await this.alphaNumericBarDriver.SetAndWriteMessageScrollAsync(message, 0, arrowPosition, false);
                        }
                        else
                        {
                            var start = arrowPosition + 6;
                            await this.alphaNumericBarDriver.SetAndWriteMessageScrollAsync(message, start, (this.alphaNumericBarDriver.NumberOfLeds - start) / 6, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
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
