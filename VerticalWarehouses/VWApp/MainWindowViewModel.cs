using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.MathLib;
using Ferretto.VW.Utils.Source;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.VWApp
{
    internal class MainWindowViewModel : BindableBase, IDataErrorInfo
    {
        #region Fields

        public CalibrateVerticalAxis CalibrateVerticalAxis;
        public IUnityContainer Container;
        public DataManager Data;
        public DrawerWeightDetection DrawerWeightDetection;
        public InverterDriver.InverterDriver Inverter;
        public PositioningDrawer PositioningDrawer;
        private ICommand changeSkin;
        private Converter converter;
        private bool installationCompleted;
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
        }

        #endregion Constructors

        #region Properties

        public ICommand ChangeSkin => this.changeSkin ?? (this.changeSkin = new DelegateCommand(() => (Application.Current as App).ChangeSkin()));

        public string Error => null;

        public ICommand LoginButtonCommand => this.loginButtonCommand ?? (this.loginButtonCommand = new DelegateCommand(this.ExecuteLoginButtonCommand));

        public String LoginErrorMessage { get => this.loginErrorMessage; set => this.SetProperty(ref this.loginErrorMessage, value); }

        public String MachineModel { get => this.machineModel; set => this.SetProperty(ref this.machineModel, value); }

        public String PasswordLogin { get => this.passwordLogin; set => this.SetProperty(ref this.passwordLogin, value); }

        public String SerialNumber { get => this.serialNumber; set => this.SetProperty(ref this.serialNumber, value); }

        public ICommand SwitchOffCommand => this.switchOffCommand ?? (this.switchOffCommand = new DelegateCommand(() => { Application.Current.Shutdown(); }));

        public String UserLogin { get => this.userLogin; set => this.SetProperty(ref this.userLogin, value); }

        #endregion Properties

        #region Indexers

        public string this[string columnName] => this.Validate(columnName);

        #endregion Indexers

        #region Methods

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
            this.Data = (DataManager)this.Container.Resolve<IDataManager>();
            this.installationCompleted = this.Data.InstallationInfo.Machine_Ok;
            this.machineModel = this.Data.GeneralInfo.Model;
            this.serialNumber = this.Data.GeneralInfo.Serial;
        }

        private bool CheckInputCorrectness(string user, string password)
        {
            //TODO implement correct input procedure once the user login structure is defined
            return true;
        }

        private void ExecuteLoginButtonCommand()
        {
            this.InitializeInverterConnection();
            if (this.CheckInputCorrectness(this.UserLogin, this.PasswordLogin))
            {
                switch (this.UserLogin)
                {
                    case "Installer":
                        ((App)Application.Current).InstallationAppMainWindowInstance = (InstallationApp.MainWindow)this.Container.Resolve<InstallationApp.IMainWindow>();
                        ((App)Application.Current).InstallationAppMainWindowInstance.DataContext = (InstallationApp.MainWindowViewModel)this.Container.Resolve<IMainWindowViewModel>();
                        ((App)Application.Current).InstallationAppMainWindowInstance.Show();
                        break;

                    case "Operator":
                        if (this.installationCompleted)
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

        private void InitializeInverterConnection()
        {
            this.Inverter = (InverterDriver.InverterDriver)this.Container.Resolve<IInverterDriver>();
            this.PositioningDrawer = (PositioningDrawer)this.Container.Resolve<IPositioningDrawer>();
            this.PositioningDrawer.SetInverterDriverInterface = this.Inverter;
            this.PositioningDrawer.Initialize();  // 1024 is the default value

            this.DrawerWeightDetection = (DrawerWeightDetection)this.Container.Resolve<IDrawerWeightDetection>();
            this.DrawerWeightDetection.SetPositioningDrawerInterface = this.PositioningDrawer;
            this.DrawerWeightDetection.Initialize();

            this.CalibrateVerticalAxis = (CalibrateVerticalAxis)this.Container.Resolve<ICalibrateVerticalAxis>();

            this.converter = (Converter)this.Container.Resolve<IConverter>();
            this.converter.ManageResolution = 1024;
        }

        private string Validate(string propertyName)
        {
            var validationMessage = string.Empty;
            switch (propertyName)
            {
                case "UserLogin":
                    if (this.UserLogin == "")
                    {
                        validationMessage = "Error";
                    }
                    break;

                default:
                    break;
            }
            return validationMessage;
        }

        #endregion Methods
    }
}
