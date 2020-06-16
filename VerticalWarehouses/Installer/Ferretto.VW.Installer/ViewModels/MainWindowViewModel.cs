using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private const string StepsSnapshotFileName = "steps-snapshot.json";

        private const string StepsDefinitionFileName = "steps.json";

        #region Fields

        private readonly ICommand closeCommand;

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        private IOperationResult baySelectionViewModel;

        private IOperationResult activeViewModel;

        private string errorMessage;

        private bool hasError;

        private InstallationService installationService;

        private IOperationResult roleSelectionViewModel;

        private IOperationResult updateViewModel;

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            this.closeCommand = new RelayCommand(this.Close);
        }

        #endregion

        #region Properties

        public IOperationResult ActiveViewModel
        {
            get => this.activeViewModel;
            set => this.SetProperty(ref this.activeViewModel, value);
        }

        public ICommand CloseCommand => this.closeCommand;

        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        public bool HasError
        {
            get => this.hasError;
            set => this.SetProperty(ref this.hasError, value);
        }

        #endregion

        #region Methods

        public void StartInstallation()
        {
            var installerDirName = ConfigurationManager.AppSettings.GetInstallerDirName();
            var installerLocation = $"{ConfigurationManager.AppSettings.GetUpdateTempPath()}\\{installerDirName}";

#if !DEBUG
            if (!Directory.Exists(installerLocation))
                {
                    this.ShowError($"Folder '{installerLocation}' does not exists.");
                    return;
                }

            Directory.SetCurrentDirectory(installerLocation);
#endif
            var stepsFileFound = false;

            try
            {
                if (File.Exists(StepsSnapshotFileName))
                {
                    this.installationService = new InstallationService(StepsSnapshotFileName);
                    this.installationService.GetInfoFromSnapShot();
                    stepsFileFound = true;
                }
                else if (File.Exists(StepsDefinitionFileName))
                {
                    this.installationService = new InstallationService(StepsDefinitionFileName);
                    stepsFileFound = true;
                }

                if (stepsFileFound)
                {
                    this.installationService.SetArgsStartup();

                    this.installationService.GetInstallerParameters();

                    this.installationService.UpdateMachineRole();

                    this.installationService.LoadSteps();

                    this.installationService.PropertyChanged += this.InstallationService_PropertyChanged;

                    if (!this.installationService.CanStart())
                    {
                        this.Close();
                    }

                    this.installationService.Start();
                }
                else
                {
                    this.ShowError($"Steps file not found in folder '{Directory.GetCurrentDirectory()}'.");
                }
            }
            catch (Exception ex)
            {
                this.ShowError($"Unable to initialize installation: {ex.Message}");
            }
        }

        private void Close()
        {
            Application.Current.Shutdown(this.ActiveViewModel?.IsSuccessful == true ? 0 : -1);
        }

        private void InstallationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OperationStage))
            {
                switch (this.installationService.OperationStage)
                {
                    case OperationStage.None:
                        throw new InvalidOperationException("Can't change on current mode:Operation type not supported.");

                    case OperationStage.RoleSelection:
                        this.ActiveViewModel = (this.roleSelectionViewModel ??= new RoleSelectionViewModel(this.installationService));
                        break;

                    case OperationStage.BaySelection:
                        this.ActiveViewModel = (this.baySelectionViewModel ??= new BaySelectionViewModel(this.installationService));
                        break;

                    case OperationStage.Update:
                        this.ActiveViewModel = this.updateViewModel ?? (this.updateViewModel = new StepsViewModel(this.installationService));
                        break;

                    default:
                        break;
                }
            }
        }

        private void ShowError(string message)
        {
            this.logger.Error(message);
            this.HasError = true;
            this.ErrorMessage = message;
        }

        #endregion
    }
}
