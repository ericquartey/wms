using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class LoadingUnitsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private int currentIndex;

        private DelegateCommand downDataGridButtonCommand;

        private bool isWaitingForResponse;

        private IEnumerable<LoadingUnit> loadingUnits;

        private LoadingUnit selectedLU;

        private DelegateCommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public LoadingUnitsViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IHealthProbeService healthProbeService
            )
            : base(Services.PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
        }

        #endregion

        #region Properties

        public ICommand DownDataGridButtonCommand =>
            this.downDataGridButtonCommand
            ??
            (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(false), this.IsWaitingForResponse));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public IEnumerable<LoadingUnit> LoadingUnits
        {
            get => this.loadingUnits;
            private set => this.SetProperty(ref this.loadingUnits, value, this.RaiseCanExecuteChanged);
        }

        public LoadingUnit SelectedLU
        {
            get => this.selectedLU;
            set => this.SetProperty(ref this.selectedLU, value);
        }

        public ICommand UpDataGridButtonCommand =>
            this.upDataGridButtonCommand
            ??
            (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(true), this.IsWaitingForResponse));

        #endregion

        #region Methods

        public void ChangeSelectedItemAsync(bool isUp)
        {
            if (this.LoadingUnits == null)
            {
                return;
            }

            if (this.LoadingUnits.Count() > 0)
            {
                this.currentIndex = isUp ? --this.currentIndex : ++this.currentIndex;
                if (this.currentIndex < 0 || this.currentIndex >= this.LoadingUnits.Count())
                {
                    this.currentIndex = (this.currentIndex < 0) ? 0 : this.LoadingUnits.Count() - 1;
                }

                this.SelectedLU = this.LoadingUnits?.ToList()[this.currentIndex];
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsWaitingForResponse = true;

            await this.RetrieveLoadingUnitsAsync().ContinueWith((e) => { this.IsWaitingForResponse = false; });

            this.IsBackNavigationAllowed = true;
        }

        private void RaiseCanExecuteChanged()
        {
            this.ClearNotifications();
        }

        private async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                if (this.healthProbeService.HealthStatus == HealthStatus.Healthy)
                {
                    this.LoadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();
                    this.SelectedLU = this.LoadingUnits?.ToList()[this.currentIndex];
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
