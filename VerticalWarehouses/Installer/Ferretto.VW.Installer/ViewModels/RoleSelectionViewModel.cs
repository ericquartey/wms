using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.Installer.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ferretto.VW.Installer.ViewModels
{
    internal sealed class RoleSelectionViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly InstallationService installationService;

        private bool canNext;

        private RelayCommand checkMasConfigurationCommand;

        private bool isMasConfiguration;

        private bool isMasConfigurationValid;

        private bool isSlaveConfigurationValid;

        private MAS.DataModels.VertimagConfiguration masConfiguration;

        private IPAddress masIpAddress;

        private IPEndPoint masIpEndpoint;

        private string messageMaster;

        private string messageSlave;

        private RelayCommand nextCommand;

        private RelayCommand openFileCommand;

        private string serviceVersion;

        private string uiVersion;

        #endregion

        #region Constructors

        public RoleSelectionViewModel(InstallationService installationService)
        {
            this.installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));

            this.Initialize();
        }

        #endregion

        #region Properties

        public ICommand CheckMasConfigurationCommand =>
            this.checkMasConfigurationCommand ??= new RelayCommand(
                async () => await this.EvaluateCanNextAsync(),
                this.CanCheckMasConfiguration);

        public bool DialogResult { get; protected set; }

        public bool IsMasConfiguration
        {
            get => this.isMasConfiguration;
            set => this.SetProperty(ref this.isMasConfiguration, value, async () => await this.EvaluateCanNextAsync());
        }

        public bool IsSuccessful { get; private set; }

        public string MachineConfigurationFileName { get; protected set; }

        public IPAddress MasIpAddress
        {
            get => this.masIpAddress;
            set => this.SetProperty(ref this.masIpAddress, value, this.EvaluateCheckMasHost);
        }

        public string MessageMaster
        {
            get => this.messageMaster;
            set => this.SetProperty(ref this.messageMaster, value);
        }

        public string MessageSlave
        {
            get => this.messageSlave;
            set => this.SetProperty(ref this.messageSlave, value);
        }

        public ICommand NextCommand =>
            this.nextCommand
            ??
            (this.nextCommand = new RelayCommand(this.Next, this.CanNext));

        public ICommand OpenFileCommand =>
            this.openFileCommand
            ??
            (this.openFileCommand = new RelayCommand(
                async () => await this.OpenFileAsync(),
                this.CanOpenFile));

        public string PanelPcVersion
        {
            get => this.uiVersion;
            set => this.SetProperty(ref this.uiVersion, value);
        }

        public string ServiceVersion
        {
            get => this.serviceVersion;
            set => this.SetProperty(ref this.serviceVersion, value);
        }

        public string SoftwareVersion => string.Format("Welcome to installation {0}", this.installationService?.SoftwareVersion);

        public string UiVersion
        {
            get => this.uiVersion;
            set => this.SetProperty(ref this.uiVersion, value);
        }

        #endregion

        #region Methods

        public async Task OpenFileAsync()
        {
            var selectedFileNames = DialogService.BrowseFile(
                "Scegli file di configurazione",
                string.Empty,
                "json",
                "File di configuirazione");

            if (selectedFileNames?.Length == 1)
            {
                var fileName = selectedFileNames.First();

                try
                {
                    using var sr = new StreamReader(fileName);
                    var fileContents = sr.ReadToEnd();

                    this.LoadMachineConfiguration(fileContents);

                    var configurationFilePath = Path.Combine(
                        ConfigurationManager.AppSettings.GetUpdateTempPath(),
                        ConfigurationManager.AppSettings.GetMasDirName(),
                        @"\Configuration\vertimag-configuration.json");

                    this.installationService.SaveVertimagConfiguration(configurationFilePath, fileContents);

                    this.isMasConfigurationValid = true;
                }
                catch
                {
                    this.isMasConfigurationValid = false;
                }
            }
            else
            {
                this.MachineConfigurationFileName = string.Empty;
                this.isMasConfigurationValid = false;
            }

            await this.EvaluateCanNextAsync();
        }

        public void Save()
        {
            this.IsSuccessful = true;
        }

        private bool CanCheckMasConfiguration()
        {
            return this.masIpEndpoint != null;
        }

        private bool CanNext()
        {
            return this.canNext;
        }

        private bool CanOpenFile()
        {
            return true;
        }

        private IPEndPoint CheckMasHost()
        {
            this.MessageSlave = string.Empty;
            int? ipPort = null;
            try
            {
                if (int.TryParse(ConfigurationManager.AppSettings.GetInstallDefaultMasIpport(), out var port))
                {
                    ipPort = port;
                    var ipEndpoint = new IPEndPoint(this.masIpAddress, port);

                    var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                    var result = sock.BeginConnect(ipEndpoint, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(50, true);
                    if (!success)
                    {
                        ipEndpoint = null;
                    }
                    sock.Close();

                    return ipEndpoint;
                }
            }
            catch
            {
                this.MessageSlave = $"Servizio di automazione non raggiungibile, host: {this.masIpAddress?.ToString()}:{ipPort}";
            }

            return null;
        }

        private async Task EvaluateCanNextAsync()
        {
            this.canNext = false;

            if (this.isMasConfiguration)
            {
                this.MessageMaster = string.Empty;
                if (this.isMasConfigurationValid)
                {
                    this.canNext = true;
                }
            }

            if (!this.isMasConfiguration)
            {
                this.isSlaveConfigurationValid = !(this.masIpEndpoint is null);
                if (this.isSlaveConfigurationValid)
                {
                    if (this.masIpEndpoint != null)
                    {
                        var masConfiguration = await this.GetConfigurationFromMasAsync(this.masIpEndpoint);

                        try
                        {
                            this.LoadMachineConfiguration(masConfiguration);
                            this.isSlaveConfigurationValid = true;
                        }
                        catch
                        {
                            this.isSlaveConfigurationValid = false;
                        }
                    }
                }
                this.canNext = this.isSlaveConfigurationValid;
            }

            this.RaiseCanExecuteChanged();
        }

        private void EvaluateCheckMasHost()
        {
            this.masIpEndpoint = this.CheckMasHost();
            this.RaiseCanExecuteChanged();
        }

        private async Task<string> GetConfigurationFromMasAsync(IPEndPoint ipEndPoint)
        {
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    //httpClient.GetAsync.BaseAddress = new Uri("http://" + ipEndPoint.ToString());
                    var httpResponseMessage = await httpClient.GetAsync(new Uri($"http://{ipEndPoint.Address}:{ipEndPoint.Port}/api/Configuration"));
                    return await httpResponseMessage.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                this.MessageSlave = $"Servizio di automazione non raggiungibile host:{ipEndPoint?.ToString()}/Application/Configuration {ex.Message}";
            }

            return null;
        }

        private void Initialize()
        {
            this.RaisePropertyChanged(nameof(this.SoftwareVersion));

            if (IPAddress.TryParse(ConfigurationManager.AppSettings.GetInstallDefaultMasIpaddress(), out var ip))
            {
                this.MasIpAddress = ip;
            }

            this.IsMasConfiguration = true;
        }

        private void LoadMachineConfiguration(string configuration)
        {
            var jsonObject = JObject.Parse(configuration);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CommonUtils.Converters.IPAddressConverter());

            this.masConfiguration = JsonConvert.DeserializeObject<MAS.DataModels.VertimagConfiguration>(jsonObject.ToString(), settings);

            if (this.masConfiguration is null)
            {
                throw new Exception("Could not load configuration. File is empty.");
            }
        }

        private void Next()
        {
            this.installationService.SetConfiguration(this.masIpAddress, this.masConfiguration);

            this.installationService.SetStage(OperationStage.BaySelection);
        }

        private void RaiseCanExecuteChanged()
        {
            this.checkMasConfigurationCommand?.RaiseCanExecuteChanged();
            this.nextCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
