using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;
using Ferretto.Common.Resources;
using System.Diagnostics;

namespace Ferretto.VW.VWApp
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private ICommand loginButtonCommand;
        private string loginErrorMessage;
        private string passwordLogin;
        private ICommand switchOffCommand;
        private string userLogin = "Installer";

        #endregion Fields

        #region Properties

        public ICommand LoginButtonCommand => this.loginButtonCommand ?? (this.loginButtonCommand = new DelegateCommand(this.ExecuteLoginButtonCommand));
        public String LoginErrorMessage { get => this.loginErrorMessage; set => this.SetProperty(ref this.loginErrorMessage, value); }
        public String PasswordLogin { get => this.passwordLogin; set => this.SetProperty(ref this.passwordLogin, value); }
        public ICommand SwitchOffCommand => this.switchOffCommand ?? (this.switchOffCommand = new DelegateCommand(() => { Application.Current.Shutdown(); }));
        public String UserLogin { get => this.userLogin; set => this.SetProperty(ref this.userLogin, value); }

        #endregion Properties

        #region Methods

        private bool CheckInputCorrectness(string user, string password)
        {
            //TODO implement correct input procedure once the user login structure is defined
            return true;
        }

        private void ExecuteLoginButtonCommand()
        {
            if (this.CheckInputCorrectness(this.UserLogin, this.PasswordLogin))
            {
                switch (this.UserLogin)
                {
                    case "Installer":
                        ((App)Application.Current).InstallationAppMainWindowInstance = new InstallationApp.MainWindow();
                        ((App)Application.Current).InstallationAppMainWindowInstance.Show();
                        break;

                    case "Operator":

                        ((App)Application.Current).OperatorMainWindowInstance = new OperatorApp.MainWindow();
                        ((App)Application.Current).OperatorMainWindowInstance.Show();
                        break;

                    default:
                        this.LoginErrorMessage = Common.Resources.VWApp.ErrorLogin;
                        break;
                }
            }
            else
            {
                this.LoginErrorMessage = Common.Resources.VWApp.ErrorLogin;
            }
        }

        #endregion Methods
    }
}
