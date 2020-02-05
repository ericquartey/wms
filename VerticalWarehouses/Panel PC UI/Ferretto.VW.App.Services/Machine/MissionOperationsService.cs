using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class MissionOperationsService : IMissionOperationsService
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsWmsWebService missionOperationsDataService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsDataService;

        private readonly IOperatorHubClient operatorHubClient;

        #endregion

        #region Constructors

        public MissionOperationsService(
            IEventAggregator eventAggregator,
            IMachineMissionOperationsWebService missionOperationsWebService,
            WMS.Data.WebAPI.Contracts.IMissionOperationsWmsWebService missionOperationsDataService,
            WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsDataService,
            IBayManager bayManager,
            IOperatorHubClient operatorHubClient)
        {
            this.eventAggregator = eventAggregator;
            this.missionOperationsDataService = missionOperationsDataService ?? throw new ArgumentNullException(nameof(missionOperationsDataService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));
            this.operatorHubClient.AssignedMissionOperationChanged += async (sender, e) => await this.OnAssignedMissionOperationChangedAsync(sender, e);
        }

        #endregion

        #region Properties

        public WMS.Data.WebAPI.Contracts.MissionInfo CurrentMission { get; private set; }

        public WMS.Data.WebAPI.Contracts.MissionOperation CurrentMissionOperation { get; private set; }

        #endregion

        #region Methods

        public async Task CancelCurrentAsync()
        {
            await this.missionOperationsWebService.CancelAsync();
        }

        public async Task CompleteCurrentAsync(double quantity)
        {
            await this.missionOperationsWebService.CompleteAsync(
                this.CurrentMissionOperation.Id,
                quantity,
                ConfigurationManager.AppSettings.GetLabelPrinterName());
        }

        public async Task PartiallyCompleteCurrentAsync(double quantity)
        {
            await this.missionOperationsWebService.PartiallyCompleteAsync(
                this.CurrentMissionOperation.Id,
                quantity,
                ConfigurationManager.AppSettings.GetLabelPrinterName());
        }

        public async Task StartAsync()
        {
            await this.LoadCurrentMissionOperationAsync();
        }

        private async Task LoadCurrentMissionOperationAsync()
        {
            var bay = await this.bayManager.GetBayAsync();

            var currentMissionOperation = this.CurrentMissionOperation;

            if (!(bay.CurrentMission?.Id is null)
                &&
                bay.CurrentWmsMissionOperationId.HasValue)
            {
                try
                {
                    //var loadingUnitAccessibleInBay = this.bay.Positions.Where(p => p.LoadingUnit != null).OrderByDescending(p => p.Height).Select(p => p.LoadingUnit).FirstOrDefault();
                    var currentMission = await this.missionsDataService.GetByIdAsync(bay.CurrentMission.Id);
                    // if (loadingUnitAccessibleInBay?.Id == currentMission.LoadingUnitId)
                    //  {
                    this.CurrentMission = currentMission;
                    this.CurrentMissionOperation =
                        await this.missionOperationsDataService.GetByIdAsync(bay.CurrentWmsMissionOperationId.Value);
                    //}
                    // else
                    // {
                    //     this.logger.Warn($"Mission discarded load. unit in Bay Id {loadingUnitAccessibleInBay?.Id}, Mission load. Unit Id {currentMission.LoadingUnitId}");
                    //}
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                this.CurrentMission = null;
                this.CurrentMissionOperation = null;
            }

            if (currentMissionOperation?.Id != this.CurrentMissionOperation?.Id
                ||
                currentMissionOperation?.RequestedQuantity != this.CurrentMissionOperation?.RequestedQuantity)
            {
                var args = new AssignedMissionOperationChangedEventArgs(bay.Number, bay.CurrentMission?.Id, bay.CurrentWmsMissionOperationId);
                this.eventAggregator
                       .GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                       .Publish(args);
            }
        }

        private async Task OnAssignedMissionOperationChangedAsync(object sender, AssignedMissionOperationChangedEventArgs e)
        {
            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
            if (e.BayNumber == bayNumber)
            {
                await this.LoadCurrentMissionOperationAsync();
            }

            return;
        }

        #endregion
    }
}
