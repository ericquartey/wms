using System;
using System.Configuration;
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

        private readonly IMachineService machineService;

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly ISessionService sessionService;

        private MachineIdentity machineIdentity;

        private DelegateCommand menuAboutCommand;

        private DelegateCommand menuInstalationCommand;

        private DelegateCommand menuMaintenanceCommand;

        private DelegateCommand menuMovementsCommand;

        private DelegateCommand menuOperationCommand;

        #endregion

        #region Constructors

        public MainMenuViewModel(
            IBayManager bayManager,
            ISessionService sessionService,
            IOperatorNavigationService operatorNavigationService,
            IMachineService machineService)
            : base(PresentationMode.Menu)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(operatorNavigationService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
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

            Movements,
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
                () => this.CanExecuteCommand(Menu.About)));

        public ICommand MenuInstalationCommand =>
            this.menuInstalationCommand
            ??
            (this.menuInstalationCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Installation),
                () => this.CanExecuteCommand(Menu.Installation)));

        public ICommand MenuMaintenanceCommand =>
            this.menuMaintenanceCommand
            ??
            (this.menuMaintenanceCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Maintenance),
                () => this.CanExecuteCommand(Menu.Maintenance)));

        public ICommand MenuMovementsCommand =>
            this.menuMovementsCommand
            ??
            (this.menuMovementsCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Movements),
                () => this.CanExecuteCommand(Menu.Movements)));

        public ICommand MenuOperationCommand =>
            this.menuOperationCommand
            ??
            (this.menuOperationCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Operation),
                () => this.CanExecuteCommand(Menu.Operation)));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = false;

            this.MachineIdentity = this.sessionService.MachineIdentity;

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.menuAboutCommand?.RaiseCanExecuteChanged();
            this.menuInstalationCommand?.RaiseCanExecuteChanged();
            this.menuMaintenanceCommand?.RaiseCanExecuteChanged();
            this.menuOperationCommand?.RaiseCanExecuteChanged();
            this.menuMovementsCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.MachineIdentity));
            this.RaisePropertyChanged(nameof(this.BayNumber));
        }

        private bool CanExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.About:
                case Menu.Maintenance:
                    return true;

                case Menu.Operation:
                    return this.MachineModeService.MachineMode != MachineMode.Test &&
                this.MachineModeService.MachineMode != MachineMode.Test2 &&
                this.MachineModeService.MachineMode != MachineMode.Test3 &&
                (this.machineService.IsTuningCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus());

                case Menu.Installation:
                    if (this.MachineModeService.MachineMode == MachineMode.Automatic ||
                        this.MachineModeService.MachineMode == MachineMode.SwitchingToAutomatic ||
                        this.MachineModeService.MachineMode == MachineMode.NotSpecified ||
                        this.MachineModeService.MachinePower == MachinePowerState.NotSpecified)
                    {
                        return true;
                    }

                    switch (this.machineService?.BayNumber)
                    {
                        case MAS.AutomationService.Contracts.BayNumber.BayOne:
                            return this.MachineModeService.MachineMode == MachineMode.Manual
                                || this.MachineModeService.MachineMode == MachineMode.Test
                                || this.MachineModeService.MachineMode == MachineMode.Compact
                                || this.MachineModeService.MachineMode == MachineMode.FirstTest
                                || this.MachineModeService.MachineMode == MachineMode.FullTest
                                || this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToCompact
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToFirstTest
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToFullTest
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToLoadUnitOperations
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToManual;

                        case MAS.AutomationService.Contracts.BayNumber.BayTwo:
                            return this.MachineModeService.MachineMode == MachineMode.Manual2
                                || this.MachineModeService.MachineMode == MachineMode.Test2
                                || this.MachineModeService.MachineMode == MachineMode.Compact2
                                || this.MachineModeService.MachineMode == MachineMode.FirstTest2
                                || this.MachineModeService.MachineMode == MachineMode.FullTest2
                                || this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations2
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToCompact2
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToFirstTest2
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToFullTest2
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToLoadUnitOperations2
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToManual2;

                        case MAS.AutomationService.Contracts.BayNumber.BayThree:
                            return this.MachineModeService.MachineMode == MachineMode.Manual3
                                || this.MachineModeService.MachineMode == MachineMode.Test3
                                || this.MachineModeService.MachineMode == MachineMode.Compact3
                                || this.MachineModeService.MachineMode == MachineMode.FirstTest3
                                || this.MachineModeService.MachineMode == MachineMode.FullTest3
                                || this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations3
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToCompact3
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToFirstTest3
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToFullTest3
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToLoadUnitOperations3
                                || this.MachineModeService.MachineMode == MachineMode.SwitchingToManual3;

                        default:
                            return true;
                    }

                default:
                    return true;
            }
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
                        this.operatorNavigationService.NavigateToOperatorMenuAsync();
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

                    case Menu.Movements:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.MOVEMENTS,
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
