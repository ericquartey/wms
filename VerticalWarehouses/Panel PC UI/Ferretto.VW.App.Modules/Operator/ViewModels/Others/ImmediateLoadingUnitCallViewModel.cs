﻿using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Http;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.User)]
    public class ImmediateLoadingUnitCallViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineService machineService;

        private readonly ISessionService sessionService;

        private DelegateCommand callLoadingUnitCommand;

        private DelegateCommand changeLaserOffsetCommand;

        private DelegateCommand changeLoadUnitFixedCommand;

        private DelegateCommand changeRotationClassCommand;

        private bool isEnabledLaser;

        private bool isLoadUnitFixedEnabled;

        private bool isRotationClassEnabled;

        private bool isUserLimited;

        private int? loadingUnitId;

        private List<LoadingUnit> loadingUnits;

        private DelegateCommand loadingUnitsMissionsCommand;

        private SubscriptionToken loadUnitsChangedToken;

        private int maxLoadingUnitId;

        private int minLoadingUnitId;

        private SubscriptionToken positioningMessageReceivedToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private LoadingUnit selectedLoadingUnit;

        private List<LoadingUnit> selectedUnits;

        #endregion

        #region Constructors

        public ImmediateLoadingUnitCallViewModel(
            ISessionService sessionService,
            IMachineService machineService,
            IMachineIdentityWebService machineIdentityWebService,
            IEventAggregator eventAggregator,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IAuthenticationService authenticationService)
            : base(PresentationMode.Operator)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
        }

        #endregion

        #region Properties

        public ICommand ChangeLaserOffsetCommand =>
            this.changeLaserOffsetCommand
            ??
            (this.changeLaserOffsetCommand = new DelegateCommand(this.ChangeLaserOffsetAppear, this.CanChangeLaserOffset));

        public ICommand ChangeLoadUnitFixedCommand =>
           this.changeLoadUnitFixedCommand
           ??
           (this.changeLoadUnitFixedCommand = new DelegateCommand(this.ChangeLoadUnitFixedAppear, this.CanChangeLoadUnitFixed));

        public ICommand ChangeRotationClassCommand =>
                   this.changeRotationClassCommand
           ??
           (this.changeRotationClassCommand = new DelegateCommand(this.ChangeRotationClassAppear, this.CanChangeRotationClass));

        public bool IsEnabledLaser
        {
            get => this.isEnabledLaser;
            set => this.SetProperty(ref this.isEnabledLaser, value, this.RaiseCanExecuteChanged);
        }

        public bool IsLoadUnitFixedEnabled
        {
            get => this.isLoadUnitFixedEnabled;
            set => this.SetProperty(ref this.isLoadUnitFixedEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsOperator => this.sessionService.UserAccessLevel <= MAS.AutomationService.Contracts.UserAccessLevel.Movement;

        public bool IsRotationClassEnabled
        {
            get => this.isRotationClassEnabled;
            set => this.SetProperty(ref this.isRotationClassEnabled, value, this.RaiseCanExecuteChanged);
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
                if (this.SetProperty(ref this.loadingUnitId, value))
                {
                    if (this.LoadingUnits.Any(s => s.Id == this.loadingUnitId))
                    {
                        this.selectedLoadingUnit = this.LoadingUnits.SingleOrDefault(s => s.Id == this.loadingUnitId);
                    }
                    else
                    {
                        this.selectedLoadingUnit = null;
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public List<LoadingUnit> LoadingUnits
        {
            get => this.loadingUnits;
            set => this.SetProperty(ref this.loadingUnits, value);
        }

        //public IEnumerable<LoadingUnit> LoadingUnits => this.MachineService.Loadunits;
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
            get => this.selectedLoadingUnit;
            set
            {
                if (this.SetProperty(ref this.selectedLoadingUnit, value))
                {
                    this.loadingUnitId = this.selectedLoadingUnit?.Id;

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public List<LoadingUnit> SelectedUnits
        {
            get => this.selectedUnits;
            set => this.SetProperty(ref this.selectedUnits, value);
        }

        #endregion

        #region Methods

        public async Task CallLoadingUnitAsync()
        {
            if (this.selectedLoadingUnit == null)
            {
                this.ShowNotification(Resources.Localized.Get("General.IdLoadingUnitNotExists"), Services.Models.NotificationSeverity.Warning);
                return;
            }

            this.Logger.Debug($"CallLoadingUnitAsync: loadingUnitId {this.selectedLoadingUnit.Id} ");

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.MoveToBayAsync(this.selectedLoadingUnit.Id, this.authenticationService.UserName);

                this.ShowNotification(string.Format(Resources.Localized.Get("ServiceMachine.LoadingUnitSuccessfullyRequested"), this.selectedLoadingUnit?.Id), Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) // when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (ex is MasWebApiException webEx
                    && webEx.StatusCode == StatusCodes.Status403Forbidden)
                {
                    this.ShowNotification(Resources.Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    this.ShowNotification(ex);
                }
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.LoadingUnitId = null;
            }
        }

        public override void Disappear()
        {
            base.Disappear();

            if (this.positioningMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>().Unsubscribe(this.positioningMessageReceivedToken);
                this.positioningMessageReceivedToken?.Dispose();
                this.positioningMessageReceivedToken = null;
            }
            if (this.receiveHomingUpdateToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<HomingMessageData>>().Unsubscribe(this.receiveHomingUpdateToken);
                this.receiveHomingUpdateToken?.Dispose();
                this.receiveHomingUpdateToken = null;
            }
            if (this.loadUnitsChangedToken != null)
            {
                this.EventAggregator.GetEvent<LoadUnitsChangedPubSubEvent>().Unsubscribe(this.loadUnitsChangedToken);
                this.loadUnitsChangedToken?.Dispose();
                this.loadUnitsChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
            this.LoadingUnitId = null;

            this.IsBackNavigationAllowed = true;

            this.IsRotationClassEnabled = await this.machineIdentityWebService.GetIsRotationClassAsync();
            this.IsLoadUnitFixedEnabled = await this.machineIdentityWebService.GetIsLoadUnitFixedAsync();

            await this.MachineService.GetLoadUnits(details: true);
            this.loadingUnits = this.MachineService.Loadunits.ToList();
            this.RaisePropertyChanged(nameof(this.LoadingUnits));

            if (this.LoadingUnits.Any())
            {
                this.MinLoadingUnitId = this.loadingUnits.Select(s => s.Id).Min();
                this.MaxLoadingUnitId = this.loadingUnits.Select(s => s.Id).Max();
                this.LoadingUnitId = null;

                var bay = this.MachineService.Bay;
                this.IsEnabledLaser = bay?.Accessories?.LaserPointer?.IsEnabledNew ?? false;
                this.changeLaserOffsetCommand?.RaiseCanExecuteChanged();
                this.changeRotationClassCommand?.RaiseCanExecuteChanged();
                this.changeLoadUnitFixedCommand?.RaiseCanExecuteChanged();
                if (!string.IsNullOrEmpty(this.authenticationService.UserName))
                {
                    this.isUserLimited = await this.MachineUsersWebService.GetIsLimitedAsync(this.authenticationService.UserName);
                }
            }
            this.SubscribeToEvents();
        }

        protected override void RaiseCanExecuteChanged()
        {
            if (this.IsVisible)
            {
                base.RaiseCanExecuteChanged();

                this.callLoadingUnitCommand?.RaiseCanExecuteChanged();
                this.changeLaserOffsetCommand?.RaiseCanExecuteChanged();
                this.changeRotationClassCommand?.RaiseCanExecuteChanged();
                this.changeLoadUnitFixedCommand?.RaiseCanExecuteChanged();

                if (this.selectedLoadingUnit == null)
                {
                    this.loadingUnitId = null;
                }

                this.RaisePropertyChanged(nameof(this.SelectedLoadingUnit));
                this.RaisePropertyChanged(nameof(this.LoadingUnitId));
                this.RaisePropertyChanged(nameof(this.IsOperator));
            }
        }

        private bool CanCallLoadingUnit()
        {
            //return true;
            return this.SelectedLoadingUnit != null
            &&
            this.LoadingUnitId.HasValue
            &&
            !this.IsWaitingForResponse
            &&
            this.SelectedLoadingUnit.IsIntoMachineOK
            &&
            !this.isUserLimited;
        }

        private bool CanChangeLaserOffset()
        {
            return this.IsEnabledLaser
                && this.selectedLoadingUnit != null
                && this.selectedLoadingUnit.Id > 0
                && !this.isUserLimited;
        }

        private bool CanChangeLoadUnitFixed()
        {
            return this.IsLoadUnitFixedEnabled
                && this.selectedLoadingUnit != null
                && this.selectedLoadingUnit.Id > 0
                && !this.isUserLimited
                && (this.selectedLoadingUnit.CellId.HasValue || this.selectedLoadingUnit.IsCellFixed);
        }

        private bool CanChangeRotationClass()
        {
            return this.IsRotationClassEnabled
                && this.selectedLoadingUnit != null
                && this.selectedLoadingUnit.Id > 0
                && !this.isUserLimited
                && !this.selectedLoadingUnit.IsCellFixed;
        }

        private void ChangeLaserOffsetAppear()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.Others.CHANGELASEROFFSET,
                this.SelectedLoadingUnit,
                trackCurrentView: true);
        }

        private void ChangeLoadUnitFixedAppear()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.Others.CHANGELOADUNITFIXED,
                this.SelectedLoadingUnit,
                trackCurrentView: true);
        }

        private void ChangeRotationClassAppear()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.Others.CHANGEROTATIONCLASS,
                this.SelectedLoadingUnit,
                trackCurrentView: true);
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

        private void OnLoadUnitsUpdate(LoadUnitsChangedMessage obj)
        {
            if (this.IsVisible)
            {
                this.IsWaitingForResponse = true;
                var oldId = this.LoadingUnitId;
                this.LoadingUnits = obj.Loadunits.ToList();
                this.LoadingUnitId = oldId;
                this.IsWaitingForResponse = false;
                if (this.LoadingUnits.Any())
                {
                    this.MinLoadingUnitId = this.loadingUnits.Select(s => s.Id).Min();
                    this.MaxLoadingUnitId = this.loadingUnits.Select(s => s.Id).Max();
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

            this.loadUnitsChangedToken = this.loadUnitsChangedToken
                ?? this.EventAggregator
                    .GetEvent<LoadUnitsChangedPubSubEvent>()
                    .Subscribe(
                        this.OnLoadUnitsUpdate,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
