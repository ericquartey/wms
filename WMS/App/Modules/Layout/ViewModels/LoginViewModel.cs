﻿using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Layout
{
    public class LoginViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IAuthenticationProvider authenticationProvider = ServiceLocator.Current.GetInstance<IAuthenticationProvider>();

        private bool isBusy;

        private bool isEnabled;

        private string loginCheck;

        private ICommand loginCommand;

        private string status;

        private string validationError;

        #endregion

        #region Constructors

        public LoginViewModel()
        {
            this.User = new User();
            this.LoginCheck = Common.Resources.Layout.Access;
            this.IsBusy = false;
            this.IsEnabled = true;
            this.Status = Icons.ResourceManager.GetString(nameof(Icons.NavigationForward));
            this.User.PropertyChanged += this.OnItemPropertyChanged;
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        public string LoginCheck
        {
            get => this.loginCheck;
            set => this.SetProperty(ref this.loginCheck, value);
        }

        public ICommand LoginCommand => this.loginCommand ??
                                        (this.loginCommand = new DelegateCommand(
                                             async () => await this.ExecuteLoginAsync().ConfigureAwait(true),
                                             this.CanLogin));

        public string Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        public User User { get; set; }

        public string ValidationError
        {
            get => this.validationError;
            set => this.SetProperty(ref this.validationError, value);
        }

        #endregion

        #region Methods

        public override bool KeyPress(ShortKeyInfo shortKeyInfo)
        {
            if (shortKeyInfo != null &&
                shortKeyInfo.ShortKey.IsControlShift &&
                shortKeyInfo.ShortKey.Key == Key.O)
            {
                this.AccessGranted();
                return true;
            }

            if (shortKeyInfo != null &&
                shortKeyInfo.ShortKey.Key == Key.Enter)
            {
                this.loginCommand.Execute(null);
                return true;
            }

            return base.KeyPress(shortKeyInfo);
        }

        protected override void OnDispose()
        {
            this.User.PropertyChanged -= this.OnItemPropertyChanged;
            base.OnDispose();
        }

        private void AccessGranted()
        {
            this.NavigationService.StartPresentation(() => this.NotifyAccess(), () => this.Disappear());
        }

        private bool CanLogin()
        {
            if (string.IsNullOrEmpty(this.User.Login) ||
                string.IsNullOrEmpty(this.User.Password))
            {
                return false;
            }

            return true;
        }

        private async Task ExecuteLoginAsync()
        {
#if DEBUG
            this.User.Login = System.Environment.UserName;
            await Task.Delay(250);
            this.User.Password = "password";
#endif

            var result = await this.authenticationProvider.LoginAsync(this.User.Login, this.User.Password);

            this.ValidationError = result.Description;
            if (result.Success == false)
            {
                return;
            }

            await Task.Run(() =>
            {
                this.AccessGranted();
            });
        }

        private void NotifyAccess()
        {
            this.IsBusy = true;
            this.IsEnabled = false;
            this.Status = Icons.ResourceManager.GetString(nameof(Icons.NavigationCheck));
            this.LoginCheck = Common.Resources.Layout.Ok;
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.ValidationError = string.Empty;
            ((DelegateCommand)this.LoginCommand)?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
