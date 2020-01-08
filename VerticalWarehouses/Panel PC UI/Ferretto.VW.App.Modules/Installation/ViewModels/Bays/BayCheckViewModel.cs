using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
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
    internal sealed class BayCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private Bay bay;

        private BayCheckStep currentStep;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public BayCheckViewModel(
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.CurrentStep = BayCheckStep.PositionUp;
        }

        #endregion

        #region Properties

        public Bay Bay => this.bay;

        public BayCheckStep CurrentStep
        {
            get => this.currentStep;
            set => this.SetProperty(ref this.currentStep, value, this.RaiseCanExecuteChanged);
        }

        public string Error { get; }

        public bool HasStepConfirm => this.currentStep is BayCheckStep.Confirm;

        public bool HasStepPositionDown => this.currentStep is BayCheckStep.PositionDown;

        public bool HasStepPositionDownVisible => this.bay?.IsDouble ?? false;

        public bool HasStepPositionUp => this.currentStep is BayCheckStep.PositionUp;

        public int NumberStepConfirm => (this.bay?.IsDouble ?? false) ? 3 : 2;

        public ICommand StopCommand =>
                    this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
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

        public override void InitializeSteps()
        {
            this.ShowSteps();
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                await base.OnAppearedAsync();

                this.IsWaitingForResponse = true;

                this.stepChangedToken = this.stepChangedToken
                    ?? this.EventAggregator
                        .GetEvent<StepChangedPubSubEvent>()
                        .Subscribe(
                            (m) => this.OnStepChanged(m),
                            ThreadOption.UIThread,
                            false);

                await this.InitializeDataAsync();

                this.IsBackNavigationAllowed = false;

                this.RaiseCanExecuteChanged();
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case BayCheckStep.PositionUp:
                    if (e.Next)
                    {
                        if (this.bay?.IsDouble ?? false)
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
                        if (this.bay?.IsDouble ?? false)
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

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.HasStepConfirm));
            this.RaisePropertyChanged(nameof(this.HasStepPositionDown));
            this.RaisePropertyChanged(nameof(this.HasStepPositionDownVisible));
            this.RaisePropertyChanged(nameof(this.HasStepPositionUp));
            this.RaisePropertyChanged(nameof(this.NumberStepConfirm));

            this.UpdateStatusButtonFooter();
        }

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                this.bay = await this.bayManager.GetBayAsync();
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void ShowSteps()
        {
            this.ShowPrevStepSinglePage(true, false);
            this.ShowNextStepSinglePage(true, true);
            this.ShowAbortStep(true, true);
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.MachineService.StopMovingByAllAsync();
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

        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case BayCheckStep.PositionUp:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case BayCheckStep.PositionDown:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case BayCheckStep.Confirm:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);
        }

        #endregion
    }
}
