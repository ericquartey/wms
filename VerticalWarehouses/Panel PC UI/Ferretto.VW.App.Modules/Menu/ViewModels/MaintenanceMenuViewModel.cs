using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    internal sealed class MaintenanceMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private bool isWaitingForResponse;

        private DelegateCommand menuBackupCommand;

        private DelegateCommand menuCompactionCommand;

        private DelegateCommand menuComunicationWMSCommand;

        private DelegateCommand menuMaintenanceCommand;

        private DelegateCommand menuParametersCommand;

        private DelegateCommand menuRestoreCommand;

        private DelegateCommand menuUpdateCommand;

        private DelegateCommand menuUsersCommand;

        #endregion

        #region Constructors

        public MaintenanceMenuViewModel()
            : base(PresentationMode.Menu)
        {
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Compaction,

            Maintenance,

            Backup,

            Restore,

            Parameters,

            Update,

            Users,

            ComunicationWms,
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MenuBackupCommand =>
            this.menuBackupCommand
            ??
            (this.menuBackupCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Backup),
                this.CanExecuteCommand));

        public ICommand MenuCompactionCommand =>
            this.menuCompactionCommand
            ??
            (this.menuCompactionCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Compaction),
                this.CanExecuteCommand));

        public ICommand MenuComunicationWmsCommand =>
            this.menuComunicationWMSCommand
            ??
            (this.menuComunicationWMSCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.ComunicationWms),
                this.CanExecuteCommand));

        public ICommand MenuMaintenanceCommand =>
                    this.menuMaintenanceCommand
            ??
            (this.menuMaintenanceCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Maintenance),
                this.CanExecuteCommand));

        public ICommand MenuParametersCommand =>
            this.menuParametersCommand
            ??
            (this.menuParametersCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Parameters),
                this.CanExecuteCommand));

        public ICommand MenuRestoreCommand =>
            this.menuRestoreCommand
            ??
            (this.menuRestoreCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Restore),
                this.CanExecuteCommand));

        public ICommand MenuUpdateCommand =>
            this.menuUpdateCommand
            ??
            (this.menuUpdateCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Update),
                this.CanExecuteCommand));

        public ICommand MenuUsersCommand =>
            this.menuUsersCommand
            ??
            (this.menuUsersCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Users),
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

        private void MenuCommand(Menu menu)
        {
            this.ClearNotifications();

            this.Logger.Trace($"MenuCommand({menu})");

            this.IsWaitingForResponse = true;

            try
            {
                switch (menu)
                {
                    case Menu.Compaction:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.Others.DrawerCompacting.MAIN,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Maintenance:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.Others.Maintenance.MAIN,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Parameters:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.Parameters.PARAMETERS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Restore:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.Parameters.PARAMETERSIMPORTSTEP1,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Backup:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.Parameters.PARAMETERSEXPORT,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Update:
                        //this.NavigationService.Appear(
                        //    nameof(Utils.Modules.Installation),
                        //    Utils.Modules.Installation.Parameters.PARAMETERSEXPORT,
                        //    data: null,
                        //    trackCurrentView: true);
                        break;

                    case Menu.Users:
                        //this.NavigationService.Appear(
                        //    nameof(Utils.Modules.Installation),
                        //    Utils.Modules.Installation.Parameters.PARAMETERSEXPORT,
                        //    data: null,
                        //    trackCurrentView: true);
                        break;
                    case Menu.ComunicationWms:
                        //this.NavigationService.Appear(
                        //    nameof(Utils.Modules.Installation),
                        //    Utils.Modules.Installation.Parameters.PARAMETERSEXPORT,
                        //    data: null,
                        //    trackCurrentView: true);
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
