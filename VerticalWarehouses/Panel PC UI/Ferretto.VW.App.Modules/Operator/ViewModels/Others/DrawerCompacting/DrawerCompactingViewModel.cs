using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Maintenance)]
    public class DrawerCompactingViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineAutoCompactingSettingsWebService machineAutoCompactingSettingsWebService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineCompactingWebService machineCompactingWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand compactingStartCommand;

        private DelegateCommand compactingStopCommand;

        private DelegateCommand daysCountCommand;

        private DelegateCommand detailButtonCommand;

        private DelegateCommand fastCompactingStartCommand;

        private double fragmentBackPercent;

        private double fragmentFrontPercent;

        private double fragmentTotalPercent;

        private bool isEnabledReorder;

        private bool isInstaller;

        private bool isReorder;

        private bool isRotationClassEnabled;

        private bool isStopPressed;

        private double maxSolidSpaceBack;

        private double maxSolidSpaceFront;

        private SubscriptionToken positioningOperationChangedToken;

        private DelegateCommand settingsButtonCommand;

        private bool showAutoCompactingSettings;

        private int totalDrawers;

        #endregion

        #region Constructors

        public DrawerCompactingViewModel(
            IMachineIdentityWebService machineIdentityWebService,
            IMachineCompactingWebService machineCompactingWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineAutoCompactingSettingsWebService machineAutoCompactingSettingsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineCompactingWebService = machineCompactingWebService ?? throw new ArgumentNullException(nameof(machineCompactingWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineAutoCompactingSettingsWebService = machineAutoCompactingSettingsWebService ?? throw new ArgumentNullException(nameof(machineAutoCompactingSettingsWebService));

            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();
        }

        #endregion

        #region Properties

        public ICommand CompactingStartCommand =>
            this.compactingStartCommand
            ??
            (this.compactingStartCommand =
                new DelegateCommand(
                    async () => await this.StartAsync(),
                    this.CanCompactingStart));

        public ICommand CompactingStopCommand =>
            this.compactingStopCommand
            ??
            (this.compactingStopCommand =
                new DelegateCommand(
                    async () => await this.StopAsync(),
                    this.CanCompactingStop));

        public ICommand DaysCountCommand =>
            this.daysCountCommand
            ??
            (this.daysCountCommand =
                new DelegateCommand(
                    () => this.DaysCount(),
                    this.CanDaysCountCommand));

        public ICommand DetailButtonCommand =>
            this.detailButtonCommand
            ??
            (this.detailButtonCommand =
                new DelegateCommand(
                    () => this.Detail(),
                    this.CanDetailCommand));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand FastCompactingStartCommand =>
                                            this.fastCompactingStartCommand
            ??
            (this.fastCompactingStartCommand =
                new DelegateCommand(
                    async () => await this.FastStartAsync(),
                    this.CanCompactingStart));

        public double FragmentBackPercent
        {
            get => this.fragmentBackPercent;
            set => this.SetProperty(ref this.fragmentBackPercent, value, this.RaiseCanExecuteChanged);
        }

        public double FragmentFrontPercent
        {
            get => this.fragmentFrontPercent;
            set => this.SetProperty(ref this.fragmentFrontPercent, value, this.RaiseCanExecuteChanged);
        }

        public double FragmentTotalPercent
        {
            get => this.fragmentTotalPercent;
            set => this.SetProperty(ref this.fragmentTotalPercent, value, this.RaiseCanExecuteChanged);
        }

        public bool IsEnabledReorder
        {
            get => this.isEnabledReorder;
            set => this.SetProperty(ref this.isEnabledReorder, value, this.RaiseCanExecuteChanged);
        }

        public bool IsInstallerAndRotationClassEnable
        {
            get => this.isInstaller;
            set => this.SetProperty(ref this.isInstaller, value, this.RaiseCanExecuteChanged);
        }

        public bool IsReorder
        {
            get => this.isReorder;
            set => this.SetProperty(ref this.isReorder, value, this.RaiseCanExecuteChanged);
        }

        public bool IsRotationClassEnabled
        {
            get => this.isRotationClassEnabled;
            set => this.SetProperty(ref this.isRotationClassEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsStopPressed
        {
            get => this.isStopPressed;
            protected set => this.SetProperty(ref this.isStopPressed, value, this.RaiseCanExecuteChanged);
        }

        public double MaxSolidSpaceBack
        {
            get => this.maxSolidSpaceBack;
            set => this.SetProperty(ref this.maxSolidSpaceBack, value, this.RaiseCanExecuteChanged);
        }

        public double MaxSolidSpaceFront
        {
            get => this.maxSolidSpaceFront;
            set => this.SetProperty(ref this.maxSolidSpaceFront, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SettingsButtonCommand =>
            this.settingsButtonCommand
            ??
            (this.settingsButtonCommand =
                new DelegateCommand(
                    () => this.Settings(),
                    this.CanSettingsCommand));

        public bool ShowAutoCompactingSettings
        {
            get => this.showAutoCompactingSettings;
            set => this.SetProperty(ref this.showAutoCompactingSettings, value, this.RaiseCanExecuteChanged);
        }

        public int TotalDrawers
        {
            get => this.totalDrawers;
            protected set => this.SetProperty(ref this.totalDrawers, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.positioningOperationChangedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<ProfileCalibrationMessageData>>().Unsubscribe(this.positioningOperationChangedToken);
                this.positioningOperationChangedToken?.Dispose();
                this.positioningOperationChangedToken = null;

                this.ShowAutoCompactingSettings = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            //this.positioningOperationChangedToken = this.positioningOperationChangedToken
            //    ??
            //    this.EventAggregator
            //        .GetEvent<NotificationEventUI<MissionOperationCompletedMessageData>>()
            //        .Subscribe(
            //            async m => await this.OnPositioningOperationChangedAsync(m),
            //            ThreadOption.UIThread,
            //            false,
            //            m => this.IsVisible);

            var list = await this.machineAutoCompactingSettingsWebService.GetAllAutoCompactingSettingsAsync();

            this.ShowAutoCompactingSettings = list.Any() || this.sessionService.UserAccessLevel > UserAccessLevel.Movement;
            this.IsRotationClassEnabled = await this.machineIdentityWebService.GetIsRotationClassAsync();
            this.IsReorder = await this.machineIdentityWebService.GetIsRotationClassAsync();

            this.IsInstallerAndRotationClassEnable = this.sessionService.UserAccessLevel > UserAccessLevel.Movement && this.IsRotationClassEnabled;

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.RefreshAllValue();

                var unit = await this.machineLoadingUnitsWebService.GetAllAsync();
                this.TotalDrawers = unit.Count(n => n.IsIntoMachineOK);

                await base.OnDataRefreshAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            //if (this.MachineService.MachineMode == MachineMode.Compact || this.MachineService.MachineMode == MachineMode.Manual)
            {
                if (e.MachineStatus.MessageStatus == CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd)
                {
                    await this.RefreshAllValue();
                }
            }

            if (!this.IsMachineMoving)
            {
                this.IsStopPressed = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.detailButtonCommand?.RaiseCanExecuteChanged();
            this.compactingStartCommand?.RaiseCanExecuteChanged();
            this.compactingStopCommand?.RaiseCanExecuteChanged();
            this.fastCompactingStartCommand?.RaiseCanExecuteChanged();
            this.daysCountCommand?.RaiseCanExecuteChanged();
            this.settingsButtonCommand?.RaiseCanExecuteChanged();

            this.IsEnabledReorder = this.CanCompactingStart();
        }

        private bool CanCompactingStart()
        {
            var result = !this.IsWaitingForResponse &&
                   this.MachineService.MachinePower == MachinePowerState.Powered &&
                   (this.MachineService.HasShutter || this.MachineService.Bay.CurrentMission is null) &&
                   !this.IsMachineMoving &&
                   this.SensorsService.IsZeroChain;

            if (result)
            {
                switch (this.MachineService.BayNumber)
                {
                    case BayNumber.BayOne:
                    default:
                        result = this.MachineModeService.MachineMode == MachineMode.Manual;
                        break;

                    case BayNumber.BayTwo:
                        result = this.MachineModeService.MachineMode == MachineMode.Manual2;
                        break;

                    case BayNumber.BayThree:
                        result = this.MachineModeService.MachineMode == MachineMode.Manual3;
                        break;
                }
            }

            if (result)
            {
                this.OnDataRefreshAsync();
            }

            return result;
        }

        private bool CanCompactingStop()
        {
            return !this.IsWaitingForResponse &&
                   (this.IsMachineMoving || this.IsMoving) &&
                   !this.IsStopPressed;
        }

        private bool CanDaysCountCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanDetailCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanSettingsCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private void DaysCount()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.DrawerCompacting.DAYSCOUNT,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void Detail()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.DrawerCompacting.DETAIL,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task FastStartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineCompactingWebService.FastCompactingAsync(this.IsReorder);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task RefreshAllValue()
        {
            try
            {
                var cells = await this.machineCellsWebService.GetStatisticsAsync();
                this.FragmentBackPercent = cells.FragmentBackPercent;
                this.FragmentFrontPercent = cells.FragmentFrontPercent;
                this.FragmentTotalPercent = cells.FragmentTotalPercent;

                foreach (var spaceSide in cells.MaxSolidSpace)
                {
                    if (spaceSide.Key == WarehouseSide.Front)
                    {
                        this.MaxSolidSpaceFront = spaceSide.Value;
                    }
                    else if (spaceSide.Key == WarehouseSide.Back)
                    {
                        this.MaxSolidSpaceBack = spaceSide.Value;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void Settings()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.DrawerCompacting.SETTINGS,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        //private async Task OnPositioningOperationChangedAsync(NotificationMessageUI<MissionOperationCompletedMessageData> message)
        //{
        //    switch (message.Status)
        //    {
        //        case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
        //            {
        //                await this.RefreshAllValue();

        //                await base.OnDataRefreshAsync();
        //                break;
        //            }
        //    }
        //}

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineCompactingWebService.CompactingAsync(this.IsReorder);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineCompactingWebService.StopAsync();

                this.IsStopPressed = true;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
