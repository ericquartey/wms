using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
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

        private DelegateCommand menuOldCommand;

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

        private enum MenuOlther
        {
            Users,

            Parameters,

            ParameterInverter,

            Old,

            ComunicationWms,
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand MenuComunicationWmsCommand =>
            this.menuComunicationWMSCommand
            ??
            (this.menuComunicationWMSCommand = new DelegateCommand(
                () => this.MenuCommandOlther(MenuOlther.ComunicationWms),
                this.CanExecuteCommand));

        public ICommand MenuOldCommand =>
                    this.menuOldCommand
            ??
            (this.menuOldCommand = new DelegateCommand(
                () => this.MenuCommandOlther(MenuOlther.Old),
                this.CanExecuteCommand));

        public ICommand MenuParameterInverterCommand =>
            this.menuParameterInverterCommand
            ??
            (this.menuParameterInverterCommand = new DelegateCommand(
                () => this.MenuCommandOlther(MenuOlther.ParameterInverter),
                this.CanExecuteCommand));

        public ICommand MenuParametersCommand =>
            this.menuParametersCommand
            ??
            (this.menuParametersCommand = new DelegateCommand(
                () => this.MenuCommandOlther(MenuOlther.Parameters),
                this.CanExecuteCommand));

        public ICommand MenuUsersCommand =>
            this.menuUsersCommand
            ??
            (this.menuUsersCommand = new DelegateCommand(
                () => this.MenuCommandOlther(MenuOlther.Users),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        internal override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.menuComunicationWMSCommand?.RaiseCanExecuteChanged();
            this.menuUsersCommand?.RaiseCanExecuteChanged();
            this.menuOldCommand?.RaiseCanExecuteChanged();
            this.menuParametersCommand?.RaiseCanExecuteChanged();
            this.menuParameterInverterCommand?.RaiseCanExecuteChanged();
        }

        private void MenuCommandOlther(MenuOlther menu)
        {
            this.ClearNotifications();

            this.Logger.Trace($"MenuCommand({menu})");

            this.IsWaitingForResponse = true;

            try
            {
                switch (menu)
                {
                    case MenuOlther.Parameters:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.Parameters.PARAMETERS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOlther.Old:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.INSTALLATORMENU,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOlther.Users:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.USERS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOlther.ComunicationWms:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.COMUNICATIONWMS,
                            data: null,
                            trackCurrentView: true);
                        break;

                    case MenuOlther.ParameterInverter:
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
