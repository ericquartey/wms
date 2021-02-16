using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineCompactingWebService machineCompactingWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private DelegateCommand compactingStartCommand;

        private DelegateCommand compactingStopCommand;

        private DelegateCommand detailButtonCommand;

        private double fragmentBackPercent;

        private double fragmentFrontPercent;

        private double fragmentTotalPercent;

        private bool isStopPressed;

        private double maxSolidSpaceBack;

        private double maxSolidSpaceFront;

        private SubscriptionToken positioningOperationChangedToken;

        private int totalDrawers;

        #endregion

        #region Constructors

        public DrawerCompactingViewModel(
            IMachineCompactingWebService machineCompactingWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineCompactingWebService = machineCompactingWebService ?? throw new ArgumentNullException(nameof(machineCompactingWebService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
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

        public ICommand DetailButtonCommand =>
            this.detailButtonCommand
            ??
            (this.detailButtonCommand =
                new DelegateCommand(
                    () => this.Detail(),
                    this.CanDetailCommand));

        public override EnableMask EnableMask => EnableMask.Any;

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

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.RefreshAllValue();

                var unit = await this.machineLoadingUnitsWebService.GetAllAsync();
                this.TotalDrawers = unit.Count(n => n.IsIntoMachine);

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

        private bool CanDetailCommand()
        {
            return !this.IsWaitingForResponse;
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

                await this.machineCompactingWebService.CompactingAsync();
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
