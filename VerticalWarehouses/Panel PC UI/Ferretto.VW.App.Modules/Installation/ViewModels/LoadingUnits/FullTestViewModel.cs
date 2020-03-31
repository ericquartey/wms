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

        private int? totalCycles;

        private DelegateCommand startCommand;

        private DelegateCommand resetSession;

        private DelegateCommand resetTotal;

        private DelegateCommand stopCommand;

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

        public int? TotalCycles
        {
            get => this.totalCycles;
            set => this.SetProperty(ref this.totalCycles, value, () => this.startCommand?.RaiseCanExecuteChanged());
        }

        public ICommand StartCommand =>
                    this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartAsync(),
                this.CanStart));

        public ICommand ResetSession =>
                    this.resetSession
            ??
            (this.resetSession = new DelegateCommand(
                async () => await this.ResetSessionAsync(),
                this.CanResetSession));

        public ICommand ResetTotal =>
                    this.resetTotal
            ??
            (this.resetTotal = new DelegateCommand(
                async () => await this.ResetTotalAsync(),
                this.CanResetTotal));

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

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            await this.SensorsService.RefreshAsync(true);

            if (this.RequiredCycles == null || this.PerformedCyclesThisSession == null  || this.totalCycles == null)
            {
                //var procedureParameters = await this.machineFullTestWebService.GetParametersAsync();
                this.RequiredCycles = 200;
                this.PerformedCyclesThisSession = 0;
                this.TotalCycles = 0;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
        }

        private bool CanStart()
        {
            return !this.IsMoving &&
                   this.LoadingUnits.Any() &&
                   this.RequiredCycles.HasValue;
        }

        private bool CanResetSession()
        {
            return this.requiredCycles != 0;
        }

        private bool CanResetTotal()
        {
            return this.TotalCycles != 0;
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

        private async Task ResetSessionAsync()
        {
            this.RequiredCycles = 0;
            this.PerformedCyclesThisSession = 0;
            this.CyclesPercent = 0;
        }

        private async Task ResetTotalAsync()
        {
            this.TotalCycles = 0;
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineFullTestWebService.StopAsync();
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
