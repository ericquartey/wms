using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    internal sealed class InstallationMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private bool isWaitingForResponse;

        private DelegateCommand menuAccessoriesCommand;

        private DelegateCommand menuBaysCommand;

        private DelegateCommand menuCellsCommand;

        private DelegateCommand menuElevatorCommand;

        private DelegateCommand menuLoadingUnitsCommand;

        private DelegateCommand menuMovementsCommand;

        private DelegateCommand menuOldCommand;

        private DelegateCommand menuOtherCommand;

        private DelegateCommand viewStatusSensorsCommand;

        #endregion

        #region Constructors

        public InstallationMenuViewModel(IMachineSetupStatusWebService machineSetupStatusWebService)
            : base(PresentationMode.Menu)
        {
            this.machineSetupStatusWebService = machineSetupStatusWebService;
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Elevator,

            Accessories,

            Bays,

            Cells,

            LoadingUnits,

            Other,

            Old,
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MenuAccessoriesCommand =>
            this.menuAccessoriesCommand
            ??
            (this.menuAccessoriesCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Accessories),
                this.CanExecuteCommand));

        public ICommand MenuBaysCommand =>
            this.menuBaysCommand
            ??
            (this.menuBaysCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Bays),
                this.CanExecuteCommand));

        public ICommand MenuCellsCommand =>
            this.menuCellsCommand
            ??
            (this.menuCellsCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Cells),
                this.CanExecuteCommand));

        public ICommand MenuElevatorCommand =>
            this.menuElevatorCommand
            ??
            (this.menuElevatorCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Elevator),
                this.CanExecuteCommand));

        public ICommand MenuLoadingUnitsCommand =>
            this.menuLoadingUnitsCommand
            ??
            (this.menuLoadingUnitsCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.LoadingUnits),
                this.CanExecuteCommand));

        public ICommand MenuMovementsCommand =>
            this.menuMovementsCommand
            ??
            (this.menuMovementsCommand = new DelegateCommand(
                () => this.MovementsCommand(),
                this.CanExecuteMovementsCommand));

        public ICommand MenuOldCommand =>
            this.menuOldCommand
            ??
            (this.menuOldCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Old),
                this.CanExecuteCommand));

        public ICommand MenuOtherCommand =>
            this.menuOtherCommand
            ??
            (this.menuOtherCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Other),
                this.CanExecuteCommand));

        public ICommand ViewStatusSensorsCommand =>
            this.viewStatusSensorsCommand
            ??
            (this.viewStatusSensorsCommand = new DelegateCommand(
                () => this.StatusSensorsCommand(),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.IsWaitingForResponse = false;
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

        private bool CanExecuteCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanExecuteMovementsCommand()
        {
            return !this.IsWaitingForResponse
                && this.MachineModeService.MachinePower == MachinePowerState.Powered
                && this.HealthProbeService.HealthStatus == HealthStatus.Healthy;
        }

        private void MenuCommand(Menu menu)
        {
            this.ClearNotifications();

            this.Logger.Trace($"MenuCommand({menu})");

            this.IsWaitingForResponse = true;

            try
            {
                switch (menu)
                {
                    case Menu.Accessories:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.ACCESSORIES_MENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Bays:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.BAYS_MENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Cells:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.CELLS_MENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Elevator:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.ELEVATOR_MENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.LoadingUnits:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.LOADINGUNITS_MENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Old:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.INSTALLATORMENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Other:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),partial
                            Utils.Modules.Menu.Installation.OTHER_MENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    default:
                        Debugger.Break();
                        break;
                }
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

        private void RaiseCanExecuteChanged()
        {
            this.menuAccessoriesCommand?.RaiseCanExecuteChanged();
            this.menuBaysCommand?.RaiseCanExecuteChanged();
            this.menuCellsCommand?.RaiseCanExecuteChanged();
            this.menuElevatorCommand?.RaiseCanExecuteChanged();
            this.menuLoadingUnitsCommand?.RaiseCanExecuteChanged();
            this.menuOldCommand?.RaiseCanExecuteChanged();
            this.menuMovementsCommand?.RaiseCanExecuteChanged();
            this.menuOtherCommand?.RaiseCanExecuteChanged();
            this.viewStatusSensorsCommand?.RaiseCanExecuteChanged();
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
