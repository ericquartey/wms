using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    internal sealed class FullTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineFullTestWebService machineFullTestWebService;

        private bool isExecutingProcedure;

        private SubscriptionToken loadUnitsChangedToken;

        private int? requiredCycles;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public FullTestViewModel(IMachineFullTestWebService machineFullTestWebService)
            : base(PresentationMode.Installer)
        {
            this.machineFullTestWebService = machineFullTestWebService ?? throw new ArgumentNullException(nameof(machineFullTestWebService));
        }

        #endregion

        #region Properties

        public string Error { get; }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set => this.SetProperty(ref this.isExecutingProcedure, value);
        }

        public IEnumerable<LoadingUnit> LoadingUnits => this.MachineService.Loadunits;

        public int? RequiredCycles
        {
            get => this.requiredCycles;
            set => this.SetProperty(ref this.requiredCycles, value, () => this.startCommand?.RaiseCanExecuteChanged());
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

        private bool CanStop()
        {
            return this.IsMoving;
        }

        private async Task StartAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                var lst = this.LoadingUnits.Where(w => w.Status == LoadingUnitStatus.InLocation).Select(s => s.Id).Take(3).ToList();
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
        }

        #endregion
    }
}
