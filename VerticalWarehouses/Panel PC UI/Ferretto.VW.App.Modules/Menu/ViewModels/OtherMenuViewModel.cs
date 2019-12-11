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
    internal sealed class OtherMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private bool isWaitingForResponse;

        private DelegateCommand menuComunicationWMSCommand;

        private DelegateCommand menuUsersCommand;

        #endregion

        #region Constructors

        public OtherMenuViewModel()
            : base(PresentationMode.Menu)
        {
        }

        #endregion

        #region Enums

        private enum Menu
        {
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

        public ICommand MenuComunicationWmsCommand =>
            this.menuComunicationWMSCommand
            ??
            (this.menuComunicationWMSCommand = new DelegateCommand(
                () => this.MenuCommand(Menu.ComunicationWms),
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
            this.menuComunicationWMSCommand?.RaiseCanExecuteChanged();
            this.menuUsersCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
