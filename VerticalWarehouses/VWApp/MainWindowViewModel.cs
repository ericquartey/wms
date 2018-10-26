using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.VWApp
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly bool installation_completed;
        private ICommand loginButtonCommand;
        private string loginErrorMessage;
        private string machineModel;
        private string passwordLogin;
        private string serialNumber;
        private ICommand switchOffCommand;
        private string userLogin = "Installer";

        #endregion Fields

        #region Constructors

        public MainWindowViewModel()
        {
            this.installation_completed = DataManager.InstallationInfo.Machine_Ok;
            this.machineModel = DataMngr.CurrentData.GeneralInfo.Model;
            this.serialNumber = DataMngr.CurrentData.GeneralInfo.Serial;
        }

        #endregion Constructors

        #region Properties

        public ICommand LoginButtonCommand => this.loginButtonCommand ?? (this.loginButtonCommand = new DelegateCommand(this.ExecuteLoginButtonCommand));
        public String LoginErrorMessage { get => this.loginErrorMessage; set => this.SetProperty(ref this.loginErrorMessage, value); }
        public String MachineModel { get => this.machineModel; set => this.SetProperty(ref this.machineModel, value); }
        public String PasswordLogin { get => this.passwordLogin; set => this.SetProperty(ref this.passwordLogin, value); }
        public String SerialNumber { get => this.serialNumber; set => this.SetProperty(ref this.serialNumber, value); }
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
                        if (this.installation_completed)
                        {
                            ((App)Application.Current).OperatorMainWindowInstance = new OperatorApp.MainWindow();
                            ((App)Application.Current).OperatorMainWindowInstance.Show();
                        }
                        else
                        {
                            this.LoginErrorMessage = "Error: Machine's installation not completed yet.";
                        }
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
