using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ElevatorWeightCheckStep2ViewModel : BaseElevatorWeightCheckViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly INavigationService navigationService;

        private double? inputWeight;

        private bool isWorking;

        private int? loadingUnitId;

        private double? measuredWeight;

        private string noteText;

        private DelegateCommand startWeightCheckCommand;

        private DelegateCommand stopWeightCheckCommand;

        private SubscriptionToken subscriptionToken;

        private double testToRun;

        private double weightTolerance;

        #endregion

        #region Constructors

        public ElevatorWeightCheckStep2ViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            INavigationService navigationService)
            : base()
        {
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        #endregion

        #region Properties

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.TestToRun)],
            this[nameof(this.InputWeight)],
            this[nameof(this.WeightTolerance)]);

        public double? InputWeight
        {
            get => this.inputWeight;
            set
            {
                if (this.SetProperty(ref this.inputWeight, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWorking
        {
            get => this.isWorking;
            set => this.SetProperty(ref this.isWorking, value);
        }

        public double? MeasuredWeight
        {
            get => this.measuredWeight;
            set
            {
                if (this.SetProperty(ref this.measuredWeight, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string NoteText
        {
            get => this.noteText;
            set
            {
                if (this.SetProperty(ref this.noteText, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand StartWeightCheckCommand =>
            this.startWeightCheckCommand
            ??
            (this.startWeightCheckCommand = new DelegateCommand(
                async () => await this.StartWeightCheckAsync(),
                this.CanStartWeightCheck));

        public ICommand StopWeightCheckCommand =>
            this.stopWeightCheckCommand
            ??
            (this.stopWeightCheckCommand = new DelegateCommand(
                async () => await this.StopWeightCheckAsync(),
                this.CanStopWeightCheck));

        public double TestToRun
        {
            get => this.testToRun;
            set
            {
                if (this.SetProperty(ref this.testToRun, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double WeightTolerance
        {
            get => this.weightTolerance;
            set
            {
                if (this.SetProperty(ref this.weightTolerance, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.TestToRun):
                        if (this.TestToRun < 0)
                        {
                            return $"Test to Run must be positive";
                        }

                        break;

                    case nameof(this.InputWeight):
                        if (!this.InputWeight.HasValue)
                        {
                            return "Input Weight must be specified.";
                        }

                        if (this.InputWeight.Value <= 0)
                        {
                            return "Input Weight must be strictly positive.";
                        }

                        break;

                    case nameof(this.WeightTolerance):

                        if (this.WeightTolerance <= 0)
                        {
                            return "Weight Tolerance must be strictly positive.";
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;
            */
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            if (this.Data is int idLoadingUnit)
            {
                this.loadingUnitId = idLoadingUnit;
            }

            // TO DO must implement get parameters on corresponding service
            this.TestToRun = 5;
            this.WeightTolerance = 2;

            this.IsWorking = false;
            this.InputWeight = null;

            this.IsBackNavigationAllowed = true;

            this.subscriptionToken = this.subscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ElevatorWeightCheckMessageData>>()
                    .Subscribe(
                        this.ElevatorWeightCheckChanged,
                        ThreadOption.UIThread,
                        false);

            this.RaiseCanExecuteChanged();
        }

        public override void InitializeSteps()
        {
            this.ShowSteps();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.startWeightCheckCommand?.RaiseCanExecuteChanged();
            this.stopWeightCheckCommand?.RaiseCanExecuteChanged();
        }

        private bool CanStartWeightCheck()
        {
            return
                this.loadingUnitId.HasValue
                &&
                this.testToRun >= 0
                &&
                this.InputWeight > 0
                &&
                this.WeightTolerance > 0
                &&
                !this.isWorking
                &&
                string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanStopWeightCheck()
        {
            return this.isWorking;
        }

        private void ElevatorWeightCheckChanged(NotificationMessageUI<ElevatorWeightCheckMessageData> message)
        {
            if (message.IsErrored())
            {
                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped);
            }
            else if (message.IsNotRunning())
            {
                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureCompleted);
            }

            this.MeasuredWeight = message.Data?.Weight ?? this.MeasuredWeight;
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.Elevator.WeightCheck.STEP1);
            this.ShowNextStep(true, false);
            this.ShowAbortStep(true, true);
        }

        private async Task StartWeightCheckAsync()
        {
            try
            {
                this.IsWorking = true;

                this.NoteText = string.Empty;

                this.RaiseCanExecuteChanged();

                await this.machineElevatorWebService.WeightCheckAsync(this.loadingUnitId.Value, this.testToRun, this.inputWeight.Value);

                this.IsWorking = false;
            }
            catch (Exception ex)
            {
                this.IsWorking = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.RaiseCanExecuteChanged();
            }
        }

        private async Task StopWeightCheckAsync()
        {
            try
            {
                await this.machineElevatorWebService.StopWeightCheckAsync();

                this.IsWorking = false;
            }
            catch (Exception ex)
            {
                this.IsWorking = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }
}
