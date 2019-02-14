using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MathLib;
using Ferretto.VW.RemoteIODriver;
using Ferretto.VW.Utils.Source;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;

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

        public RemoteIO remoteIO;

        public SwitchMotors switchMotors;

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

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
        }

        #endregion

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

        #endregion

        #region Indexers

        public string this[string columnName] => this.Validate(columnName);

        #endregion

        #region Methods

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
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

        private void ExecuteLoginButtonCommand()
        {
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

            this.switchMotors = (SwitchMotors)this.Container.Resolve<ISwitchMotors>();
            this.switchMotors.SetInverterDriverInterface = this.Inverter;
            this.switchMotors.SetRemoteIOInterface = this.remoteIO;
            this.switchMotors.Initialize();

            this.converter = (Converter)this.Container.Resolve<IConverter>();
            this.converter.ManageResolution = 1024;
        }

        private void InitializeRemoteIOConnection()
        {
            this.remoteIO = (RemoteIODriver.RemoteIO)this.Container.Resolve<IRemoteIO>();
            try
            {
                this.remoteIO.Connect();
            }
            catch (Exception exc)
            {
            }
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

        #endregion
    }
}
