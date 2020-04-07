using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    internal sealed class FullTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineFullTestWebService machineFullTestWebService;

        private SubscriptionToken cycleMessageReceivedToken;

        private double? cyclesPercent;

        private bool isExecutingProcedure;

        private SubscriptionToken loadUnitsChangedToken;

        private int? performedCyclesThisSession;

        private int? requiredCycles;

        private DelegateCommand resetSessionCommand;

        private DelegateCommand resetTotalCommand;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private DelegateCommand stopTestCommand;

        private int? totalCycles;

        #endregion

        #region Constructors

        public FullTestViewModel(IMachineFullTestWebService machineFullTestWebService,
            IEventAggregator eventAggregator)
            : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineFullTestWebService = machineFullTestWebService ?? throw new ArgumentNullException(nameof(machineFullTestWebService));
        }

        #endregion

        #region Properties

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public string Error { get; }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set => this.SetProperty(ref this.isExecutingProcedure, value);
        }

        public IEnumerable<LoadingUnit> LoadingUnits => this.MachineService.Loadunits;

        public int? PerformedCyclesThisSession
        {
            get => this.performedCyclesThisSession;
            set => this.SetProperty(ref this.performedCyclesThisSession, value);
        }

        public int? RequiredCycles
        {
            get => this.requiredCycles;
            set => this.SetProperty(ref this.requiredCycles, value, () => this.startCommand?.RaiseCanExecuteChanged());
        }

        public ICommand ResetSessionCommand =>
            this.resetSessionCommand
            ??
            (this.resetSessionCommand = new DelegateCommand(
                async () => await this.ResetSessionAsync(),
                this.CanResetSession));

        public ICommand ResetTotalCommand =>
            this.resetTotalCommand
            ??
            (this.resetTotalCommand = new DelegateCommand(
                async () => await this.ResetTotalAsync(),
                this.CanResetTotal));

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartAsync(),
                this.CanStart));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        public ICommand StopTestCommand =>
            this.stopTestCommand
            ??
            (this.stopTestCommand = new DelegateCommand(
                async () => await this.StopTestAsync(),
                this.CanStop));

        public int? TotalCycles
        {
            get => this.totalCycles;
            set => this.SetProperty(ref this.totalCycles, value, () => this.startCommand?.RaiseCanExecuteChanged());
        }

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

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            await this.SensorsService.RefreshAsync(true);

            this.IsExecutingProcedure = this.MachineService.MachineStatus.IsMoving || this.MachineService.MachineMode == MachineMode.Test;

            if (this.RequiredCycles == null || this.PerformedCyclesThisSession == null || this.totalCycles == null)
            {
                var procedureParameters = await this.machineFullTestWebService.GetParametersAsync();
                this.RequiredCycles = procedureParameters.RequiredCycles;
                this.PerformedCyclesThisSession = 0;
                this.TotalCycles = procedureParameters.PerformedCycles;
            }
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            this.IsExecutingProcedure = this.MachineService.MachineStatus.IsMoving || this.MachineService.MachineMode == MachineMode.Test;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.stopTestCommand?.RaiseCanExecuteChanged();
            this.resetSessionCommand?.RaiseCanExecuteChanged();
            this.resetTotalCommand?.RaiseCanExecuteChanged();
        }

        private bool CanResetSession()
        {
            return this.requiredCycles != 0 &&
                   !this.IsMoving;
        }

        private bool CanResetTotal()
        {
            return this.TotalCycles != 0 &&
                   !this.IsMoving;
        }

        private bool CanStart()
        {
            return !this.IsMoving &&
                   this.LoadingUnits.Any() &&
                   this.RequiredCycles.HasValue;
        }

        private bool CanStop()
        {
            return this.IsMoving;
        }

        private void OnTestMessageReceived(NotificationMessageUI<MoveTestMessageData> message)
        {
            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored())
            {
                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped, Services.Models.NotificationSeverity.Warning);
                this.IsExecutingProcedure = false;
            }

            this.PerformedCyclesThisSession = message.Data.ExecutedCycles;
            this.RequiredCycles = message.Data.RequiredCycles;

            if (this.RequiredCycles.HasValue)
            {
                this.CyclesPercent = ((double)(this.PerformedCyclesThisSession ?? 0) / (double)this.RequiredCycles) * 100.0;
            }
            else
            {
                this.CyclesPercent = null;
            }

            if (message.Status == MessageStatus.OperationEnd &&
                message.Data?.ExecutedCycles == message.Data.RequiredCycles)
            {
                this.ShowNotification(VW.App.Resources.InstallationApp.CompletedTest, Services.Models.NotificationSeverity.Success);
                //this.isCompleted = true;
                this.IsExecutingProcedure = false;
            }
        }

        private async Task ResetSessionAsync()
        {
            this.PerformedCyclesThisSession = 0;
            this.CyclesPercent = 0;
        }

        private async Task ResetTotalAsync()
        {
            this.TotalCycles = 0;
        }

        private async Task StartAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                var lst = this.LoadingUnits.Where(w => w.Status == LoadingUnitStatus.InLocation).Select(s => s.Id).ToList();
                await this.machineFullTestWebService.StartAsync(lst, this.RequiredCycles.Value);

                this.IsExecutingProcedure = true;
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

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.MachineService.StopMovingByAllAsync();

                this.IsExecutingProcedure = false;
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

        private async Task StopTestAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineFullTestWebService.StopAsync();

                this.IsExecutingProcedure = false;
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
            this.loadUnitsChangedToken = this.loadUnitsChangedToken
                ?? this.EventAggregator
                    .GetEvent<LoadUnitsChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.RaisePropertyChanged(nameof(this.LoadingUnits)),
                        ThreadOption.UIThread,
                        false);

            this.cycleMessageReceivedToken = this.cycleMessageReceivedToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<MoveTestMessageData>>()
                    .Subscribe(
                        this.OnTestMessageReceived,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
