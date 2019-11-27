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
        }

        #endregion

        #region Properties

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

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        private bool CanExecuteCommand()
        {
            return !this.IsWaitingForResponse;
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

        private void RaiseCanExecuteChanged()
        {
        }

        #endregion
    }
}
