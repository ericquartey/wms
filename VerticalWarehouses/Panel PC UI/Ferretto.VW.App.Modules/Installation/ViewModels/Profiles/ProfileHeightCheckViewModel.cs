using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ProfileHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineProfileProcedureService profileProcedureService;

        private int activeRaysQuantity;

        private decimal currentHeight;

        private decimal gateCorrection;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private string noteText;

        private SubscriptionToken receivedActionUpdateToken;

        private int speed;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private decimal systemError;

        private decimal tolerance;

        #endregion

        #region Constructors

        public ProfileHeightCheckViewModel(
            IEventAggregator eventAggregator,
            IMachineProfileProcedureService profileProcedureService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (profileProcedureService is null)
            {
                throw new ArgumentNullException(nameof(profileProcedureService));
            }

            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.eventAggregator = eventAggregator;
            this.profileProcedureService = profileProcedureService;
            this.bayManager = bayManager;
        }

        #endregion

        #region Properties

        public int ActiveRaysQuantity { get => this.activeRaysQuantity; set => this.SetProperty(ref this.activeRaysQuantity, value); }

        public decimal CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public string Error => string.Join(
                System.Environment.NewLine,
                this[nameof(this.Speed)]);

        public decimal GateCorrection { get => this.gateCorrection; set => this.SetProperty(ref this.gateCorrection, value); }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            private set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ShowNotification(string.Empty, Services.Models.NotificationSeverity.Clear);
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }

        public int Speed { get => this.speed; set => this.SetProperty(ref this.speed, value); }

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

        public decimal SystemError { get => this.systemError; set => this.SetProperty(ref this.systemError, value); }

        public decimal Tolerance { get => this.tolerance; set => this.SetProperty(ref this.tolerance, value); }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.Speed):
                        if (this.Speed < 0)
                        {
                            return "Speed must be strictly positive.";
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

            if (this.receivedActionUpdateToken != null)
            {
                this.eventAggregator
                  .GetEvent<NotificationEventUI<PositioningMessageData>>()
                  .Unsubscribe(this.receivedActionUpdateToken);

                this.receivedActionUpdateToken = null;
            }
        }

        public async Task GetParameterValuesAsync()
        {
            try
            {
                // var procedureParameters = await this.beltBurnishingService.GetParametersAsync();
                // this.InputUpperBound = procedureParameters.UpperBound;
                // this.InputLowerBound = procedureParameters.LowerBound;
                // this.InputRequiredCycles = procedureParameters.RequiredCycles;
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            await this.GetParameterValuesAsync();

            this.receivedActionUpdateToken = this.eventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                    async message => await this.UpdateCompletion(message),
                    ThreadOption.UIThread,
                    false);
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

        private void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                var currentBay = this.bayManager.Bay.Number;
                await this.profileProcedureService.RunAsync(currentBay);
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

                await this.profileProcedureService.StopAsync();
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

        private async Task UpdateCompletion(NotificationMessageUI<PositioningMessageData> message)
        {
            await Task.Delay(1);

            if (message is null)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsExecutingProcedure = true;
                    break;

                case MessageStatus.OperationEnd:
                case MessageStatus.OperationStop:
                    this.IsExecutingProcedure = false;
                    break;

                case MessageStatus.OperationError:
                    this.IsExecutingProcedure = false;
                    break;
            }
        }

        #endregion
    }
}
