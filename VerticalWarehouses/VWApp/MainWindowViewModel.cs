using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;

namespace Ferretto.VW.VWApp
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private ICommand closeErrorPopupButtonCommand;
        private bool enableErrorPopup;
        private ICommand loginButtonCommand;
        private string passwordLogin;
        private ICommand switchOffButtonCommand;
        private string userLogin;

        #endregion Fields

        #region Properties

        public ICommand CloseErrorPopupButtonCommand => this.closeErrorPopupButtonCommand ?? (this.closeErrorPopupButtonCommand = new DelegateCommand(this.ExecuteCloseErrorPopupButtonCommand));
        public bool EnableErrorPopup { get => this.enableErrorPopup; set => this.SetProperty(ref this.enableErrorPopup, value); }
        public ICommand LoginButtonCommand => this.loginButtonCommand ?? (this.loginButtonCommand = new DelegateCommand(this.ExecuteLoginButtonCommand));
        public String PasswordLogin { get => this.passwordLogin; set => this.SetProperty(ref this.passwordLogin, value); }
        public ICommand SwitchOffButtonCommand => this.switchOffButtonCommand ?? (this.switchOffButtonCommand = new DelegateCommand(this.ExecuteSwitchOffButtonCommand));
        public String UserLogin { get => this.userLogin; set => this.SetProperty(ref this.userLogin, value); }

        #endregion Properties

        #region Methods

        private bool CheckInputCorrectness(string user, string password)
        {
            //TODO implement correct input procedure once the user login structure is defined
            return true;
        }

        private void ExecuteCloseErrorPopupButtonCommand()
        {
            this.EnableErrorPopup = false;
        }

        private void ExecuteLoginButtonCommand()
        {
            if (this.CheckInputCorrectness(this.UserLogin, this.PasswordLogin))
            {
                if (this.UserLogin == "Installer")
                {
                    ((App)Application.Current).MainWindow.Hide();
                    ((App)Application.Current).InstallationMainWindowInstance = new InstallationApp.MainWindow();
                    ((App)Application.Current).InstallationMainWindowInstance.Show();
                }
                else if (this.UserLogin == "Operator")
                {
                    ((App)Application.Current).MainWindow.Hide();
                    ((App)Application.Current).OperatorMainWindowInstance = new OperatorApp.MainWindow();
                    ((App)Application.Current).OperatorMainWindowInstance.Show();
                }
                else if (this.UserLogin == "Installer1") //PRESENTATION ONLY: REMOVE THOSE 2 BLOCKS BEFORE PRODUCTION
                {
                    ((App)Application.Current).MainWindow.Hide();
                    ((App)Application.Current).InstallationWindow1Instance = new InstallationApp.Window1();
                    ((App)Application.Current).InstallationWindow1Instance.Show();
                }
                else if (this.UserLogin == "Installer2")
                {
                    ((App)Application.Current).MainWindow.Hide();
                    ((App)Application.Current).InstallationWindow2Instance = new InstallationApp.Window2();
                    ((App)Application.Current).InstallationWindow2Instance.Show();
                }
                else //TODO: remove this block once CheckLoginInput is implemented.
                {
                    this.EnableErrorPopup = true;
                    //TODO: create error message for both wrong user/password and installation incomplete
                }
            }
            else
            {
                this.EnableErrorPopup = true;
                //TODO: open a popup to communicate to the user that the login info are not correct
            }
        }

        private void ExecuteSwitchOffButtonCommand()
        {
            Application.Current.Shutdown();
        }

        #endregion Methods
    }
}
