using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Menu)]
    internal sealed class MainMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly ISessionService sessionService;

        private MachineIdentity machineIdentity;

        private DelegateCommand menuAboutCommand;

        private DelegateCommand menuInstalationCommand;

        private DelegateCommand menuMaintenanceCommand;

        private DelegateCommand menuOperationCommand;

        #endregion

        #region Constructors

        public MainMenuViewModel(
            IBayManager bayManager,
            ISessionService sessionService,
            IOperatorNavigationService operatorNavigationService)
            : base(PresentationMode.Menu)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(operatorNavigationService));
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Operation,

            Maintenance,

            InstallationOld,

            Installation,

            About,
        }

        #endregion

        #region Properties

        public int BayNumber => (int)this.MachineService?.BayNumber;

        public override EnableMask EnableMask => EnableMask.Any;

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
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode != MachineMode.Test));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = false;

            if (this.Data is MachineIdentity machineIdentity)
            {
                this.MachineIdentity = machineIdentity;
            }
            else
            {
                this.MachineIdentity = this.sessionService.MachineIdentity;
            }

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.menuAboutCommand?.RaiseCanExecuteChanged();
            this.menuInstalationCommand?.RaiseCanExecuteChanged();
            this.menuMaintenanceCommand?.RaiseCanExecuteChanged();
            this.menuOperationCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.MachineIdentity));
            this.RaisePropertyChanged(nameof(this.BayNumber));
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        private void MenuCommand(Menu menu)
        {
            this.Logger.Trace($"MenuCommand({menu})");

            this.IsWaitingForResponse = true;

            try
            {
                switch (menu)
                {
                    case Menu.Operation:
                        this.operatorNavigationService.NavigateToOperatorMenu();
                        break;

                    case Menu.Maintenance:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.MAINTENANCE_MENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.Installation:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Menu.INSTALLATION_MENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case Menu.About:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.About.GENERAL,
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

        #endregion
    }
}
