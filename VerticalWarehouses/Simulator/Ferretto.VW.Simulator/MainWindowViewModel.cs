using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.Simulator.Services.Interfaces;
using Ferretto.VW.Simulator.Services.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using VertimagConfiguration = Ferretto.VW.MAS.DataModels.VertimagConfiguration;

namespace Ferretto.VW.Simulator
{
    public class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly ICultureService cultureService;

        private readonly IThemeService themeService;

        private string configurationName;

        private ICommand errorGeneratorInverterCommand;

        private ICommand errorGeneratorIODeviceCommand;

        private string errorMessage;

        private ICommand importConfigurationCommand;

        private bool isBusy;

        private IMachineService machineService;

        private ICommand simulateErrorCommand;

        private int simulateSpeed = 1;

        private ICommand startSimulatorCommand;

        private ICommand stopSimulatorCommand;

        private ICommand toggleCultureCommand;

        private ICommand toggleThemeCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IMachineService inverterService,
            IThemeService themeService,
            ICultureService cultureService
            )
        {
            this.machineService = inverterService ?? throw new ArgumentNullException(nameof(inverterService));
            this.themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            this.cultureService = cultureService ?? throw new ArgumentNullException(nameof(cultureService));
        }

        #endregion

        #region Properties

        public string ConfigurationName
        {
            get => this.configurationName;
            private set
            {
                if (this.configurationName != value)
                {
                    this.SetProperty(ref this.configurationName, value);
                }
            }
        }

        public ICommand ErrorGeneratorInverterCommand =>
            this.errorGeneratorInverterCommand
            ??
            (this.errorGeneratorInverterCommand = new DelegateCommand(async () => await this.machineService.ProcessErrorGeneratorInverterAsync()));

        public ICommand ErrorGeneratorIODeviceCommand =>
            this.errorGeneratorIODeviceCommand
            ??
            (this.errorGeneratorIODeviceCommand = new DelegateCommand(async () => await this.machineService.ProcessErrorGeneratorIODeviceAsync()));

        public string ErrorMessage { get => this.errorMessage; set => this.SetProperty(ref this.errorMessage, value); }

        public ICommand ImportConfigurationCommand =>
            this.importConfigurationCommand
            ??
            (this.importConfigurationCommand = new DelegateCommand(async () => await this.ImportConfiguration()));

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsDarkThemeActive => this.themeService.ActiveTheme == ApplicationTheme.Dark;

        public bool IsEngCultureActive => this.cultureService.ActiveCulture == ApplicationCulture.Eng;

        public IMachineService MachineService
        {
            get => this.machineService;
            set => this.SetProperty(ref this.machineService, value);
        }

        public ICommand SimulateErrorCommand =>
            this.simulateErrorCommand
            ??
            (this.simulateErrorCommand = new DelegateCommand(async () =>
            {
                await this.machineService.SimulateErrorAsync();
            }));

        public int SimulateSpeed
        {
            get => this.simulateSpeed;
            set
            {
                this.SetProperty(ref this.simulateSpeed, value);
                this.machineService.SimulateSpeedAsync(this.SimulateSpeed);
            }
        }

        public ICommand StartSimulatorCommand =>
            this.startSimulatorCommand
            ??
            (this.startSimulatorCommand = new DelegateCommand(async () => await this.machineService.ProcessStartSimulatorAsync()));

        public ICommand StopSimulatorCommand =>
            this.stopSimulatorCommand
            ??
            (this.stopSimulatorCommand = new DelegateCommand(async () => await this.machineService.ProcessStopSimulatorAsync()));

        public ICommand ToggleCultureCommand =>
            this.toggleCultureCommand
            ??
            (this.toggleCultureCommand = new DelegateCommand(() => this.ToggleCulture()));

        public ICommand ToggleThemeCommand =>
                    this.toggleThemeCommand
            ??
            (this.toggleThemeCommand = new DelegateCommand(() => this.ToggleTheme()));

        #endregion

        #region Methods

        public async Task ImportConfiguration()
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.DefaultExt = "json";

                var dir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\Machine Automation Service\Ferretto.VW.MAS.AutomationService\Configuration"));
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
                    await this.machineService.ProcessStopSimulatorAsync();
                    this.LoadConfiguration(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        public void LoadConfiguration(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                }

                string fileContents;
                using (var streamReader = new StreamReader(fileName))
                {
                    fileContents = streamReader.ReadToEnd();
                }

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new IPAddressConverter());
                settings.MissingMemberHandling = MissingMemberHandling.Ignore;

                var vertimagConfiguration = JsonConvert.DeserializeObject<VertimagConfiguration>(fileContents, settings);

                this.machineService.Machine = vertimagConfiguration?.Machine;
                this.ConfigurationName = new FileInfo(fileName).Name;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private void ToggleCulture()
        {
            this.cultureService.ApplyCulture(
                this.cultureService.ActiveCulture == ApplicationCulture.Ita
                    ? ApplicationCulture.Eng
                    : ApplicationCulture.Ita);

            this.RaisePropertyChanged(nameof(this.IsEngCultureActive));
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
