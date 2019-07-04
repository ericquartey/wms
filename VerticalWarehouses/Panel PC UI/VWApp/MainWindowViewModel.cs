using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.VW.Utils.Source.Events;
using Ferretto.VW.VWApp.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.VWApp
{
    internal class MainWindowViewModel : BindableBase, IDataErrorInfo
    {
        #region Fields

        public IUnityContainer Container;

        private readonly IEventAggregator eventAggregator;

        private ICommand changeSkin;

        private bool isDarkSkinChecked = true;

        private bool isLoginButtonWorking = false;

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
            this.eventAggregator.GetEvent<ChangeSkinEvent>().Subscribe(() => (App.Current as App)?.ChangeSkin());
        }

        #endregion

        #region Properties

        public ICommand ChangeSkin => this.changeSkin ?? (this.changeSkin = new DelegateCommand(() => App.ChangeSkin()));

        public string Error => null;

        public bool IsDarkSkinChecked
        {
            get => this.isDarkSkinChecked;
            set
            {
                this.eventAggregator.GetEvent<ChangeSkinEvent>().Publish();
                this.SetProperty(ref this.isDarkSkinChecked, value);
            }
        }

        public bool IsLoginButtonWorking { get => this.isLoginButtonWorking; set => this.SetProperty(ref this.isLoginButtonWorking, value); }

        public ICommand LoginButtonCommand => this.loginButtonCommand ?? (this.loginButtonCommand = new DelegateCommand(async () => await this.ExecuteLoginButtonCommandAsync()));

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
        }

        private bool CheckInputCorrectness(string user, string password)
        {
            //TODO implement correct input procedure once the user login structure is defined
            return true;
        }

        private async Task ExecuteLoginButtonCommandAsync()
        {
            if (this.CheckInputCorrectness(this.UserLogin, this.PasswordLogin))
            {
                switch (this.UserLogin)
                {
                    case "Installer":
                        try
                        {
                            this.IsLoginButtonWorking = true;
                            ((App)Application.Current).InstallationAppMainWindowInstance = (InstallationApp.MainWindow)this.Container.Resolve<InstallationApp.IMainWindow>();
                            ((App)Application.Current).InstallationAppMainWindowInstance.DataContext = (InstallationApp.MainWindowViewModel)this.Container.Resolve<IMainWindowViewModel>();
                            await this.Container.Resolve<IInstallationHubClient>().ConnectAsync(); // INFO Comment this line for UI development
                            this.Container.Resolve<INotificationCatcher>().SubscribeInstallationMethodsToMAService(); // INFO Comment this line for UI development
                            this.IsLoginButtonWorking = false;
                            (((App)Application.Current).InstallationAppMainWindowInstance.DataContext as InstallationApp.MainWindowViewModel).LoggedUser = "Installer";
                            ((App)Application.Current).InstallationAppMainWindowInstance.Show();
                        }
                        catch (Exception)
                        {
                            this.LoginErrorMessage = "Error: Couldn't connect to Machine Automation Service";
                        }
                        finally
                        {
                            this.IsLoginButtonWorking = false;
                        }
                        break;

                    case "Operator":
                        try
                        {
                            this.IsLoginButtonWorking = true;
                            ((App)Application.Current).OperatorAppMainWindowInstance = (OperatorApp.MainWindow)this.Container.Resolve<OperatorApp.Interfaces.IMainWindow>();
                            ((App)Application.Current).OperatorAppMainWindowInstance.DataContext =
                                (OperatorApp.MainWindowViewModel)this.Container.Resolve<OperatorApp.Interfaces.IMainWindowViewModel>();
                            this.Container.Resolve<INotificationCatcher>().SubscribeOperatorMethodsToMAService();
                            await this.Container.Resolve<IOperatorHubClient>().ConnectAsync(); // INFO Comment this line for UI development
                            (((App)Application.Current).OperatorAppMainWindowInstance.DataContext as OperatorApp.MainWindowViewModel).LoggedUser = "Operator";
                            ((App)Application.Current).OperatorAppMainWindowInstance.Show();
                        }
                        catch (Exception)
                        {
                            this.LoginErrorMessage = "Error: Couldn't connect to Machine Automation Service";
                        }
                        finally
                        {
                            this.IsLoginButtonWorking = false;
                        }
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
