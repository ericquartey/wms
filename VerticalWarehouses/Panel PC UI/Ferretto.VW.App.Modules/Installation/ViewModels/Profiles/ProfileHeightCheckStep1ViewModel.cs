using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using LoadingUnitLocation = Ferretto.VW.MAS.AutomationService.Contracts.LoadingUnitLocation;
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ProfileHeightCheckStep1ViewModel : BaseProfileHeightCheckViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private bool canInputLoadingUnitId;

        private int? inputLoadingUnitId;

        private IEnumerable<LoadingUnit> loadingUnits;

        private bool luPresentInBay;

        private SubscriptionToken moveLoadingUnitToken;

        private LoadingUnit selectedLoadingUnit;

        private SubscriptionToken sensorsChangedToken;

        private DelegateCommand startCommand;

        #endregion

        #region Constructors

        public ProfileHeightCheckStep1ViewModel(
            IMachineSensorsWebService machineSensorsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IEventAggregator eventAggregator,
            IMachineProfileProcedureWebService profileProcedureService,
            IMachineModeService machineModeService,
            IBayManager bayManager)
            : base(eventAggregator, profileProcedureService, machineModeService, bayManager)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
        }

        #endregion

        #region Properties

        public bool CanInputLoadingUnitId
        {
            get => this.canInputLoadingUnitId;
            private set => this.SetProperty(ref this.canInputLoadingUnitId, value);
        }

        public int? InputLoadingUnitId
        {
            get => this.inputLoadingUnitId;
            set
            {
                if (this.SetProperty(ref this.inputLoadingUnitId, value)
                    &&
                    this.LoadingUnits != null)
                {
                    this.SelectedLoadingUnit = value == null
                        ? null
                        : this.LoadingUnits.SingleOrDefault(c => c.Id == value);

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoadingUnitIdValid
        {
            get
            {
                if (!this.inputLoadingUnitId.HasValue)
                {
                    return false;
                }

                return this.loadingUnits.Any(l => l.Id == this.inputLoadingUnitId.Value);
            }
        }

        public IEnumerable<LoadingUnit> LoadingUnits { get => this.loadingUnits; set => this.loadingUnits = value; }

        public bool LUPresentInBay
        {
            get => this.luPresentInBay;
            private set => this.SetProperty(ref this.luPresentInBay, value);
        }

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            private set
            {
                if (this.SetProperty(ref this.selectedLoadingUnit, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand StartCommand =>
               this.startCommand
               ??
               (this.startCommand = new DelegateCommand(
                   async () => await this.StartAsync(),
                   this.CanExecuteStartCommand));

        private bool IsCanExecuteStepCommand =>
                   !this.IsExecutingProcedure
                && !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error)
                && this.LUPresentInBay;

        #endregion

        #region Methods

        public bool CanExecuteStartCommand()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.LUPresentInBay
                &&
                string.IsNullOrWhiteSpace(this.Error);
        }

        public override void Disappear()
        {
            base.Disappear();

            if (this.sensorsChangedToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.sensorsChangedToken);

                this.sensorsChangedToken = null;
            }

            if (this.moveLoadingUnitToken != null)
            {
                this.EventAggregator
                 .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                 .Unsubscribe(this.moveLoadingUnitToken);

                this.moveLoadingUnitToken = null;
            }
        }

        public LoadingUnitLocation GetLoadingUnitSource()
        {
            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                return LoadingUnitLocation.InternalBay1Up;
            }

            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                return LoadingUnitLocation.InternalBay2Up;
            }

            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                return LoadingUnitLocation.InternalBay3Up;
            }

            return LoadingUnitLocation.NoLocation;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.currentStep = ProfileHeightCheckStep.Initialize;

            try
            {
                this.SubscribeToEvents();

                var sensorsStates = await this.machineSensorsWebService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());

                await this.RetrieveLoadingUnitsAsync();

                // Voglio sapere qual'è il numero di cassetto in missione
                this.InputLoadingUnitId = this.Bay.Positions.Last().LoadingUnit?.Id;

                this.RaiseCanExecuteChanged();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.loadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();
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

        protected void Ended()
        {
            this.RestoreStates();

            this.IsExecutingProcedure = false;

            this.ShowNotification(
                VW.App.Resources.InstallationApp.ProcedureCompleted,
                Services.Models.NotificationSeverity.Success);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand?.RaiseCanExecuteChanged();

            this.CanInputLoadingUnitId = !this.IsExecutingProcedure && !this.IsWaitingForResponse;

            if ((this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne && this.sensors.LUPresentInBay1) ||
                (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo && this.sensors.LUPresentInBay2) ||
                (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree && this.sensors.LUPresentInBay3))
            {
                this.LUPresentInBay = true;
            }
            else
            {
                this.LUPresentInBay = false;
            }

            this.ShowNextStep(true, this.IsCanExecuteStepCommand, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP2);
        }

        protected override void ShowSteps()
        {
            this.ShowPrevStep(false, false);
            this.ShowNextStep(true, this.IsCanExecuteStepCommand, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP2);
            this.ShowAbortStep(true, true);
        }

        protected async Task StartAsync()
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    this.ShowNotification("Id cassetto inserito non valido", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var destination = this.GetLoadingUnitSource();

                if (destination == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(destination, this.InputLoadingUnitId.Value);

                this.ShowNotification(
                    "Operazione in corso...",
                    Services.Models.NotificationSeverity.Info);
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

        private void OnMoveLoadingUnitChanged(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    //this.CurrentMissionId = (message.Data as MoveLoadingUnitMessageData).MissionId;

                    break;

                case MessageStatus.OperationWaitResume:
                    this.RaiseCanExecuteChanged();

                    break;

                case MessageStatus.OperationEnd:
                    {
                        if (!this.IsExecutingProcedure)
                        {
                            break;
                        }

                        this.Ended();

                        break;
                    }

                case MessageStatus.OperationStop:
                case MessageStatus.OperationFaultStop:
                case MessageStatus.OperationRunningStop:
                    {
                        this.ShowNotification(
                            VW.App.Resources.InstallationApp.ProcedureWasStopped,
                            Services.Models.NotificationSeverity.Warning);

                        break;
                    }

                case MessageStatus.OperationError:
                    this.IsExecutingProcedure = false;

                    this.ShowNotification(
                        VW.App.Resources.InstallationApp.Error,
                        Services.Models.NotificationSeverity.Error);

                    break;
            }
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
            this.RaiseCanExecuteChanged();
        }

        private void SubscribeToEvents()
        {
            this.sensorsChangedToken = this.sensorsChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);

            this.moveLoadingUnitToken = this.moveLoadingUnitToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        this.OnMoveLoadingUnitChanged,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
