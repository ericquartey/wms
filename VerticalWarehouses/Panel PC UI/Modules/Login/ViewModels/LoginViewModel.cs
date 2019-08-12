using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Login.ViewModels
{
    public class LoginViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private ICommand loginCommand;

        #endregion

        #region Constructors

        public LoginViewModel(
            IAuthenticationService authenticationService)
            : base(PresentationMode.Login)
        {
            if (authenticationService == null)
            {
                throw new ArgumentNullException(nameof(authenticationService));
            }

            this.authenticationService = authenticationService;

#if DEBUG
            this.UserLogin = new UserLogin
            {
                UserName = "installer",
                Password = "password",
            };
#else
            this.UserLogin = new UserLogin();
#endif
        }

        #endregion

        #region Properties

        public ICommand LoginCommand =>
            this.loginCommand
            ??
            (this.loginCommand = new DelegateCommand(async () => await this.ExecuteLoginCommandAsync(), this.CanExecuteLogin));

        private bool CanExecuteLogin()
        {
            return this.machineIdentity != null
                &&
                string.IsNullOrEmpty(this.UserLogin.Error);
        }

        public UserLogin UserLogin { get; }

        public MachineIdentity MachineIdentity
        {
            get => this.machineIdentity;
            set
            {
                if (this.SetProperty(ref this.machineIdentity, value))
                {
                    ((DelegateCommand)this.LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private MachineIdentity machineIdentity;

        #endregion

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            if (this.Data is MachineIdentity machineIdentity)
            {
                this.MachineIdentity = machineIdentity;
            }
        }

        private async Task ExecuteLoginCommandAsync()
        {
            this.ShowError(string.Empty);

            this.UserLogin.IsValidationEnabled = true;
            if (!string.IsNullOrEmpty(this.UserLogin.Error))
            {
                this.ShowError(this.UserLogin.Error);
                return;
            }

            this.NavigationService.SetBusy(true);

            var claims = await this.authenticationService.LogInAsync(
               this.UserLogin.UserName,
               this.UserLogin.Password);

            this.NavigationService.SetBusy(false);

            if (claims != null)
            {
                if (claims.AccessLevel == UserAccessLevel.SuperUser)
                {
                    this.LoadInstallerModule();
                }
                else
                {
                    this.LoadOperatorModule();
                }
            }
            else
            {
                this.ShowError(Resources.Errors.UserLogin_InvalidCredentials);
            }
        }

        private void LoadInstallerModule()
        {
            try
            {
                this.NavigationService.LoadModule(nameof(Utils.Modules.Installation));

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.INSTALLATORMENU, true);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void LoadOperatorModule()
        {
            try
            {
                this.NavigationService.LoadModule(nameof(Utils.Modules.Operator));

                this.NavigationService.Appear(nameof(Utils.Modules.Operator), "TODO", true); // TODO
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }
    }
}
