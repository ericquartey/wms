using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Menu.ViewModels
{
    internal sealed class InstallationMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private bool isWaitingForResponse;

        private DelegateCommand menuAccessoriesCommand;

        private DelegateCommand menuBaysCommand;

        private DelegateCommand menuCellsCommand;

        private DelegateCommand menuElevatorCommand;

        private DelegateCommand menuLoadingUnitsCommand;

        private DelegateCommand menuMovementsCommand;

        private DelegateCommand menuOldCommand;

        #endregion

        #region Constructors

        public InstallationMenuViewModel()
            : base(PresentationMode.Menu)
        {
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
                () => this.MovementsCommandAsync(),
                this.CanExecuteMovementsCommand));

        public ICommand MenuOldCommand =>
                    this.menuOldCommand
            ??
            (this.menuOldCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Old),
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

        private async Task MovementsCommandAsync()
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

        private void RaiseCanExecuteChanged()
        {
            this.menuAccessoriesCommand?.RaiseCanExecuteChanged();
            this.menuBaysCommand?.RaiseCanExecuteChanged();
            this.menuCellsCommand?.RaiseCanExecuteChanged();
            this.menuElevatorCommand?.RaiseCanExecuteChanged();
            this.menuLoadingUnitsCommand?.RaiseCanExecuteChanged();
            this.menuOldCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
