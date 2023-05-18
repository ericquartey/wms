using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Errors.ViewModels
{
    [Warning(WarningsArea.None)]
    internal sealed class ErrorDetailsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly IMachineIdentityWebService machineIdentity;

        private readonly ISessionService sessionService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISessionService>();

        private MachineError error;

        private string errorTime;

        private bool findZeroBayChain;

        private bool findZeroElevator;

        private bool isErrorTopLevelBayOccupiedEmpty;

        private bool isHeightAlarm;

        private bool isVisibleGoTo;

        private SubscriptionToken machineModeChangedToken;

        private ICommand markAsResolvedAndGoCommand;

        private ICommand markAsResolvedCommand;

        #endregion

        #region Constructors

        public ErrorDetailsViewModel(
            IMachineErrorsWebService machineErrorsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineIdentityWebService machineIdentity)
            : base(Services.PresentationMode.Menu | Services.PresentationMode.Installer | Services.PresentationMode.Operator)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineIdentity = machineIdentity ?? throw new ArgumentNullException(nameof(machineIdentity));

            new Timer(this.OnErrorChanged, null, 0, 30 * 1000);
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public MachineError Error
        {
            get => this.error;
            set => this.SetProperty(ref this.error, value, () => this.OnErrorChanged(null));
        }

        public string ErrorTime
        {
            get => this.errorTime;
            set => this.SetProperty(ref this.errorTime, value);
        }

        public bool IsErrorTopLevelBayOccupiedEmpty
        {
            get => this.isErrorTopLevelBayOccupiedEmpty;
            set => this.SetProperty(ref this.isErrorTopLevelBayOccupiedEmpty, value, this.RaiseCanExecuteChanged);
        }

        public bool IsHeightAlarm
        {
            get => this.isHeightAlarm;
            set => this.SetProperty(ref this.isHeightAlarm, value, this.RaiseCanExecuteChanged);
        }

        public bool IsVisibleGoTo
        {
            get => this.isVisibleGoTo;
            set => this.SetProperty(ref this.isVisibleGoTo, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MarkAsResolvedAndGoCommand =>
                   this.markAsResolvedAndGoCommand
           ??
           (this.markAsResolvedAndGoCommand = new DelegateCommand(
               async () => await this.MarkAsResolvedAndGoAsync(),
               this.CanMarkAsResolved)
           .ObservesProperty(() => this.Error)
           .ObservesProperty(() => this.IsWaitingForResponse));

        public ICommand MarkAsResolvedCommand =>
                    this.markAsResolvedCommand
            ??
            (this.markAsResolvedCommand = new DelegateCommand(
                async () => await this.MarkAsResolvedAsync(),
                this.CanMarkAsResolved)
            .ObservesProperty(() => this.Error)
            .ObservesProperty(() => this.IsWaitingForResponse));

        protected bool IsInstaller => this.sessionService.UserAccessLevel >= UserAccessLevel.Installer;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.Error = null;

            if (!this.findZeroElevator && !this.findZeroBayChain)
            {
                this.machineModeChangedToken?.Dispose();
                this.machineModeChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.ResetImageVisibility();

            var isVisible = await this.machineIdentity.GetIsOstecEnableAsync();
            var isEnabled = !await this.machineIdentity.IsSilenceSirenAlarmAsync();
            this.ShowSilenceSiren(isVisible, isEnabled);

            await base.OnAppearedAsync();

            this.ShowPrevStepSinglePage(false, false);
            this.ShowNextStepSinglePage(false, false);
            this.ShowAbortStep(false, false);

            await this.RetrieveErrorAsync();

            this.machineModeChangedToken = this.EventAggregator
               .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
               .Subscribe(
                   this.OnMachineModeChanged,
                   ThreadOption.UIThread,
                   false);

            try
            {
                switch ((MachineErrorCode)this.error?.Code)
                {
                    case MachineErrorCode.CradleNotCompletelyLoaded:
                        break;

                    case MachineErrorCode.ConditionsNotMetForPositioning:
                        break;

                    case MachineErrorCode.ConditionsNotMetForRunning:
                        break;

                    case MachineErrorCode.ConditionsNotMetForHoming:
                        break;

                    case MachineErrorCode.SecurityWasTriggered:
                        break;

                    case MachineErrorCode.SecurityButtonWasTriggered:
                        break;

                    case MachineErrorCode.SecurityBarrierWasTriggered:
                        break;

                    case MachineErrorCode.SecurityLeftSensorWasTriggered:
                        break;

                    case MachineErrorCode.InverterFaultStateDetected:
                        break;

                    case MachineErrorCode.CradleNotCorrectlyLoadedDuringPickup:
                        break;

                    case MachineErrorCode.CradleNotCorrectlyUnloadedDuringDeposit:
                        break;

                    case MachineErrorCode.ZeroSensorErrorAfterPickup:
                        break;

                    case MachineErrorCode.ZeroSensorErrorAfterDeposit:
                        break;

                    case MachineErrorCode.InvalidPresenceSensors:
                        break;

                    case MachineErrorCode.MissingZeroSensorWithEmptyElevator:
                        break;

                    case MachineErrorCode.ZeroSensorActiveWithFullElevator:
                        break;

                    case MachineErrorCode.LoadUnitPresentOnEmptyElevator:
                        break;

                    case MachineErrorCode.TopLevelBayOccupied:
                    case MachineErrorCode.TopLevelBayEmpty:
                        this.IsErrorTopLevelBayOccupiedEmpty = true;
                        break;

                    case MachineErrorCode.BottomLevelBayOccupied:
                        break;

                    case MachineErrorCode.BottomLevelBayEmpty:
                        break;

                    case MachineErrorCode.SensorZeroBayNotActiveAtStart:
                        break;

                    case MachineErrorCode.InverterConnectionError:
                        break;

                    case MachineErrorCode.IoDeviceConnectionError:
                        break;

                    case MachineErrorCode.LaserConnectionError:
                        break;

                    case MachineErrorCode.LoadUnitWeightExceeded:
                        break;

                    case MachineErrorCode.LoadUnitHeightFromBayExceeded:
                        break;

                    case MachineErrorCode.LoadUnitHeightToBayExceeded:
                        break;

                    case MachineErrorCode.LoadUnitWeightTooLow:
                        break;

                    case MachineErrorCode.MachineWeightExceeded:
                        break;

                    case MachineErrorCode.DestinationBelowLowerBound:
                        break;

                    case MachineErrorCode.DestinationOverUpperBound:
                        break;

                    case MachineErrorCode.BayInvertersBusy:
                        break;

                    case MachineErrorCode.IoDeviceError:
                        break;

                    case MachineErrorCode.MachineModeNotValid:
                        break;

                    case MachineErrorCode.AnotherMissionIsActiveForThisLoadUnit:
                        break;

                    case MachineErrorCode.AnotherMissionIsActiveForThisBay:
                        break;

                    case MachineErrorCode.AnotherMissionOfThisTypeIsActive:
                        break;

                    case MachineErrorCode.WarehouseIsFull:
                        break;

                    case MachineErrorCode.CellLogicallyOccupied:
                        break;

                    case MachineErrorCode.MoveBayChainNotAllowed:
                        break;

                    case MachineErrorCode.AutomaticRestoreNotAllowed:
                        break;

                    case MachineErrorCode.DestinationTypeNotValid:
                        break;

                    case MachineErrorCode.MissionTypeNotValid:
                        break;

                    case MachineErrorCode.ResumeCommandNotValid:
                        break;

                    case MachineErrorCode.DestinationBayNotCalibrated:
                        break;

                    case MachineErrorCode.NoLoadUnitInSource:
                        break;

                    case MachineErrorCode.LoadUnitSourceDb:
                        break;

                    case MachineErrorCode.LoadUnitDestinationCell:
                        break;

                    case MachineErrorCode.LoadUnitElevator:
                        break;

                    case MachineErrorCode.LoadUnitNotRemoved:
                        break;

                    case MachineErrorCode.LoadUnitDestinationBay:
                        break;

                    case MachineErrorCode.LoadUnitSourceCell:
                        break;

                    case MachineErrorCode.LoadUnitNotFound:
                        break;

                    case MachineErrorCode.LoadUnitNotLoaded:
                        break;

                    case MachineErrorCode.LoadUnitSourceBay:
                        break;

                    case MachineErrorCode.LoadUnitShutterOpen:
                        break;

                    case MachineErrorCode.LoadUnitShutterInvalid:
                        break;

                    case MachineErrorCode.LoadUnitShutterClosed:
                        break;

                    case MachineErrorCode.LoadUnitPresentInCell:
                        break;

                    case MachineErrorCode.LoadUnitOtherBay:
                        break;

                    case MachineErrorCode.LoadUnitSourceElevator:
                        break;

                    case MachineErrorCode.LoadUnitMissingOnElevator:
                        break;

                    case MachineErrorCode.LoadUnitMissingOnBay:
                        break;

                    case MachineErrorCode.LoadUnitUndefinedUpper:
                        break;

                    case MachineErrorCode.LoadUnitUndefinedBottom:
                        break;

                    case MachineErrorCode.FirstTestFailed:
                        break;

                    case MachineErrorCode.FullTestFailed:
                        break;

                    case MachineErrorCode.WarehouseNotEmpty:
                        break;

                    case MachineErrorCode.SensorZeroBayNotActiveAtEnd:
                        break;

                    case MachineErrorCode.SecurityRightSensorWasTriggered:
                        break;

                    case MachineErrorCode.VerticalPositionChanged:
                        break;

                    case MachineErrorCode.InvalidBay:
                        break;

                    case MachineErrorCode.InvalidPositionBay:
                        break;

                    case MachineErrorCode.ElevatorOverrunDetected:
                        break;

                    case MachineErrorCode.ElevatorUnderrunDetected:
                        break;

                    case MachineErrorCode.ExternalBayEmpty:
                        break;

                    case MachineErrorCode.ExternalBayOccupied:
                        break;

                    case MachineErrorCode.WmsError:
                        break;

                    case MachineErrorCode.BayPositionDisabled:
                        break;

                    case MachineErrorCode.MoveExtBayNotAllowed:
                        break;

                    case MachineErrorCode.StartPositioningBlocked:
                        break;

                    case MachineErrorCode.InverterCommandTimeout:
                        break;

                    case MachineErrorCode.IoDeviceCommandTimeout:
                        break;

                    case MachineErrorCode.TelescopicBayError:
                        break;

                    case MachineErrorCode.LoadUnitTareError:
                        break;

                    case MachineErrorCode.VerticalZeroLowError:
                        break;

                    case MachineErrorCode.VerticalZeroHighError:
                        break;

                    case MachineErrorCode.LoadUnitHeightFromBayTooLow:
                        break;

                    case MachineErrorCode.PreFireAlarm:
                        break;

                    case MachineErrorCode.FireAlarm:
                        break;

                    case MachineErrorCode.BackupDatabaseOnServer:
                        break;

                    case MachineErrorCode.InverterErrorBaseCode:
                        break;

                    case MachineErrorCode.InverterErrorInvalidParameter:
                        break;

                    case MachineErrorCode.InverterErrorInvalidDataset:
                        break;

                    case MachineErrorCode.InverterErrorParameterIsWriteOnly:
                        break;

                    case MachineErrorCode.InverterErrorParameterIsReadOnly:
                        break;

                    case MachineErrorCode.InverterErrorEepromReadError:
                        break;

                    case MachineErrorCode.InverterErrorEepromWriteError:
                        break;

                    case MachineErrorCode.InverterErrorEepromChecksumError:
                        break;

                    case MachineErrorCode.InverterErrorCannotWriteParameterWhileRunning:
                        break;

                    case MachineErrorCode.InverterErrorDatasetValuesAreDifferent:
                        break;

                    case MachineErrorCode.InverterErrorUnknownParameter:
                        break;

                    case MachineErrorCode.InverterErrorSyntaxError:
                        break;

                    case MachineErrorCode.InverterErrorWrongPayloadLength:
                        break;

                    case MachineErrorCode.InverterErrorNodeNotAvailable:
                        break;

                    case MachineErrorCode.InverterErrorSyntaxError2:
                        break;

                    case MachineErrorCode.NoError:
                        break;

                    default:
                        break;
                }
            }
            catch (Exception) { }

            if (this.error.Code == (int)MachineErrorCode.HeightAlarm)
            {
                await this.MachineService.StopMovingByAllAsync();
            }
        }

        public void ResetImageVisibility()
        {
            this.IsErrorTopLevelBayOccupiedEmpty = false;
        }

        private bool CanMarkAsResolved()
        {
            return
                this.Error != null
                &&
                !this.IsWaitingForResponse
                &&
                (!this.IsHeightAlarm || this.IsInstaller);
        }

        private async Task MarkAsResolvedAndGoAsync()
        {
            if (this.Error is null)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineErrorsWebService.ResolveAsync(this.Error.Id);

                // await this.machineErrorsWebService.ResolveAllAsync();

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.DrawerCompacting.MAIN,
                    null,
                    trackCurrentView: false);

                this.Error = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MarkAsResolvedAsync()
        {
            if (this.Error is null)
            {
                return;
            }

            if (this.IsHeightAlarm)
            {
                this.machineIdentity.SetHeightAlarmAsync(false);
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineErrorsWebService.ResolveAsync(this.Error.Id);

                // await this.machineErrorsWebService.ResolveAllAsync();
                this.Error = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnErrorChanged(object state)
        {
            if (this.error is null)
            {
                this.ErrorTime = null;
                this.IsVisibleGoTo = false;
                return;
            }

            if (this.error.Code == (int)MachineErrorCode.WarehouseIsFull)
            {
                this.IsVisibleGoTo = true;
            }
            else
            {
                this.IsVisibleGoTo = false;
            }

            var elapsedTime = DateTime.UtcNow - this.error.OccurrenceDate;
            if (elapsedTime.TotalMinutes < 1)
            {
                this.ErrorTime = Resources.Localized.Get("General.Now");
            }
            else if (elapsedTime.TotalHours < 1)
            {
                this.ErrorTime = string.Format(Resources.General.MinutesAgo, elapsedTime.TotalMinutes);
            }
            else if (elapsedTime.TotalDays < 1)
            {
                this.ErrorTime = string.Format(Resources.General.HoursAgo, elapsedTime.TotalHours);
            }
            else
            {
                this.ErrorTime = string.Format(Resources.General.DaysAgo, elapsedTime.TotalDays);
            }
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            if (e.MachineMode != MachineMode.Manual)
            {
                this.findZeroElevator = false;
            }
        }

        private async Task RetrieveErrorAsync()
        {
            try
            {
                // reset the command
                this.findZeroElevator = false;
                this.findZeroBayChain = false;

                this.IsWaitingForResponse = true;

                this.Error = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
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
