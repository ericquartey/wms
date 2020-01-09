﻿using System;
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
    [Warning(WarningsArea.Installation)]
    internal sealed class OtherMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private DelegateCommand menuComunicationWMSCommand;

        private DelegateCommand menuDateTimeCommand;

        private DelegateCommand menuParameterInverterCommand;

        private DelegateCommand menuParametersCommand;

        private DelegateCommand menuUsersCommand;

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
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand MenuComunicationWmsCommand =>
            this.menuComunicationWMSCommand
            ??
            (this.menuComunicationWMSCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.ComunicationWms),
                this.CanExecuteCommand));

        public ICommand MenuDateTimeCommand =>
            this.menuDateTimeCommand
            ??
            (this.menuDateTimeCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.DateTime),
                this.CanExecuteCommand));

        public ICommand MenuParameterInverterCommand =>
            this.menuParameterInverterCommand
            ??
            (this.menuParameterInverterCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.ParameterInverter),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual));

        public ICommand MenuParametersCommand =>
            this.menuParametersCommand
            ??
            (this.menuParametersCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.Parameters),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual));

        public ICommand MenuUsersCommand =>
            this.menuUsersCommand
            ??
            (this.menuUsersCommand = new DelegateCommand(
                () => this.MenuCommandOther(MenuOther.Users),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.menuComunicationWMSCommand?.RaiseCanExecuteChanged();
            this.menuUsersCommand?.RaiseCanExecuteChanged();
            this.menuParametersCommand?.RaiseCanExecuteChanged();
            this.menuParameterInverterCommand?.RaiseCanExecuteChanged();
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
                            Utils.Modules.Installation.COMUNICATIONWMS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOther.ParameterInverter:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.PARAMETERINVERTER,
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
