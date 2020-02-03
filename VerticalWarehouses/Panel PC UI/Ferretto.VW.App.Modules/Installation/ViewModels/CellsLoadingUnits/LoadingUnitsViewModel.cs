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
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private DelegateCommand freeDrawerCommand;

        private DelegateCommand immediateDrawerCallCommand;

        private DelegateCommand insertDrawerCommand;

        private bool isExecutingProcedure;

        private IEnumerable<LoadingUnit> loadingUnits;

        private DelegateCommand removeDrawerCommand;

        private DelegateCommand saveDrawerCommand;

        private LoadingUnit selectedLU;

        private DelegateCommand stopMovingCommand;

        #endregion

        #region Constructors

        public LoadingUnitsViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IHealthProbeService healthProbeService)
            : base(PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand FreeDrawerCommand =>
            this.freeDrawerCommand
            ??
            (this.freeDrawerCommand = new DelegateCommand(
                () => this.ImmediateDrawerCall(),
                () => !this.IsExecutingProcedure && !this.IsWaitingForResponse && false));

        public ICommand ImmediateDrawerCallCommand =>
            this.immediateDrawerCallCommand
            ??
            (this.immediateDrawerCallCommand = new DelegateCommand(
                () => this.ImmediateDrawerCall(),
                () => !this.IsExecutingProcedure && !this.IsWaitingForResponse && false));

        public ICommand InsertDrawerCommand =>
            this.insertDrawerCommand
            ??
            (this.insertDrawerCommand = new DelegateCommand(
                () => this.ImmediateDrawerCall(),
                () => !this.IsExecutingProcedure && !this.IsWaitingForResponse && false));

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            private set => this.SetProperty(ref this.isExecutingProcedure, value, this.RaiseCanExecuteChanged);
        }

        public IEnumerable<LoadingUnit> LoadingUnits
        {
            get => this.loadingUnits;
            private set => this.SetProperty(ref this.loadingUnits, value, this.RaiseCanExecuteChanged);
        }

        public ICommand RemoveDrawerCommand =>
            this.removeDrawerCommand
            ??
            (this.removeDrawerCommand = new DelegateCommand(
                () => this.ImmediateDrawerCall(),
                () => !this.IsExecutingProcedure && !this.IsWaitingForResponse && false));

        public ICommand SaveDrawerCommand =>
            this.saveDrawerCommand
            ??
            (this.saveDrawerCommand = new DelegateCommand(
                () => this.ImmediateDrawerCall(),
                () => !this.IsExecutingProcedure && !this.IsWaitingForResponse && false));

        public LoadingUnit SelectedLU
        {
            get => this.selectedLU;
            set => this.SetProperty(ref this.selectedLU, value);
        }

        public ICommand StopMovingCommand =>
                                                                                    this.stopMovingCommand
            ??
            (this.stopMovingCommand = new DelegateCommand(
                () => this.ImmediateDrawerCall(),
                () => !this.IsExecutingProcedure && !this.IsWaitingForResponse && false));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsWaitingForResponse = true;

            await this.RetrieveLoadingUnitsAsync().ContinueWith((e) => { this.IsWaitingForResponse = false; });

            this.IsBackNavigationAllowed = true;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.ClearNotifications();
        }

        private void ImmediateDrawerCall()
        {
        }

        private async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                if (this.healthProbeService.HealthMasStatus == HealthStatus.Healthy || this.healthProbeService.HealthMasStatus == HealthStatus.Degraded)
                {
                    this.LoadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();
                    this.SelectedLU = this.LoadingUnits?.ToList()[0];
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
