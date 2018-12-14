using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.Navigation;
using System.ComponentModel;
using Ferretto.VW.InverterDriver.Source;
using System.Diagnostics;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.MathLib;

namespace Ferretto.VW.VWApp
{
    internal class MainWindowViewModel : BindableBase, IDataErrorInfo
    {
        #region Fields

        private readonly bool installation_completed;
        private ICommand changeSkin;
        private int currentSelection;
        private bool customComboBoxStateBool = false;
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
            this.installation_completed = DataManager.CurrentData.InstallationInfo.Machine_Ok;
            this.machineModel = DataManager.CurrentData.GeneralInfo.Model;
            this.serialNumber = DataManager.CurrentData.GeneralInfo.Serial;
        }

        #endregion Constructors

        #region Properties

        public ICommand ChangeSkin => this.changeSkin ?? (this.changeSkin = new DelegateCommand(() => (Application.Current as App).ChangeSkin()));

        public Int32 CurrentSelection
        {
            get => this.currentSelection;
            set
            {
                this.SetProperty(ref this.currentSelection, value);
                if (this.CurrentSelection == 0)
                {
                    this.CustomComboBoxStateBool = true;
                }
                else
                {
                    this.CustomComboBoxStateBool = false;
                }
            }
        }

        public Boolean CustomComboBoxStateBool { get => this.customComboBoxStateBool; set => this.SetProperty(ref this.customComboBoxStateBool, value); }

        public string Error
        {
            get { return null; }
        }

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

        private void InitializeInverterConnection()
        {
            InverteDriverManager.InverterDriverStaticInstance = new InverterDriver.InverterDriver();
            if (InverteDriverManager.InverterDriverStaticInstance.Initialize())
            {
                ActionManager.PositioningDrawerInstance = new PositioningDrawer();
                ActionManager.PositioningDrawerInstance.SetInverterDriverInterface = InverteDriverManager.InverterDriverStaticInstance;
                ActionManager.PositioningDrawerInstance.Initialize();  // 1024 is the default value
                ActionManager.ConverterInstance = new Converter();
                ActionManager.ConverterInstance.ManageResolution = 1024;
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
            }

            return validationMessage;
        }

        #endregion Methods
    }
}
