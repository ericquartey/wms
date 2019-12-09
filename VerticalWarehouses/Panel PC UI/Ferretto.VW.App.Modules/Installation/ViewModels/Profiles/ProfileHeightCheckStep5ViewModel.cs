using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ProfileHeightCheckStep5ViewModel : BaseProfileHeightCheckViewModel
    {
        #region Fields

        private int activeRaysQuantity;

        private decimal currentHeight;

        private decimal gateCorrection;

        private string noteText;

        //private int speed;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private decimal tolerance;

        #endregion

        #region Constructors

        public ProfileHeightCheckStep5ViewModel(
            IEventAggregator eventAggregator,
            IMachineProfileProcedureWebService profileProcedureService,
            IMachineModeService machineModeService,
            IBayManager bayManager)
            : base(eventAggregator, profileProcedureService, machineModeService, bayManager)
        {
        }

        #endregion

        #region Properties

        public int ActiveRaysQuantity { get => this.activeRaysQuantity; set => this.SetProperty(ref this.activeRaysQuantity, value); }

        public decimal CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        //public override string Error => string.Join(
        //        System.Environment.NewLine,
        //        this[nameof(this.Speed)]);

        public decimal GateCorrection { get => this.gateCorrection; set => this.SetProperty(ref this.gateCorrection, value); }

        public string NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }

        //public int Speed { get => this.speed; set => this.SetProperty(ref this.speed, value); }

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartAsync(),
                this.CanExecuteStartCommand));

        public ICommand StopCommand =>
                                    this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanExecuteStopCommand));

        public decimal Tolerance { get => this.tolerance; set => this.SetProperty(ref this.tolerance, value); }

        #endregion

        //public override string this[string columnName]
        //{
        //    get
        //    {
        //        switch (columnName)
        //        {
        //            case nameof(this.Speed):
        //                if (this.Speed < 0)
        //                {
        //                    return "Speed must be strictly positive.";
        //                }
        //                break;
        //        }
        //        return base[columnName];
        //    }
        //}

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.currentStep = ProfileHeightCheckStep.TaraturaCatena;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
        }

        protected override void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP4);
            this.ShowNextStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP6);
            this.ShowAbortStep(true, true);
        }

        private bool CanExecuteStartCommand()
        {
            return !this.IsExecutingProcedure
                && !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanExecuteStopCommand()
        {
            return this.IsExecutingProcedure
                && !this.IsWaitingForResponse;
        }

        private HorizontalMovementDirection GetDirection()
        {
            return this.Bay.Side == WarehouseSide.Front
                ? HorizontalMovementDirection.Forwards
                : HorizontalMovementDirection.Backwards;
        }

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                this.IsBackNavigationAllowed = false;

                var direction = this.GetDirection();

                await this.ProfileProcedureService.CalibrationAsync(direction);
            }
            catch (Exception ex)
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

                this.IsBackNavigationAllowed = true;

                await this.ProfileProcedureService.StopAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsExecutingProcedure = false;
            }
        }

        #endregion
    }
}
