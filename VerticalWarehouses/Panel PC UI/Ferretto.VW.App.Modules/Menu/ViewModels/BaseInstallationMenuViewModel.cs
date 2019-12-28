﻿using System;
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
    internal abstract class BaseInstallationMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private bool isAccessoriesActive;

        private bool isBaysActive;

        private bool isCellsActive;

        private bool isElevatorActive;

        private bool isGeneralActive;

        private bool isLoadingUnitsActive;

        private bool isOtherActive;

        private DelegateCommand menuAccessoriesCommand;

        private DelegateCommand menuBaysCommand;

        private DelegateCommand menuCellsCommand;

        private DelegateCommand menuElevatorCommand;

        private DelegateCommand menuInstallatorCommand;

        private DelegateCommand menuLoadingUnitsCommand;

        private DelegateCommand menuMovementsCommand;

        private DelegateCommand menuOtherCommand;

        private DelegateCommand viewStatusSensorsCommand;

        #endregion

        #region Constructors

        public BaseInstallationMenuViewModel()
            : base(PresentationMode.Menu)
        {
        }

        #endregion

        #region Enums

        private enum Menu
        {
            General,

            Elevator,

            Accessories,

            Bays,

            Cells,

            LoadingUnits,

            Other,
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsAccessoriesActive
        {
            get => this.isAccessoriesActive;
            set => this.SetProperty(ref this.isAccessoriesActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBaysActive
        {
            get => this.isBaysActive;
            set => this.SetProperty(ref this.isBaysActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCellsActive
        {
            get => this.isCellsActive;
            set => this.SetProperty(ref this.isCellsActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsElevatorActive
        {
            get => this.isElevatorActive;
            set => this.SetProperty(ref this.isElevatorActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsGeneralActive
        {
            get => this.isGeneralActive;
            set => this.SetProperty(ref this.isGeneralActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsLoadingUnitsActive
        {
            get => this.isLoadingUnitsActive;
            set => this.SetProperty(ref this.isLoadingUnitsActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsOtherActive
        {
            get => this.isOtherActive;
            set => this.SetProperty(ref this.isOtherActive, value, this.RaiseCanExecuteChanged);
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

        public ICommand MenuInstallatorCommand =>
            this.menuInstallatorCommand
            ??
            (this.menuInstallatorCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.General),
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

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            if (this.IsVisible)
            {
                this.IsAccessoriesActive = false;
                this.IsBaysActive = false;
                this.IsCellsActive = false;
                this.IsElevatorActive = false;
                this.IsLoadingUnitsActive = false;
                this.IsOtherActive = false;
                this.IsGeneralActive = false;

                switch ((Menu)(this.Data ?? Menu.General))
                {
                    case Menu.Accessories:
                        this.IsAccessoriesActive = true;
                        break;

                    case Menu.Bays:
                        this.IsBaysActive = true;
                        break;

                    case Menu.Cells:
                        this.IsCellsActive = true;
                        break;

                    case Menu.Elevator:
                        this.IsElevatorActive = true;
                        break;

                    case Menu.General:
                        this.IsGeneralActive = true;
                        break;

                    case Menu.LoadingUnits:
                        this.IsLoadingUnitsActive = true;
                        break;

                    case Menu.Other:
                        this.IsOtherActive = true;
                        break;
                }
            }

            this.IsBackNavigationAllowed = true;

            this.IsWaitingForResponse = false;
        }

        internal virtual bool CanExecuteCommand()
        {
            return !this.IsWaitingForResponse;
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

        protected override void RaiseCanExecuteChanged()
        {
            this.menuAccessoriesCommand?.RaiseCanExecuteChanged();
            this.menuBaysCommand?.RaiseCanExecuteChanged();
            this.menuCellsCommand?.RaiseCanExecuteChanged();
            this.menuElevatorCommand?.RaiseCanExecuteChanged();
            this.menuLoadingUnitsCommand?.RaiseCanExecuteChanged();
            this.menuOtherCommand?.RaiseCanExecuteChanged();
            this.menuMovementsCommand?.RaiseCanExecuteChanged();
            this.viewStatusSensorsCommand?.RaiseCanExecuteChanged();
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
                            data: menu,
                            trackCurrentView: false);
                        break;

                    case Menu.Bays:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.BAYS_MENU,
                            data: menu,
                            trackCurrentView: false);
                        break;

                    case Menu.Cells:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.CELLS_MENU,
                            data: menu,
                            trackCurrentView: false);
                        break;

                    case Menu.Elevator:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.ELEVATOR_MENU,
                            data: menu,
                            trackCurrentView: false);
                        break;

                    case Menu.LoadingUnits:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.LOADINGUNITS_MENU,
                            data: menu,
                            trackCurrentView: false);
                        break;

                    case Menu.Other:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.Installation.OTHER_MENU,
                            data: menu,
                            trackCurrentView: false);
                        break;

                    case Menu.General:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.INSTALLATION_MENU,
                            data: menu,
                            trackCurrentView: false);
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
