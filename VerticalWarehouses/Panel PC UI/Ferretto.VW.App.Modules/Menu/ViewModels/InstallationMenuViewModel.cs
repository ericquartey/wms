using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class InstallationMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private DelegateCommand menuMovementsCommand;

        private DelegateCommand viewStatusSensorsCommand;

        #endregion

        #region Constructors

        public InstallationMenuViewModel(IMachineSetupStatusWebService machineSetupStatusWebService)
            : base()
        {
            this.machineSetupStatusWebService = machineSetupStatusWebService;
        }

        #endregion

        #region Properties

        public ICommand MenuMovementsCommand =>
            this.menuMovementsCommand
            ??
            (this.menuMovementsCommand = new DelegateCommand(
                () => this.MovementsCommand(),
                this.CanExecuteMovementsCommand));

        public ICommand ViewStatusSensorsCommand =>
            this.viewStatusSensorsCommand
            ??
            (this.viewStatusSensorsCommand = new DelegateCommand(
                () => this.StatusSensorsCommand(),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        internal override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.menuMovementsCommand?.RaiseCanExecuteChanged();
            this.viewStatusSensorsCommand?.RaiseCanExecuteChanged();
        }

        protected override async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            await base.OnHealthStatusChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        private bool CanExecuteMovementsCommand()
        {
            return !this.IsWaitingForResponse
                && this.MachineModeService.MachinePower == MachinePowerState.Powered
                && this.HealthProbeService.HealthStatus == HealthStatus.Healthy;
        }

        private void MovementsCommand()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.MOVEMENTS,
                    data: null,
                    trackCurrentView: true);
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

        private void ParametersCommand()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.Parameters.PARAMETERS,
                    data: null,
                    trackCurrentView: true);
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

        private void StatusSensorsCommand()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.Sensors.SECURITY,
                    data: null,
                    trackCurrentView: true);
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

        #endregion
    }
}
