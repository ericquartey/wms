﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Layout
{
    public class PresentationLogged : BasePresentationViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly INavigationService navigationService;

        private readonly IThemeService themeService;

        private bool isPopupOpen;

        private DelegateCommand logOutCommand;

        private DelegateCommand toggleThemeCommand;

        private string userName;

        #endregion

        #region Constructors

        public PresentationLogged(
            IAuthenticationService authenticationService,
            INavigationService navigationService,
            IMachineBaysWebService machineBaysWebService,
            IThemeService themeService)
            : base(PresentationTypes.Logged)
        {
            if (authenticationService is null)
            {
                throw new ArgumentNullException(nameof(authenticationService));
            }

            if (navigationService is null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            if (machineBaysWebService is null)
            {
                throw new ArgumentNullException(nameof(machineBaysWebService));
            }

            if (themeService is null)
            {
                throw new ArgumentNullException(nameof(themeService));
            }

            this.authenticationService = authenticationService;
            this.navigationService = navigationService;
            this.machineBaysWebService = machineBaysWebService;
            this.themeService = themeService;
            this.Type = PresentationTypes.Logged;

            this.authenticationService.UserAuthenticated += this.AuthenticationService_UserAuthenticated;
        }

        #endregion

        #region Properties

        public bool IsPopupOpen
        {
            get => this.isPopupOpen;
            set
            {
                if (this.SetProperty(ref this.isPopupOpen, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand LogOutCommand =>
            this.logOutCommand
            ??
            (this.logOutCommand = new DelegateCommand(async () => await this.ExecuteLogOutCommand()));

        public ICommand ToggleThemeCommand =>
         this.toggleThemeCommand
         ??
         (this.toggleThemeCommand = new DelegateCommand(this.ExecuteToggleThemeCommand));

        public string UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.IsPopupOpen = !this.IsPopupOpen;

            return Task.CompletedTask;
        }

        private void AuthenticationService_UserAuthenticated(object sender, UserAuthenticatedEventArgs e)
        {
            this.UserName = this.authenticationService.UserName;
        }

        private async Task ExecuteLogOutCommand()
        {
            ScaffolderUserAccesLevel.IsLogged = false;
            this.IsPopupOpen = false;
            await this.authenticationService.LogOutAsync();
            await this.machineBaysWebService.DeactivateAsync();
            this.navigationService.GoBackTo(nameof(Utils.Modules.Login), Utils.Modules.Login.LOGIN);
        }

        private void ExecuteToggleThemeCommand()
        {
            this.IsPopupOpen = false;

            this.themeService.ApplyTheme(
               this.themeService.ActiveTheme == ApplicationTheme.Light
                   ? ApplicationTheme.Dark
                   : ApplicationTheme.Light);
        }

        #endregion
    }
}
