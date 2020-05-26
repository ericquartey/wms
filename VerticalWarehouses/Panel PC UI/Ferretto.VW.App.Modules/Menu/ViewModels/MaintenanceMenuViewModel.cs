﻿using System;
using System.Collections.Generic;
using System.Configuration;
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

        private readonly IMachineService machineService;

        private readonly ISessionService sessionService;

        private MachineIdentity machineIdentity;

        private DelegateCommand menuCompactionCommand;

        private DelegateCommand menuMaintenanceCommand;

        private DelegateCommand menuUpdateCommand;

        #endregion

        #region Constructors

        public MaintenanceMenuViewModel(
            IBayManager bayManager,
            ISessionService sessionService, IMachineService machineService)
            : base(PresentationMode.Operator)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
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
                      (this.MachineModeService.MachineMode == MachineMode.Manual || this.MachineModeService.MachineMode == MachineMode.Compact) &&
                      this.MachineModeService.MachinePower == MachinePowerState.Powered &&
                     (this.machineService.IsTuningCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
               )
            );

        public ICommand MenuMaintenanceCommand =>
            this.menuMaintenanceCommand
            ??
            (this.menuMaintenanceCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Maintenance),
                () => this.CanExecuteCommand()));

        public ICommand MenuUpdateCommand =>
            this.menuUpdateCommand
            ??
            (this.menuUpdateCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.Update),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
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

            this.menuCompactionCommand?.RaiseCanExecuteChanged();
            this.menuMaintenanceCommand?.RaiseCanExecuteChanged();
            this.menuUpdateCommand?.RaiseCanExecuteChanged();
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
                            Utils.Modules.Installation.Update.STEP1,
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
