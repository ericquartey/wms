using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class AccessoriesMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private BayAccessories accessories;

        private DelegateCommand<string> openSettingsCommand;

        #endregion

        #region Constructors

        public AccessoriesMenuViewModel(IMachineAccessoriesWebService accessoriesWebService)
        {
            this.accessoriesWebService = accessoriesWebService;
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand OpenSettingsCommand =>
            this.openSettingsCommand
            ??
            (this.openSettingsCommand = new DelegateCommand<string>(
                this.OpenSettings,
                this.CanOpenSettings));

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            try
            {
                this.accessories = await this.accessoriesWebService.GetAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.openSettingsCommand?.RaiseCanExecuteChanged();
        }

        private bool CanOpenSettings(string viewModel)
        {
            // return this.CanExecuteCommand();

            return
                (this.MachineModeService.MachinePower == MachinePowerState.Powered
                ||
                this.MachineModeService.MachinePower == MachinePowerState.Unpowered
                ||
                this.MachineModeService.MachinePower == MachinePowerState.NotSpecified)
                &&
                !(this.MachineModeService.MachineMode == MachineMode.Automatic)
                &&
                (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy
                ||
                this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded)
                &&
                this.accessories != null;
        }

        private void OpenSettings(string viewModel)
        {
            this.NavigationService.Appear(nameof(Utils.Modules.Installation), viewModel, this.accessories);
        }

        #endregion
    }
}
