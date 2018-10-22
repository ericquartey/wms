using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;
using Ferretto.Common.Resources;

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
                        ((App)Application.Current).MainWindow.Hide();
                        ((App)Application.Current).InstallationMainWindowInstance = new InstallationApp.MainWindow();
                        ((App)Application.Current).InstallationMainWindowInstance.Show();
                        break;

                    case "Operator":

                        ((App)Application.Current).MainWindow.Hide();
                        ((App)Application.Current).OperatorMainWindowInstance = new OperatorApp.MainWindow();
                        ((App)Application.Current).OperatorMainWindowInstance.Show();
                        break;

                    case "Installer1": //PRESENTATION ONLY: REMOVE THOSE 2 BLOCKS BEFORE PRODUCTION

                        ((App)Application.Current).MainWindow.Hide();
                        ((App)Application.Current).InstallationWindow1Instance = new InstallationApp.Window1();
                        ((App)Application.Current).InstallationWindow1Instance.Show();
                        break;

                    case "Installer2":

                        ((App)Application.Current).MainWindow.Hide();
                        ((App)Application.Current).InstallationWindow2Instance = new InstallationApp.Window2();
                        ((App)Application.Current).InstallationWindow2Instance.Show();
                        break;

                    default: //TODO SUGGESTION: remove this once CheckLoginInput is implemented.
                        this.LoginErrorMessage = Common.Resources.VWApp.ErrorLogin;
                        //TODO: create error message for both wrong user/password and installation incomplete
                        break;
                }
            }
            else
            {
                this.LoginErrorMessage = Common.Resources.VWApp.ErrorLogin;
                //TODO: open a popup to communicate to the user that the login info are not correct
            }
        }

        #endregion Methods
    }
}
