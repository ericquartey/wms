using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemOperationWaitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMissionOperationsService missionOperationsService;

        private bool isPerformingOperation;

        private SubscriptionToken missionToken;

        private int pendingMissionOperationsCount;

        #endregion

        #region Constructors

        public ItemOperationWaitViewModel(
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator)
            : base(PresentationMode.Operator)
        {
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(missionOperationsService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public int PendingMissionOperationsCount
        {
            get => this.pendingMissionOperationsCount;
            set => this.SetProperty(ref this.pendingMissionOperationsCount, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.eventAggregator.GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>().Unsubscribe(this.missionToken);

            this.missionToken?.Dispose();
            this.missionToken = null;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.missionToken = this.missionToken
                ??
                this.eventAggregator.GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                .Subscribe(
                    this.OnAssignedMissionOperationChanged,
                    ThreadOption.UIThread,
                    false);

            if (this.isPerformingOperation)
            {
                this.NavigationService.GoBack();
                this.isPerformingOperation = false;
                return;
            }

            this.CheckForNewOperation();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }

        private void CheckForNewOperation()
        {
            this.PendingMissionOperationsCount = this.missionOperationsService.PendingMissionOperationsCount;

            var missionOperation = this.missionOperationsService.CurrentMissionOperation;
            if (missionOperation is null)
            {
                // do nothing
                return;
            }

            switch (missionOperation.Type)
            {
                case MissionOperationType.Inventory:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.INVENTORY,
                        null,
                        trackCurrentView: true);
                    this.isPerformingOperation = true;
                    break;

                case MissionOperationType.Pick:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.PICK,
                        null,
                        trackCurrentView: true);
                    this.isPerformingOperation = true;
                    break;

                case MissionOperationType.Put:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.PUT,
                        null,
                        trackCurrentView: true);
                    this.isPerformingOperation = true;
                    break;

                case MissionOperationType.LoadingUnitCheck:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.LOADINGUNITCHECKVIEW,
                        null,
                        trackCurrentView: true);
                    this.isPerformingOperation = true;
                    break;
            }
        }

        private void OnAssignedMissionOperationChanged(AssignedMissionOperationChangedEventArgs e)
        {
            this.CheckForNewOperation();
        }

        #endregion
    }
}
