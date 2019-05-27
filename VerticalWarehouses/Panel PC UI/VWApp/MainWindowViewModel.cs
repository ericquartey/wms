using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.VWApp.Interfaces;
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

        private bool isLoginButtonWorking = false;

        private ICommand loginButtonCommand;

        private string loginErrorMessage;

        private string machineModel;

        private string passwordLogin;

        private string serialNumber;

        private ICommand switchOffCommand;

        private string userLogin = "Operator";

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

        public bool IsLoginButtonWorking { get => this.isLoginButtonWorking; set => this.SetProperty(ref this.isLoginButtonWorking, value); }

        public ICommand LoginButtonCommand => this.loginButtonCommand ?? (this.loginButtonCommand = new DelegateCommand(async () => await this.ExecuteLoginButtonCommand()));

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

        private async Task ExecuteLoginButtonCommand()
        {
            if (this.CheckInputCorrectness(this.UserLogin, this.PasswordLogin))
            {
                switch (this.UserLogin)
                {
                    case "Installer":
                        try
                        {
                            this.IsLoginButtonWorking = true;
                            ((App)Application.Current).InstallationAppMainWindowInstance = ((InstallationApp.MainWindow)this.Container.Resolve<InstallationApp.IMainWindow>());
                            ((App)Application.Current).InstallationAppMainWindowInstance.DataContext = ((InstallationApp.MainWindowViewModel)this.Container.Resolve<IMainWindowViewModel>());
                            //await this.Container.Resolve<IContainerInstallationHubClient>().ConnectAsync(); // INFO Removed this line for UI development
                            //this.Container.Resolve<INotificationCatcher>().SubscribeInstallationMethodsToMAService(); // INFO Removed this line for UI development
                            await Task.Delay(1500); // INFO Fake waiter for UI development
                            this.IsLoginButtonWorking = false;
                            ((App)Application.Current).InstallationAppMainWindowInstance.Show();
                        }
                        catch (Exception)
                        {
                            this.IsLoginButtonWorking = false;
                            this.LoginErrorMessage = "Error: Couldn't connect to Machine Automation Service";
                        }
                        this.IsLoginButtonWorking = false;
                        break;

                    case "Operator":
                        this.IsLoginButtonWorking = true;
                        ((App)Application.Current).OperatorAppMainWindowInstance = ((OperatorApp.MainWindow)this.Container.Resolve<OperatorApp.Interfaces.IMainWindow>());
                        ((App)Application.Current).OperatorAppMainWindowInstance.DataContext = ((OperatorApp.MainWindowViewModel)this.Container.Resolve<OperatorApp.Interfaces.IMainWindowViewModel>());
                        //this.Container.Resolve<INotificationCatcher>().SubscribeInstallationMethodsToMAService(); // INFO Removed this line for UI development
                        //await Task.Delay(1500); // INFO Fake waiter for UI development
                        this.IsLoginButtonWorking = false;
                        ((App)Application.Current).OperatorAppMainWindowInstance.Show();
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
            }
            return validationMessage;
        }

        #endregion
    }
}
