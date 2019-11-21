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
    internal sealed class MainMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private int bayNumber;

        private bool isWaitingForResponse;

        private MachineIdentity machineIdentity;

        private DelegateCommand menuAboutCommand;

        private DelegateCommand menuInstalationCommand;

        private DelegateCommand menuMaintenanceCommand;

        private DelegateCommand menuOperationCommand;

        #endregion

        #region Constructors

        public MainMenuViewModel(
            IBayManager bayManager)
            : base(PresentationMode.Menu)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Operation,

            Maintenance,

            Installation,

            About,
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            set => this.SetProperty(ref this.bayNumber, value, this.RaiseCanExecuteChanged);
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public MachineIdentity MachineIdentity
        {
            get => this.machineIdentity;
            set => this.SetProperty(ref this.machineIdentity, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MenuAboutCommand =>
            this.menuAboutCommand
            ??
            (this.menuAboutCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.About),
                this.CanExecuteCommand));

        public ICommand MenuInstalationCommand =>
            this.menuInstalationCommand
            ??
            (this.menuInstalationCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Installation),
                this.CanExecuteCommand));

        public ICommand MenuMaintenanceCommand =>
            this.menuMaintenanceCommand
            ??
            (this.menuMaintenanceCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Maintenance),
                this.CanExecuteCommand));

        public ICommand MenuOperationCommand =>
            this.menuOperationCommand
            ??
            (this.menuOperationCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Operation),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        public override EnableMask EnableMask => EnableMask.Any;

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await base.OnAppearedAsync();

                this.IsBackNavigationAllowed = false;

                await this.GetBayNumber();

                if (this.Data is MachineIdentity machineIdentity)
                {
                    this.MachineIdentity = machineIdentity;
                }

                this.RaiseCanExecuteChanged();
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

        private bool CanExecuteCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private async Task GetBayNumber()
        {
            try
            {
                if (this.IsConnectedByMAS)
                {
                    var bay = await this.bayManager.GetBayAsync();
                    if (!(bay is null))
                    {
                        this.BayNumber = (int)bay.Number;
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
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
                    case Menu.Operation:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.OPERATORMENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Maintenance:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.MAINTENANCEMENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Installation:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.INSTALLATORMENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.About:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.ABOUTMENU,
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
            this.menuAboutCommand?.RaiseCanExecuteChanged();
            this.menuInstalationCommand?.RaiseCanExecuteChanged();
            this.menuMaintenanceCommand?.RaiseCanExecuteChanged();
            this.menuMaintenanceCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.MachineIdentity));
        }

        #endregion
    }
}
