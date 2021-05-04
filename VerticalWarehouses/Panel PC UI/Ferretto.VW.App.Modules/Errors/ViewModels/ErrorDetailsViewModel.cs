using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
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

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private MachineError error;

        private string errorTime;

        private bool findZero;

        private bool isVisibleFindZero;

        private bool isVisibleGoTo;

        private SubscriptionToken machineModeChangedToken;

        private SubscriptionToken machinePowerChangedToken;

        private ICommand markAsResolvedAndFindZeroCommand;

        private ICommand markAsResolvedAndGoCommand;

        private ICommand markAsResolvedCommand;

        #endregion

        #region Constructors

        public ErrorDetailsViewModel(
            IMachineErrorsWebService machineErrorsWebService,
            IMachineElevatorWebService machineElevatorWebService)
            : base(Services.PresentationMode.Menu | Services.PresentationMode.Installer | Services.PresentationMode.Operator)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));

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

        public bool IsVisibleFindZero
        {
            get => this.isVisibleFindZero;
            set => this.SetProperty(ref this.isVisibleFindZero, value, this.RaiseCanExecuteChanged);
        }

        public bool IsVisibleGoTo
        {
            get => this.isVisibleGoTo;
            set => this.SetProperty(ref this.isVisibleGoTo, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MarkAsResolvedAndFindZeroCommand =>
          this.markAsResolvedAndFindZeroCommand
          ??
          (this.markAsResolvedAndFindZeroCommand = new DelegateCommand(
              async () => await this.MarkAsResolvedAndFindZeroAsync(),
              this.CanMarkAsResolved)
          .ObservesProperty(() => this.Error)
          .ObservesProperty(() => this.IsWaitingForResponse));

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

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.Error = null;

            if (!this.findZero)
            {
                this.machinePowerChangedToken?.Dispose();
                this.machinePowerChangedToken = null;

                this.machineModeChangedToken?.Dispose();
                this.machineModeChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.ShowPrevStepSinglePage(false, false);
            this.ShowNextStepSinglePage(false, false);
            this.ShowAbortStep(false, false);

            await this.RetrieveErrorAsync();

            await base.OnAppearedAsync();

            this.machinePowerChangedToken = this.EventAggregator
              .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
              .Subscribe(
                  this.OnMachinePowerChanged,
                  ThreadOption.UIThread,
                  false);

            this.machineModeChangedToken = this.EventAggregator
               .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
               .Subscribe(
                   this.OnMachineModeChanged,
                   ThreadOption.UIThread,
                   false);
        }

        private bool CanMarkAsResolved()
        {
            return
                this.Error != null
                &&
                !this.IsWaitingForResponse;
        }

        private async Task MarkAsResolvedAndFindZeroAsync()
        {
            if (this.Error is null)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineErrorsWebService.ResolveAsync(this.Error.Id);

                await this.MachineModeService.PowerOnAsync();

                this.findZero = true;

                this.Error = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
            {
                this.ShowNotification(ex);

                this.findZero = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
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
                this.IsVisibleFindZero = false;
                return;
            }

            if (this.error.Code == 39)
            {
                this.IsVisibleGoTo = true;
                this.IsVisibleFindZero = false;
            }
            else if (this.error.Code == 15)
            {
                this.IsVisibleGoTo = false;
                this.IsVisibleFindZero = true;
            }
            else
            {
                this.IsVisibleGoTo = false;
                this.IsVisibleFindZero = false;
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
                this.findZero = false;
            }
        }

        private void OnMachinePowerChanged(MachinePowerChangedEventArgs e)
        {
            if (e.MachinePowerState == MachinePowerState.Powered &&
                this.findZero)
            {
                this.findZero = false;
                this.machineElevatorWebService.FindLostZeroAsync();

                this.machinePowerChangedToken?.Dispose();
                this.machinePowerChangedToken = null;

                this.machineModeChangedToken?.Dispose();
                this.machineModeChangedToken = null;
            }
        }

        private async Task RetrieveErrorAsync()
        {
            try
            {
                // reset the command
                this.findZero = false;

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
