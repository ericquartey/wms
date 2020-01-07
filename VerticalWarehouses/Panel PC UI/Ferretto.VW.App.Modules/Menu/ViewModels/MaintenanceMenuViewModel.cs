using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    [Warning(WarningsArea.Maintenance)]
    internal sealed class MaintenanceMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly ISessionService sessionService;

        private int bayNumber;

        private MachineIdentity machineIdentity;

        private DelegateCommand menuCompactionCommand;

        private DelegateCommand menuMaintenanceCommand;

        private DelegateCommand menuUpdateCommand;

        #endregion

        #region Constructors

        public MaintenanceMenuViewModel(
            IBayManager bayManager,
            ISessionService sessionService)
            : base(PresentationMode.Menu)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Compaction,

            Maintenance,

            Update,
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            set => this.SetProperty(ref this.bayNumber, value, this.RaiseCanExecuteChanged);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public MachineIdentity MachineIdentity
        {
            get => this.machineIdentity;
            set => this.SetProperty(ref this.machineIdentity, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MenuCompactionCommand =>
            this.menuCompactionCommand
            ??
            (this.menuCompactionCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Compaction),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual));

        public ICommand MenuMaintenanceCommand =>
            this.menuMaintenanceCommand
            ??
            (this.menuMaintenanceCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Maintenance),
                this.CanExecuteCommand));

        public ICommand MenuUpdateCommand =>
            this.menuUpdateCommand
            ??
            (this.menuUpdateCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Update),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.GetBayNumber();

                this.RaiseCanExecuteChanged();

                await base.OnAppearedAsync();

                this.IsBackNavigationAllowed = true;
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

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.menuCompactionCommand?.RaiseCanExecuteChanged();
            this.menuMaintenanceCommand?.RaiseCanExecuteChanged();
            this.menuUpdateCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.MachineIdentity));
            this.RaisePropertyChanged(nameof(this.BayNumber));
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
                        this.bayNumber = (int)bay.Number;
                    }

                    if (this.Data is MachineIdentity machineIdentity)
                    {
                        this.MachineIdentity = machineIdentity;
                    }
                    else
                    {
                        this.MachineIdentity = this.sessionService.MachineIdentity;
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

                    case Menu.Update:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.UPDATE,
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
