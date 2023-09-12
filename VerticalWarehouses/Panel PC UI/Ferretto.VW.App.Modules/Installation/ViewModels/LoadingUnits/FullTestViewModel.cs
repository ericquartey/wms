using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class FullTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineFullTestWebService machineFullTestWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private DelegateCommand addAllUnitCommand;

        private DelegateCommand addUnitCommand;

        private SubscriptionToken cycleMessageReceivedToken;

        private double? cyclesPercent;

        private bool isExecutingProcedure;

        private ObservableCollection<LoadingUnit> loadingUnits;

        private SubscriptionToken loadUnitsChangedToken;

        private int? performedCyclesThisSession;

        private bool randomCells;

        private DelegateCommand removeAllUnitCommand;

        private DelegateCommand removeUnitCommand;

        private int? requiredCycles;

        private DelegateCommand resetSessionCommand;

        private DelegateCommand resetTotalCommand;

        private LoadingUnit selectedLU;

        private LoadingUnit selectedTU;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private DelegateCommand stopTestCommand;

        private ObservableCollection<LoadingUnit> testUnits;

        private int? totalCycles;

        #endregion

        #region Constructors

        public FullTestViewModel(
            IMachineFullTestWebService machineFullTestWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IEventAggregator eventAggregator)
            : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineFullTestWebService = machineFullTestWebService ?? throw new ArgumentNullException(nameof(machineFullTestWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));

            this.TestUnits = new ObservableCollection<LoadingUnit>();

            this.LoadingUnits = new ObservableCollection<LoadingUnit>();
        }

        #endregion

        #region Properties

        public ICommand AddAllUnitCommand =>
            this.addAllUnitCommand
            ??
            (this.addAllUnitCommand = new DelegateCommand(
                async () => await this.AddAllUnitAsync(),
                this.CanAddAllUnit));

        public ICommand AddUnitCommand =>
                    this.addUnitCommand
            ??
            (this.addUnitCommand = new DelegateCommand(
                async () => await this.AddUnitAsync(),
                this.CanAddUnit));

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public string Error { get; }

        public bool HasShutter => this.MachineService.HasShutter;

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set => this.SetProperty(ref this.isExecutingProcedure, value);
        }

        public ObservableCollection<LoadingUnit> LoadingUnits
        {
            get => this.loadingUnits;
            set
            {
                if (this.SetProperty(ref this.loadingUnits, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? PerformedCyclesThisSession
        {
            get => this.performedCyclesThisSession;
            set => this.SetProperty(ref this.performedCyclesThisSession, value);
        }

        public bool RandomCells
        {
            get => this.randomCells;
            set => this.SetProperty(ref this.randomCells, value);
        }

        public ICommand RemoveAllUnitCommand =>
            this.removeAllUnitCommand
            ??
            (this.removeAllUnitCommand = new DelegateCommand(
                async () => await this.RemoveAllUnitAsync(),
                this.CanRemoveAllUnit));

        public ICommand RemoveUnitCommand =>
                    this.removeUnitCommand
            ??
            (this.removeUnitCommand = new DelegateCommand(
                async () => await this.RemoveUnitAsync(),
                this.CanRemoveUnit));

        public int? RequiredCycles
        {
            get => this.requiredCycles;
            set => this.SetProperty(ref this.requiredCycles, value, () => this.startCommand?.RaiseCanExecuteChanged());
        }

        public ICommand ResetSessionCommand =>
            this.resetSessionCommand
            ??
            (this.resetSessionCommand = new DelegateCommand(
                this.ResetSession,
                this.CanResetSession));

        public ICommand ResetTotalCommand =>
            this.resetTotalCommand
            ??
            (this.resetTotalCommand = new DelegateCommand(
                this.ResetTotal,
                this.CanResetTotal));

        public LoadingUnit SelectedLU
        {
            get => this.selectedLU;
            set => this.SetProperty(ref this.selectedLU, value, this.RaiseCanExecuteChanged);
        }

        public LoadingUnit SelectedTU
        {
            get => this.selectedTU;
            set => this.SetProperty(ref this.selectedTU, value, this.RaiseCanExecuteChanged);
        }

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

        public ObservableCollection<LoadingUnit> TestUnits
        {
            get => this.testUnits;
            set
            {
                if (this.SetProperty(ref this.testUnits, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? TotalCycles
        {
            get => this.totalCycles;
            set => this.SetProperty(ref this.totalCycles, value, () => this.startCommand?.RaiseCanExecuteChanged());
        }

        #endregion

        #region Indexers

        public string this[string columnName] => null;

        #endregion

        #region Methods

        public ObservableCollection<T> Convert<T>(IEnumerable<T> original)
        {
            return new ObservableCollection<T>(original);
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            var iTestUnits = await this.machineLoadingUnitsWebService.GetAllTestUnitsAsync();

            this.TestUnits = this.Convert(iTestUnits);

            var iLoadingUnits = await this.machineLoadingUnitsWebService.GetAllNotTestUnitsAsync();

            this.LoadingUnits = this.Convert(iLoadingUnits);

            await base.OnAppearedAsync();

            if (!this.IsAdmin)
            {
                await this.AddAllUnitAsync();
            }
        }

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            await this.SensorsService.RefreshAsync(true);

            this.IsExecutingProcedure = this.MachineService.MachineStatus.IsMoving ||
                this.MachineService.MachineMode == MachineMode.Test ||
                this.MachineService.MachineMode == MachineMode.Test2 ||
                this.MachineService.MachineMode == MachineMode.Test3;

            if (this.RequiredCycles == null || this.PerformedCyclesThisSession == null || this.totalCycles == null)
            {
                var procedureParameters = await this.machineFullTestWebService.GetParametersAsync();
                this.RequiredCycles = procedureParameters.RequiredCycles;
                if (this.RequiredCycles is null || this.RequiredCycles == 0)
                {
                    this.RequiredCycles = 1;
                }
                this.PerformedCyclesThisSession = 0;
                this.TotalCycles = procedureParameters.PerformedCycles;
            }
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            this.IsExecutingProcedure = this.MachineService.MachineStatus.IsMoving ||
                this.MachineService.MachineMode == MachineMode.Test ||
                this.MachineService.MachineMode == MachineMode.Test2 ||
                this.MachineService.MachineMode == MachineMode.Test3;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.stopTestCommand?.RaiseCanExecuteChanged();
            this.resetSessionCommand?.RaiseCanExecuteChanged();
            this.resetTotalCommand?.RaiseCanExecuteChanged();
            this.addUnitCommand?.RaiseCanExecuteChanged();
            this.removeUnitCommand?.RaiseCanExecuteChanged();
            this.addAllUnitCommand?.RaiseCanExecuteChanged();
            this.removeAllUnitCommand?.RaiseCanExecuteChanged();
        }

        private async Task AddAllUnitAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                foreach (var unit in this.LoadingUnits)
                {
                    await this.machineLoadingUnitsWebService.AddTestUnitAsync(unit);
                }

                this.TestUnits = this.Convert(this.MachineService.Loadunits);

                this.LoadingUnits.Clear();

                this.RaiseCanExecuteChanged();

                this.RaisePropertyChanged(nameof(this.TestUnits));
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

        private async Task AddUnitAsync()
        {
            try
            {
                await this.machineLoadingUnitsWebService.AddTestUnitAsync(this.SelectedLU);

                this.TestUnits.Add(this.SelectedLU);

                this.LoadingUnits.Remove(this.selectedLU);

                this.RaiseCanExecuteChanged();

                this.RaisePropertyChanged(nameof(this.TestUnits));
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

        private bool CanAddAllUnit()
        {
            return this.LoadingUnits.Count > 0 && !this.IsMoving && (this.TestUnits != this.LoadingUnits) && !this.IsWaitingForResponse;
        }

        private bool CanAddUnit()
        {
            return this.SelectedLU != null && !this.TestUnits.Contains(this.SelectedLU) && !this.IsWaitingForResponse && !this.IsMoving && this.IsAdmin;
        }

        private bool CanRemoveAllUnit()
        {
            return this.TestUnits.Count > 0 && !this.IsMoving && !this.IsWaitingForResponse && this.IsAdmin;
        }

        private bool CanRemoveUnit()
        {
            return this.SelectedTU != null && this.TestUnits.Contains(this.SelectedTU) && !this.IsMoving && this.IsAdmin && !this.IsWaitingForResponse && this.IsAdmin;
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
                   this.TestUnits.Any() &&
                   this.RequiredCycles.HasValue &&
                   this.RequiredCycles > 0 &&
                   !this.IsWaitingForResponse;
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
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);
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
                this.ShowNotification(Resources.Localized.Get("InstallationApp.CompletedTest"), Services.Models.NotificationSeverity.Success);
                this.IsExecutingProcedure = false;
            }
        }

        private async Task RemoveAllUnitAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                foreach (var unit in this.LoadingUnits)
                {
                    await this.machineLoadingUnitsWebService.RemoveTestUnitAsync(unit);
                }

                foreach (var unit in this.TestUnits)
                {
                    await this.machineLoadingUnitsWebService.RemoveTestUnitAsync(unit);
                }

                this.LoadingUnits = this.Convert(this.MachineService.Loadunits);

                this.TestUnits.Clear();

                this.RaiseCanExecuteChanged();

                this.RaisePropertyChanged(nameof(this.TestUnits));
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

        private async Task RemoveUnitAsync()
        {
            try
            {
                await this.machineLoadingUnitsWebService.RemoveTestUnitAsync(this.SelectedTU);

                this.LoadingUnits.Add(this.SelectedTU);

                this.TestUnits.Remove(this.SelectedTU);

                this.RaiseCanExecuteChanged();

                this.RaisePropertyChanged(nameof(this.TestUnits));
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

        private void ResetSession()
        {
            this.PerformedCyclesThisSession = 0;
            this.CyclesPercent = 0;
        }

        private void ResetTotal()
        {
            this.TotalCycles = 0;
        }

        private async Task StartAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                var lst = this.TestUnits.Where(w => w.Status == LoadingUnitStatus.InLocation).Select(s => s.Id).ToList();
                await this.machineFullTestWebService.StartAsync(lst, this.RequiredCycles.Value, this.randomCells);

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
                await this.MachineService.StopMovingByAllAsync();

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
