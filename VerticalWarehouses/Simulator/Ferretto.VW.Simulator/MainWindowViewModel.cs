using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.Simulator.Services.Interfaces;
using Ferretto.VW.Simulator.Services.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator
{
    public class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly IThemeService themeService;

        private string errorMessage;

        private ICommand importConfigurationCommand;

        private IMachineService inverterService;

        private bool isBusy;

        private ICommand startSimulatorCommand;

        private ICommand stopSimulatorCommand;

        private ICommand toggleThemeCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IMachineService inverterService,
            IThemeService themeService)
        {
            this.inverterService = inverterService ?? throw new ArgumentNullException(nameof(inverterService));
            this.themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        }

        #endregion

        #region Properties

        public string ErrorMessage { get => this.errorMessage; set => this.SetProperty(ref this.errorMessage, value); }

        public ICommand ImportConfigurationCommand =>
            this.importConfigurationCommand
            ??
            (this.importConfigurationCommand = new DelegateCommand(async () => await this.ImportConfigurationAsync()));

        public IMachineService InverterService
        {
            get => this.inverterService;
            set => this.SetProperty(ref this.inverterService, value);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsDarkThemeActive => this.themeService.ActiveTheme == ApplicationTheme.Dark;

        public ICommand StartSimulatorCommand =>
            this.startSimulatorCommand
            ??
            (this.startSimulatorCommand = new DelegateCommand(async () => await this.inverterService.ProcessStartSimulatorAsync()));

        public ICommand StopSimulatorCommand =>
            this.stopSimulatorCommand
            ??
            (this.stopSimulatorCommand = new DelegateCommand(async () => await this.inverterService.ProcessStopSimulatorAsync()));

        public ICommand ToggleThemeCommand =>
            this.toggleThemeCommand
            ??
            (this.toggleThemeCommand = new DelegateCommand(() => this.ToggleTheme()));

        #endregion

        #region Methods

        public async Task ImportConfigurationAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.DefaultExt = "json";

                string dir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\Machine Automation Service\Ferretto.VW.MAS.AutomationService\Configuration"));
                if (Directory.Exists(dir))
                {
                    openFileDialog.InitialDirectory = dir;
                }
                else
                {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }

                if (openFileDialog.ShowDialog() == true)
                {
                    string fileContents;

                    Directory.SetCurrentDirectory(openFileDialog.InitialDirectory);
                    using (var streamReader = new StreamReader(openFileDialog.FileName))
                    {
                        fileContents = await streamReader.ReadToEndAsync();
                    }

                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new IPAddressConverter());
                    settings.MissingMemberHandling = MissingMemberHandling.Ignore;

                    var vertimagConfiguration = JsonConvert.DeserializeObject<VertimagConfiguration>(fileContents, settings);

                    this.inverterService.Machine = vertimagConfiguration?.Machine;
                }
            }
            catch (Exception e)
            {
            }
        }

        private void ToggleTheme()
        {
            this.themeService.ApplyTheme(
                this.themeService.ActiveTheme == ApplicationTheme.Light
                    ? ApplicationTheme.Dark
                    : ApplicationTheme.Light);

            this.RaisePropertyChanged(nameof(this.IsDarkThemeActive));
        }

        #endregion
    }
}
