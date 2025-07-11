﻿using System;
using System.Diagnostics;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class OtherMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private DelegateCommand errorInverterParametersCommand;

        private DelegateCommand logoutSettingsCommand;

        private DelegateCommand menuComunicationWMSCommand;

        private DelegateCommand menuDatabaseBackupCommand;

        private DelegateCommand menuDateTimeCommand;

        private DelegateCommand menuParameterInverterCommand;

        private DelegateCommand menuParametersCommand;

        private DelegateCommand menuUsersCommand;

        private DelegateCommand selectOperationOnBayCommand;

        #endregion

        #region Constructors

        public OtherMenuViewModel()
            : base()
        {
        }

        #endregion

        #region Enums

        private enum MenuOther
        {
            Users,

            Parameters,

            ParameterInverter,

            ComunicationWms,

            DateTime,

            DatabaseBackup,

            ErrorInverterParameters,

            LogoutSettings,

            OperationOnBay,
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ErrorInverterParametersCommand =>
            this.errorInverterParametersCommand
            ??
            (this.errorInverterParametersCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.ErrorInverterParameters),
                this.CanExecute));

        public ICommand LogoutSettingsCommand =>
            this.logoutSettingsCommand
            ??
            (this.logoutSettingsCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.LogoutSettings),
                this.CanExecute));

        public ICommand MenuComunicationWmsCommand =>
                            this.menuComunicationWMSCommand
            ??
            (this.menuComunicationWMSCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.ComunicationWms),
                this.CanExecute));

        public ICommand MenuDatabaseBackupCommand =>
            this.menuDatabaseBackupCommand
            ??
            (this.menuDatabaseBackupCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.DatabaseBackup),
                this.CanExecute));

        public ICommand MenuDateTimeCommand =>
                    this.menuDateTimeCommand
            ??
            (this.menuDateTimeCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.DateTime),
                this.CanExecute));

        public ICommand MenuParameterInverterCommand =>
            this.menuParameterInverterCommand
            ??
            (this.menuParameterInverterCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.ParameterInverter),
                this.CanExecute));

        public ICommand MenuParametersCommand =>
            this.menuParametersCommand
            ??
            (this.menuParametersCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.Parameters),
                this.CanExecute));

        public ICommand MenuUsersCommand =>
            this.menuUsersCommand
            ??
            (this.menuUsersCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.Users),
                this.CanExecute));

        public ICommand SelectOperationOnBayCommand =>
                                    this.selectOperationOnBayCommand
            ??
            (this.menuUsersCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.OperationOnBay),
                this.CanExecute));

        #endregion

        #region Methods

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.menuComunicationWMSCommand?.RaiseCanExecuteChanged();
            this.menuUsersCommand?.RaiseCanExecuteChanged();
            this.menuParametersCommand?.RaiseCanExecuteChanged();
            this.menuDateTimeCommand?.RaiseCanExecuteChanged();
            this.menuParameterInverterCommand?.RaiseCanExecuteChanged();
            this.menuDatabaseBackupCommand?.RaiseCanExecuteChanged();
            this.errorInverterParametersCommand?.RaiseCanExecuteChanged();
            this.logoutSettingsCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecute()
        {
            switch (this.MachineService.BayNumber)
            {
                case BayNumber.BayOne:
                    return !(this.MachineModeService.MachineMode == MachineMode.Test || this.MachineModeService.MachineMode == MachineMode.Automatic) &&
                      (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded);

                case BayNumber.BayTwo:
                    return !(this.MachineModeService.MachineMode == MachineMode.Test2 || this.MachineModeService.MachineMode == MachineMode.Automatic) &&
                      (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded);

                case BayNumber.BayThree:
                    return !(this.MachineModeService.MachineMode == MachineMode.Test3 || this.MachineModeService.MachineMode == MachineMode.Automatic) &&
                      (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded);

                default:
                    return !(this.MachineModeService.MachineMode == MachineMode.Test || this.MachineModeService.MachineMode == MachineMode.Automatic) &&
                      (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded);
            }
        }

        private void MenuCommandOther(MenuOther menu)
        {
            this.ClearNotifications();

            this.Logger.Trace($"MenuCommand({menu})");

            this.IsWaitingForResponse = true;

            try
            {
                switch (menu)
                {
                    case MenuOther.Parameters:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.Parameters.PARAMETERS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOther.Users:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.USERS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOther.DateTime:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.DATETIME,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOther.ComunicationWms:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.WMSSETTINGS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOther.ParameterInverter:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.Inverters.PARAMETERINVERTER,
                            data: "reset",
                            trackCurrentView: true);
                        break;

                    case MenuOther.ErrorInverterParameters:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.Inverters.ERRORPARAMETERINVERTER,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOther.DatabaseBackup:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.DATABASEBACKUP,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOther.LogoutSettings:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.LOGOUTSETTINGS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOther.OperationOnBay:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.Others.OPERATIONONBAY,
                            null,
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
