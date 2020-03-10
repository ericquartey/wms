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
using Newtonsoft.Json.Schema;

namespace Ferretto.VW.Installer.ViewModels
{
    public class InstallTypeViewModel : Core.BindableBase, IOperationResult    
    {
        #region Fields

        private readonly InstallationService installationService;

        private bool canNext;

        private RelayCommand checkMasConfigurationCommand;        

        private bool isMasConfiguration;

        private bool isMasConfigurationValid;

        private bool isSlaveConfigurationValid;

        private bool isSuccessful;

        private MAS.DataModels.VertimagConfiguration masConfiguration;

        private IPAddress masIpAddress;

        private IPEndPoint masIpEndpoint;

        private string messageMaster;

        private RelayCommand nextCommand;

        private RelayCommand openFileCommand;        

        private string messageSlave;

        #endregion

        #region Constructors

        public InstallTypeViewModel(InstallationService installationService)
        {
            this.installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));

            this.Inistialize();
        }

        #endregion

        #region Properties

        public ICommand CheckMasConfigurationCommand =>
                                this.checkMasConfigurationCommand
                                ??
                                (this.checkMasConfigurationCommand = new RelayCommand(async () => await this.EvaluateCanNextAsync(), this.CanCheckMasConfiguration));

        public virtual string DefaultExt { get; set; }

        public virtual string DefaultFileName { get; set; }

        public virtual bool DialogResult { get; protected set; }

        public virtual string Filter { get; set; }

        public virtual int FilterIndex { get; set; }

        public bool IsMasConfiguration
        {
            get => this.isMasConfiguration;
            set => this.SetProperty(ref this.isMasConfiguration, value, async () => await this.EvaluateCanNextAsync());
        }

        public bool IsSuccessful => this.isSuccessful;

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
                        (this.openFileCommand = new RelayCommand(async () => await this.OpenFileAsync(), this.CanOpenFile));

        public virtual bool OverwritePrompt { get; set; }

        public string MasVersion => $"Machine automation service ver.{this.installationService?.MasVersion}";

        public string PanelPcVersion => $"Panel pc ver.{this.installationService?.PanelPcVersion}";

        public virtual string ResultFileName { get; protected set; }

        public string SoftwareVersion => string.Format("Welcome to installation {0}", this.installationService?.SoftwareVersion);

        public virtual string Title { get; set; }

        //protected IOpenFileDialogService OpenFileDialogService { get { return this.ServiceContainer.GetService<IOpenFileDialogService>(); } }

        //protected ISaveFileDialogService SaveFileDialogService { get { return this.ServiceContainer.GetService<ISaveFileDialogService>(); } }

        #endregion

        #region Methods

        public async Task OpenFileAsync()
        {
            string[] resultFiles = DialogService.BrowseFile("Scegli file di configurazione", string.Empty, "json", "File di configuirazione");

            if ((resultFiles?.Length == 1) == false)
            {
                this.ResultFileName = string.Empty;
                this.isMasConfigurationValid = false;
            }
            else
            {
                var file = resultFiles.First();

                var sr = new StreamReader(file);
                var fileContents = sr.ReadToEnd();
             
                this.isMasConfigurationValid = this.LoadConfiguration(fileContents);

                if (this.isMasConfigurationValid)
                {
                    var configurationFilePath = $"{ConfigurationManager.AppSettings.GetUpdateTempPath()}\\{ConfigurationManager.AppSettings.GetInstallMasPath()}\\Configuration\\vertimag-configuration.json";
                    this.installationService.SaveVertimagConfiguration(configurationFilePath, fileContents);
                }
            }

            await this.EvaluateCanNextAsync();
        }



        public void Save()
        {
            this.isSuccessful = true;
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
            int ? ipPort = null;
            try
            {
                if (int.TryParse(ConfigurationManager.AppSettings.GetInstallDefaultMasIpport(), out var port))
                {
                    ipPort = port;
                    var ipEndpoint = new IPEndPoint(this.masIpAddress, port);
                    using (var client = new TcpClient(ipEndpoint))
                    {
                        return ipEndpoint;
                    }
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
                        this.isSlaveConfigurationValid = this.LoadConfiguration(masConfiguration);
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

        private void Inistialize()
        {
            this.Filter = "Conf. files (.json)|*.json";
            this.FilterIndex = 1;
            this.Title = "Caricamento file di configurazione";
            this.DefaultExt = "json";
            this.DefaultFileName = "Configurazione";
            this.OverwritePrompt = true;

            this.RaisePropertyChanged(nameof(this.SoftwareVersion));

            if (IPAddress.TryParse(ConfigurationManager.AppSettings.GetInstallDefaultMasIpaddress(), out var ip))
            {
                this.MasIpAddress = ip;
            }

            this.IsMasConfiguration = true;
        }

        private bool LoadConfiguration(string configuration)
        {
            try
            {
                var jsonObject = JObject.Parse(configuration);

                // ValidateJson(jsonObject);

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new CommonUtils.Converters.IPAddressConverter());

                this.masConfiguration = JsonConvert.DeserializeObject<MAS.DataModels.VertimagConfiguration>(jsonObject.ToString(), settings);
                //this.masConfiguration.Validate();                

                return !(this.masConfiguration is null);
            }
            catch
            {
            }

            return false;
        }

        private void Next()
        {
            this.installationService.SetConfiguration(this.isMasConfiguration ? this.masIpAddress : null, this.masConfiguration);

            this.installationService.SetOperation(OperationMode.InstallBay);
        }

        private void RaiseCanExecuteChanged()
        {
            this.checkMasConfigurationCommand?.RaiseCanExecuteChanged();
            this.nextCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
