using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ImmediateLoadingUnitCallViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly List<LoadingUnit> loadingUnits = new List<LoadingUnit>();

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineService machineService;

        private DelegateCommand callLoadingUnitCommand;

        private int currentItemIndex;

        private DelegateCommand downSelectionCommand;

        private bool isSearching;

        private int? loadingUnitId;

        private DelegateCommand loadingUnitsMissionsCommand;

        private int maxLoadingUnitId;

        private int minLoadingUnitId;

        private SubscriptionToken positioningMessageReceivedToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private LoadingUnit selectedUnitUnit;

        private DelegateCommand upSelectionCommand;

        #endregion

        #region Constructors

        public ImmediateLoadingUnitCallViewModel(
            IMachineService machineService,
            IEventAggregator eventAggregator,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService)
            : base(PresentationMode.Operator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
        }

        #endregion

        #region Properties

        public ICommand DownSelectionCommand =>
            this.downSelectionCommand
            ??
            (this.downSelectionCommand = new DelegateCommand(
                this.SelectNextLoadingUnitAsync,
                this.CanSelectNextItem));

        public bool IsSearching
        {
            get => this.isSearching;
            set => this.SetProperty(ref this.isSearching, value, this.RaiseCanExecuteChanged);
        }

        public override bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value) && value)
                {
                    this.ClearNotifications();
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public override bool KeepAlive => true;

        public ICommand LoadingUnitCallCommand =>
            this.callLoadingUnitCommand
            ??
            (this.callLoadingUnitCommand = new DelegateCommand(
                async () => await this.CallLoadingUnitAsync(),
                this.CanCallLoadingUnit));

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set
            {
                this.SetProperty(ref this.loadingUnitId, value, this.CheckToSelectLoadingUnit);
            }
        }

        public IEnumerable<LoadingUnit> LoadingUnits => new BindingList<LoadingUnit>(this.loadingUnits);

        public ICommand LoadingUnitsMissionsCommand =>
            this.loadingUnitsMissionsCommand
            ??
            (this.loadingUnitsMissionsCommand = new DelegateCommand(this.LoadingUnitsMissionsAppear));

        public int MaxLoadingUnitId
        {
            get => this.maxLoadingUnitId;
            set => this.SetProperty(ref this.maxLoadingUnitId, value);
        }

        public int MinLoadingUnitId
        {
            get => this.minLoadingUnitId;
            set => this.SetProperty(ref this.minLoadingUnitId, value);
        }

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedUnitUnit;
            set
            {
                if (this.SetProperty(ref this.selectedUnitUnit, value))
                {
                    if (this.selectedUnitUnit != null)
                    {
                        this.LoadingUnitId = this.selectedUnitUnit?.Id;
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand UpSelectionCommand =>
            this.upSelectionCommand
            ??
            (this.upSelectionCommand = new DelegateCommand(
                this.SelectPreviousLoadingUnitAsync,
                this.CanSelectPreviousItem));

        #endregion

        #region Methods

        public async Task CallLoadingUnitAsync()
        {
            this.Logger.Debug($"CallLoadingUnitAsync: loadingUnitId {this.loadingUnitId} ");

            if (!this.loadingUnitId.HasValue)
            {
                this.ShowNotification(Resources.Localized.Get("General.IdLoadingUnitNotExists"), Services.Models.NotificationSeverity.Warning);
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.MoveToBayAsync(this.LoadingUnitId.Value);

                this.ShowNotification(string.Format(Resources.Localized.Get("ServiceMachine.LoadingUnitSuccessfullyRequested"), this.SelectedLoadingUnit.Id), Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) //when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.LoadingUnitId = null;
                this.IsWaitingForResponse = false;
            }
        }

        public Task GetLoadingUnitsAsync()
        {
            try
            {
                this.loadingUnits.Clear();
                this.loadingUnits.AddRange(this.machineService.Loadunits);

                if (this.loadingUnits != null &&
                    this.loadingUnits.Any())
                {
                    this.MinLoadingUnitId = this.loadingUnits.Select(s => s.Id).Min();

                    this.MaxLoadingUnitId = this.loadingUnits.Select(s => s.Id).Max();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.loadingUnits.Clear();
                this.SelectedLoadingUnit = null;
                this.currentItemIndex = 0;
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.LoadingUnits));
                this.IsSearching = false;
            }

            return Task.CompletedTask;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.SubscribeToEvents();

            this.IsBackNavigationAllowed = true;

            this.LoadingUnitId = null;

            await this.GetLoadingUnitsAsync();
            this.SelectLoadingUnit();
        }

        public void SelectNextLoadingUnitAsync()
        {
            System.Diagnostics.Debug.Assert(this.currentItemIndex < this.loadingUnits.Count - 1);

            this.currentItemIndex++;
            this.SelectLoadingUnit();
        }

        public void SelectPreviousLoadingUnitAsync()
        {
            System.Diagnostics.Debug.Assert(this.currentItemIndex > 0);

            this.currentItemIndex--;
            this.SelectLoadingUnit();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.callLoadingUnitCommand?.RaiseCanExecuteChanged();
            this.upSelectionCommand?.RaiseCanExecuteChanged();
            this.downSelectionCommand?.RaiseCanExecuteChanged();
        }

        private bool CanCallLoadingUnit()
        {
            return true;
            //return this.SelectedLoadingUnit != null
            //&&
            //this.LoadingUnitId.HasValue
            //&&
            //!this.IsWaitingForResponse
            //&&
            //this.loadingUnits.Any(l => l.Id == this.loadingUnitId);
        }

        private bool CanSelectNextItem()
        {
            return
                this.currentItemIndex < this.loadingUnits.Count - 1
                &&
                !this.IsSearching;
        }

        private bool CanSelectPreviousItem()
        {
            return
                this.currentItemIndex > 0
                &&
                !this.IsSearching;
        }

        private void CheckToSelectLoadingUnit()
        {
            this.Logger.Debug($"CheckToSelectLoadingUnit: loadingUnitId {this.loadingUnitId} ");
            if (this.loadingUnits.FirstOrDefault(l => l.Id == this.loadingUnitId) is LoadingUnit loadingUnitfound)
            {
                if (this.selectedUnitUnit != null && loadingUnitfound.Id == this.selectedUnitUnit.Id)
                {
                    return;
                }

                this.currentItemIndex = this.loadingUnits.IndexOf(loadingUnitfound);
                this.SelectedLoadingUnit = loadingUnitfound;
                //this.SelectLoadingUnit();
            }
            else if (this.loadingUnitId > this.MaxLoadingUnitId || this.loadingUnitId < this.MinLoadingUnitId)
            {
                this.SelectLoadingUnit();
            }
            else
            {
                this.SelectedLoadingUnit = null;
            }
        }

        private void LoadingUnitsMissionsAppear()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.Others.LOADINGUNITSMISSIONS,
                null,
                trackCurrentView: true);
        }

        private void OnHomingProcedureStatusChanged(NotificationMessageUI<HomingMessageData> message)
        {
            if (message.Data.AxisToCalibrate == CommonUtils.Messages.Enumerations.Axis.HorizontalAndVertical)
            {
                if (message.Status == MessageStatus.OperationStart)
                {
                    this.ShowNotification(Resources.Localized.Get("InstallationApp.HorizontalHomingStarted"), Services.Models.NotificationSeverity.Info);
                }
            }
        }

        private void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Data?.MovementMode == MovementMode.BayTest)
            {
                this.ShowNotification(Resources.Localized.Get("OperatorApp.CarouselCalibration"), Services.Models.NotificationSeverity.Info);
            }

            if (message.Data?.MovementMode == MovementMode.HorizontalCalibration)
            {
                this.ShowNotification(Resources.Localized.Get("OperatorApp.HorizontalCalibration"), Services.Models.NotificationSeverity.Info);
            }
        }

        private void SelectLoadingUnit()
        {
            this.SelectedLoadingUnit = this.loadingUnits.ElementAtOrDefault(this.currentItemIndex);
        }

        private void SubscribeToEvents()
        {
            this.positioningMessageReceivedToken = this.positioningMessageReceivedToken
               ??
               this.eventAggregator
                   .GetEvent<NotificationEventUI<PositioningMessageData>>()
                   .Subscribe(
                       this.OnPositioningMessageReceived,
                       ThreadOption.UIThread,
                       false);

            this.receiveHomingUpdateToken = this.receiveHomingUpdateToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        this.OnHomingProcedureStatusChanged,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
