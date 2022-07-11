using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum BayCheckStep
    {
        PositionUp,

        PositionDown,

        Confirm,
    }

    [Warning(WarningsArea.Installation)]
    internal sealed class BayCheckViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IDialogService dialogService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineShuttersWebService machineShuttersWebService;

        private readonly IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService;

        private BayCheckStep currentStep;

        private DelegateCommand displacementCommand;

        private decimal? displacementDown;

        private decimal? displacementUp;

        private double lowerBound;

        private DelegateCommand moveToBayPositionCommand;

        private DelegateCommand moveToNextCommand;

        private DelegateCommand moveToShutterCommand;

        private double newDisplacementDown;

        private double newDisplacementUp;

        private DelegateCommand saveCommand;

        private SubscriptionToken stepChangedToken;

        private double stepValueDown;

        private double stepValueUp;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        #endregion

        #region Constructors

        public BayCheckViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineShuttersWebService machineShuttersWebService,
            IMachineBaysWebService machineBaysWebService,
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            IDialogService dialogService)
            : base(PresentationMode.Installer)
        {
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineShuttersWebService = machineShuttersWebService ?? throw new ArgumentNullException(nameof(machineShuttersWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            this.CurrentStep = BayCheckStep.PositionUp;
        }

        #endregion

        #region Properties

        public Bay Bay => this.MachineService.Bay;

        public int BayNumber => (int)this.MachineService.BayNumber;

        public BayPosition BayPositionActive =>
            this.currentStep is BayCheckStep.PositionUp ?
                this.Bay?.Positions.OrderBy(m => m.Height).Last() :
                this.Bay?.Positions.OrderBy(m => m.Height).First();

        public string CurrentBayPosition => this.currentStep is BayCheckStep.PositionUp ? Localized.Get("InstallationApp.PositionOnTop") : Localized.Get("InstallationApp.PositionOnBotton");

        public BayCheckStep CurrentStep
        {
            get => this.currentStep;
            set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public ICommand DisplacementCommand =>
            this.displacementCommand
            ??
            (this.displacementCommand = new DelegateCommand(
                async () => await this.DisplacementCommandAsync(),
                this.CanDisplacementCommand));

        public decimal? DisplacementDown
        {
            get => this.displacementDown;
            set => this.SetProperty(ref this.displacementDown, value);
        }

        public decimal? DisplacementUp
        {
            get => this.displacementUp;
            set => this.SetProperty(ref this.displacementUp, value);
        }

        public bool HasDisplacementDownValue => this.DisplacementDown != null;

        public bool HasDisplacementUpValue => this.DisplacementUp != null;

        public bool HasDisplacementValue => this.HasDisplacementUpValue || this.HasDisplacementDownValue;

        public bool HasStepConfirm => this.currentStep is BayCheckStep.Confirm;

        public bool HasStepPositionDown => this.currentStep is BayCheckStep.PositionDown;

        public bool HasStepPositionDownVisible => this.Bay?.IsDouble ?? false;

        public bool HasStepPositionUp => this.currentStep is BayCheckStep.PositionUp;

        public bool IsCanStepValue => this.CanBaseExecute();

        public bool HasShutter => this.MachineService.HasShutter;

        public double LowerBound
        {
            get => this.lowerBound;
            set => this.SetProperty(ref this.lowerBound, value);
        }

        public ICommand MoveToBayPositionCommand =>
            this.moveToBayPositionCommand
            ??
            (this.moveToBayPositionCommand = new DelegateCommand(
                async () => await this.MoveToBayPositionAsync(),
                () =>
                {
                    var res = this.CanMoveToBayPosition();
                    if (res)
                    {
                        try
                        {
                            var policy = Task.Run(async () => await this.machineElevatorWebService.CanMoveToBayPositionAsync(this.BayPositionActive.Id).ConfigureAwait(false)).GetAwaiter().GetResult();
                            res &= policy?.IsAllowed == true;
                            if (!res)
                            {
                                this.Logger.Debug($"Policy.IsAllowed = false ({policy.Reason})");
                            }
                        }
                        catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                        {
                            this.Logger.Error(ex);
                            res = false;
                            this.ShowNotification(ex);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Error(e);
                            throw;
                        }
                    }

                    return res;
                }));

        public ICommand MoveToNextCommand =>
            this.moveToNextCommand
            ??
            (this.moveToNextCommand = new DelegateCommand(
                () =>
                {
                    this.CurrentStep =
                        this.CurrentStep == BayCheckStep.PositionUp && (this.Bay?.IsDouble ?? false) ?
                            BayCheckStep.PositionDown :
                            BayCheckStep.Confirm;
                },
                this.CanBaseExecute));

        public ICommand MoveToShutterCommand =>
            this.moveToShutterCommand
            ??
            (this.moveToShutterCommand = new DelegateCommand(
                async () => await this.MoveToShutterAsync(),
                this.CanBaseExecute));

        public double NewPositionDown
        {
            get => this.newDisplacementDown;
            set => this.SetProperty(ref this.newDisplacementDown, value);
        }

        public double NewPositionUp
        {
            get => this.newDisplacementUp;
            set => this.SetProperty(ref this.newDisplacementUp, value);
        }

        public int NumberStepConfirm => (this.Bay?.IsDouble ?? false) ? 3 : 2;

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync(),
                () => this.DisplacementUp != null ||
                      this.DisplacementDown != null));

        public string ShutterLabel => this.SensorsService.ShutterSensors.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");

        public double StepValueDown
        {
            get => this.stepValueDown;
            set => this.SetProperty(ref this.stepValueDown, value, this.RaiseCanExecuteChanged);
        }

        public double StepValueUp
        {
            get => this.stepValueUp;
            set => this.SetProperty(ref this.stepValueUp, value, this.RaiseCanExecuteChanged);
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.MachineService.PropertyChanged -= this.MachineService_PropertyChanged;

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
                this.stepChangedToken = null;
            }

            if (this.themeChangedToken != null)
            {
                this.EventAggregator.GetEvent<ThemeChangedPubSubEvent>().Unsubscribe(this.themeChangedToken);
                this.themeChangedToken?.Dispose();
                this.themeChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.UpdateStatusButtonFooter();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.SensorsService.RefreshAsync(true);
                await this.GetLowerBound();
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

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsCanStepValue));
            this.RaisePropertyChanged(nameof(this.BayPositionActive));

            this.displacementCommand?.RaiseCanExecuteChanged();
            this.moveToNextCommand?.RaiseCanExecuteChanged();
            this.moveToBayPositionCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.moveToShutterCommand?.RaiseCanExecuteChanged();
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                if (this.DisplacementDown != null && this.LowerBound > this.NewPositionDown)
                {
                    var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ModifyLowerBoundDialog"), Localized.Get("InstallationApp.LowPositionControl"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.No)
                    {
                        return;
                    }

                    await this.machineElevatorWebService.UpdateVerticalLowerBoundAsync(this.NewPositionDown);

                    await this.GetLowerBound();
                }

                this.IsWaitingForResponse = true;

                // Up
                if (this.DisplacementUp != null)
                {
                    var bayPositionUp = this.Bay?.Positions.OrderBy(m => m.Height).Last();
                    await this.machineBaysWebService.UpdateHeightAsync(
                        1, // Valore fisso
                        this.NewPositionUp);

                    this.DisplacementUp = null;
                }

                // Down
                if (this.DisplacementDown != null)
                {
                    var bayPositionDown = this.Bay?.Positions.OrderBy(m => m.Height).First();
                    await this.machineBaysWebService.UpdateHeightAsync(
                        2, // Valore fisso
                        this.NewPositionDown);

                    this.DisplacementDown = null;
                }

                // Forzo l'aggiornamento della machine service per aggiornare le modifiche appena apportate
                await this.MachineService.OnUpdateServiceAsync();

                this.CurrentStep = BayCheckStep.PositionUp;

                this.ShowNotification(
                    VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                    Services.Models.NotificationSeverity.Success);

                this.NavigationService.GoBack();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsMoving;
        }

        private bool CanDisplacementCommand()
        {
            return this.CanBaseExecute() &&
                   (!this.HasShutter || this.SensorsService.ShutterSensors.Closed ||
                    (this.SensorsService.ShutterSensors.MidWay && this.MachineService.Bay.Shutter?.Type == MAS.AutomationService.Contracts.ShutterType.UpperHalf)) &&
                   ((this.CurrentStep == BayCheckStep.PositionUp && this.StepValueUp != 0) ||
                   (this.CurrentStep == BayCheckStep.PositionDown && this.StepValueDown != 0));
        }

        private bool CanMoveToBayPosition()
        {
            return this.CanBaseExecute() &&
                   (!this.HasShutter || this.SensorsService.ShutterSensors.Closed ||
                (this.SensorsService.ShutterSensors.MidWay && this.MachineService.Bay.Shutter?.Type == MAS.AutomationService.Contracts.ShutterType.UpperHalf));
        }

        private bool CanStop()
        {
            return this.IsMoving;
        }

        private async Task DisplacementCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                double step = 0;
                if (this.CurrentStep == BayCheckStep.PositionUp)
                {
                    step = this.StepValueUp;
                }
                else
                {
                    step = this.StepValueDown;
                }

                await this.machineElevatorWebService.MoveVerticalOfDistanceAsync(step);

                if (this.CurrentStep == BayCheckStep.PositionUp)
                {
                    if (this.DisplacementUp is null)
                    {
                        this.DisplacementUp = Convert.ToDecimal(step);
                    }
                    else
                    {
                        this.DisplacementUp += Convert.ToDecimal(step);
                    }

                    this.NewPositionUp = this.BayPositionActive.Height + (double)this.DisplacementUp.Value;
                }
                else
                {
                    if (this.DisplacementDown is null)
                    {
                        this.DisplacementDown = Convert.ToDecimal(step);
                    }
                    else
                    {
                        this.DisplacementDown += Convert.ToDecimal(step);
                    }

                    this.NewPositionDown = this.BayPositionActive.Height + (double)this.DisplacementDown.Value;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task GetLowerBound()
        {
            var procedureParameters = await this.verticalOriginProcedureWebService.GetParametersAsync();

            this.LowerBound = procedureParameters.LowerBound;
        }

        private void MachineService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    this.RaisePropertyChanged(nameof(this.ShutterLabel));
                }));
        }

        private async Task MoveToBayPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                // TODO : Il controllo del peso devo farlo? Così come l'alluggamento?
                await this.machineElevatorWebService.MoveToBayPositionAsync(
                    this.BayPositionActive.Id,
                    computeElongation: true,
                    performWeighting: true);

                if (this.CurrentStep == BayCheckStep.PositionUp)
                {
                    this.DisplacementUp = null;
                    this.StepValueUp = 0;
                }
                else
                {
                    this.DisplacementDown = null;
                    this.StepValueDown = 0;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MoveToShutterAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineShuttersWebService.MoveToAsync(
                    this.SensorsService.ShutterSensors.Open ?
                    ShutterPosition.Closed :
                    ShutterPosition.Opened);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case BayCheckStep.PositionUp:
                    if (e.Next)
                    {
                        if (this.Bay?.IsDouble ?? false)
                        {
                            this.CurrentStep = BayCheckStep.PositionDown;
                        }
                        else
                        {
                            this.CurrentStep = BayCheckStep.Confirm;
                        }
                    }

                    break;

                case BayCheckStep.PositionDown:
                    if (e.Next)
                    {
                        this.CurrentStep = BayCheckStep.Confirm;
                    }
                    else
                    {
                        this.CurrentStep = BayCheckStep.PositionUp;
                    }

                    break;

                case BayCheckStep.Confirm:
                    if (!e.Next)
                    {
                        if (this.Bay?.IsDouble ?? false)
                        {
                            this.CurrentStep = BayCheckStep.PositionDown;
                        }
                        else
                        {
                            this.CurrentStep = BayCheckStep.PositionUp;
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.MachineService.StopMovingByAllAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void SubscribeToEvents()
        {
            this.MachineService.PropertyChanged += this.MachineService_PropertyChanged;

            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepConfirm));
                           this.RaisePropertyChanged(nameof(this.HasStepPositionDown));
                           this.RaisePropertyChanged(nameof(this.HasStepPositionDownVisible));
                           this.RaisePropertyChanged(nameof(this.HasStepPositionUp));
                           this.RaisePropertyChanged(nameof(this.NumberStepConfirm));
                           this.RaisePropertyChanged(nameof(this.CurrentBayPosition));
                           this.RaisePropertyChanged(nameof(this.BayPositionActive));
                           this.RaisePropertyChanged(nameof(this.HasDisplacementUpValue));
                           this.RaisePropertyChanged(nameof(this.HasDisplacementDownValue));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case BayCheckStep.PositionUp:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case BayCheckStep.PositionDown:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case BayCheckStep.Confirm:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, true);

            this.RaisePropertyChanged(nameof(this.HasStepConfirm));
            this.RaisePropertyChanged(nameof(this.HasStepPositionDown));
            this.RaisePropertyChanged(nameof(this.HasStepPositionDownVisible));
            this.RaisePropertyChanged(nameof(this.HasStepPositionUp));
            this.RaisePropertyChanged(nameof(this.NumberStepConfirm));
            this.RaisePropertyChanged(nameof(this.CurrentBayPosition));
            this.RaisePropertyChanged(nameof(this.BayPositionActive));
            this.RaisePropertyChanged(nameof(this.HasDisplacementUpValue));
            this.RaisePropertyChanged(nameof(this.HasDisplacementDownValue));
        }

        #endregion
    }
}
