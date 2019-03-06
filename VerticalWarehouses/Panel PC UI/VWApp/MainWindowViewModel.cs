using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Ferretto.VW.Utils.Source;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.VWApp
{
    internal class MainWindowViewModel : BindableBase, IDataErrorInfo
    {
        #region Fields

        public IUnityContainer Container;

        public DataManager Data;

        private ICommand changeSkin;

        private IEventAggregator eventAggregator;

        private bool installationCompleted;

        private ICommand loginButtonCommand;

        private string loginErrorMessage;

        private string machineModel;

        private string passwordLogin;

        private string serialNumber;

        private ICommand switchOffCommand;

        private string userLogin = "Installer";

        #endregion

        #region Constructors

        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public ICommand ChangeSkin => this.changeSkin ?? (this.changeSkin = new DelegateCommand(() => (Application.Current as App).ChangeSkin()));

        public string Error => null;

        public ICommand LoginButtonCommand => this.loginButtonCommand ?? (this.loginButtonCommand = new DelegateCommand(this.ExecuteLoginButtonCommand));

        public string LoginErrorMessage { get => this.loginErrorMessage; set => this.SetProperty(ref this.loginErrorMessage, value); }

        public string MachineModel { get => this.machineModel; set => this.SetProperty(ref this.machineModel, value); }

        public string PasswordLogin { get => this.passwordLogin; set => this.SetProperty(ref this.passwordLogin, value); }

        public string SerialNumber { get => this.serialNumber; set => this.SetProperty(ref this.serialNumber, value); }

        public ICommand SwitchOffCommand => this.switchOffCommand ?? (this.switchOffCommand = new DelegateCommand(() => { Application.Current.Shutdown(); }));

        public string UserLogin { get => this.userLogin; set => this.SetProperty(ref this.userLogin, value); }

        #endregion

        #region Indexers

        public string this[string columnName] => this.Validate(columnName);

        #endregion

        #region Methods

        public void InitializeViewModel(IUnityContainer container)
        {
            this.Container = container;
            this.Data = (DataManager)this.Container.Resolve<IDataManager>();
            if (!this.Data.IsGeneralInfoFilePresent && !this.Data.IsInstallationInfoFilePresent)
            {
                this.LoginErrorMessage = "ERROR: both InstallationInfo and GeneralInfo files are missing.";
            }
            else if (!this.Data.IsGeneralInfoFilePresent)
            {
                if (!this.Data.IsGeneralInfoFilePresent) this.LoginErrorMessage = "ERROR: GeneralInfo file is missing.";
                if (!this.Data.IsInstallationInfoFilePresent) this.LoginErrorMessage = "ERROR: InstallationInfo file is missing.";
            }
            this.installationCompleted = this.Data.InstallationInfo.Machine_Ok;
            this.machineModel = this.Data.GeneralInfo.Model;
            this.serialNumber = this.Data.GeneralInfo.Serial;
        }

        private bool CheckInputCorrectness(string user, string password)
        {
            //TODO implement correct input procedure once the user login structure is defined
            return true;
        }

        private async void ExecuteLoginButtonCommand()
        {
            if (this.CheckInputCorrectness(this.UserLogin, this.PasswordLogin))
            {
                switch (this.UserLogin)
                {
                    case "Installer":
                        try
                        {
                            var ts = ((InstallationHubClient)this.Container.Resolve<IContainerInstallationHubClient>()).ConnectAsync();
                            ((App)Application.Current).InstallationAppMainWindowInstance = ((InstallationApp.MainWindow)this.Container.Resolve<InstallationApp.IMainWindow>());
                            ((App)Application.Current).InstallationAppMainWindowInstance.DataContext = ((InstallationApp.MainWindowViewModel)this.Container.Resolve<IMainWindowViewModel>());
                            await ts;
                            ((App)Application.Current).InstallationAppMainWindowInstance.Show();
                        }
                        catch (Exception)
                        {
                            this.LoginErrorMessage = "Error: Couldn't connect to Machine Automation Service";
                        }

                        break;

                    case "Operator":
                        if (this.installationCompleted)
                        {
                        }
                        else
                        {
                            this.LoginErrorMessage = "Error: Machine's installation not completed yet.";
                        }
                        break;

                    default:
                        this.LoginErrorMessage = Resources.VWApp.ErrorLogin;
                        break;
                }
            }
            else
            {
                this.LoginErrorMessage = Resources.VWApp.ErrorLogin;
            }
        }

        private string Validate(string propertyName)
        {
            var validationMessage = string.Empty;
            switch (propertyName)
            {
                case "UserLogin":
                    if (string.IsNullOrEmpty(this.UserLogin))
                    {
                        validationMessage = "Error";
                    }

                    break;

                default:
                    break;
            }
            return validationMessage;
        }

        #endregion
    }
}
