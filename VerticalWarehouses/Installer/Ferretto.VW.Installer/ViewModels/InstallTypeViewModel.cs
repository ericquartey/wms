using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.MAS.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog.Time;

namespace Ferretto.VW.Installer.ViewModels
{
    [POCOViewModel]
    public class InstallTypeViewModel : Core.BindableBase, IOperationResult, ISupportServices
    {
        #region Fields

        private readonly InstallationService installationService;

        private bool canNext;

        private RelayCommand checkMasConfigurationCommand;        

        private bool isMasConfiguration;

        private bool isMasConfigurationValid;

        private bool isSlaveConfigurationValid;

        private bool isSuccessful;

        private VertimagConfiguration masConfiguration;

        private IPAddress masIpAddress;

        private IPEndPoint masIpEndpoint;

        private string messageMaster;

        private RelayCommand nextCommand;

        private RelayCommand openFileCommand;

        private IServiceContainer serviceContainer = null;

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

        public IServiceContainer ServiceContainer
        {
            get
            {
                if (this.serviceContainer == null)
                {
                    this.serviceContainer = new ServiceContainer(this);
                }

                return this.serviceContainer;
            }
        }

        public string SoftwareVersion => string.Format("Welcome to installation {0}", this.installationService?.SoftwareVersion);

        public virtual string Title { get; set; }

        protected IOpenFileDialogService OpenFileDialogService { get { return this.ServiceContainer.GetService<IOpenFileDialogService>(); } }

        protected ISaveFileDialogService SaveFileDialogService { get { return this.ServiceContainer.GetService<ISaveFileDialogService>(); } }

        #endregion

        #region Methods

        public async Task OpenFileAsync()
        {
            this.MessageMaster = string.Empty;
            this.OpenFileDialogService.Filter = this.Filter;
            this.OpenFileDialogService.FilterIndex = this.FilterIndex;
            this.DialogResult = this.OpenFileDialogService.ShowDialog();
            if (!this.DialogResult)
            {
                this.ResultFileName = string.Empty;
                this.isMasConfigurationValid = false;
            }
            else
            {

               
                var file = this.OpenFileDialogService.Files.First();
                string fileContents;
                using (var stream = file.OpenText())
                {
                    fileContents = stream.ReadToEnd();
                }

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

        private static void ValidateJson(JObject jsonObject)
        {            
            using (var streamReader = new StreamReader("../Machine Automation Service/configuration/schemas/vertimag-configuration-schema.json"))
            {
                using (var textReader = new JsonTextReader(streamReader))
                {
                    var schema = JSchema.Load(textReader);
                    jsonObject.Validate(schema);
                }
            }
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

            if (this.isMasConfiguration && this.isMasConfigurationValid)
            {
                this.canNext = true;
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
                this.MessageMaster = $"Servizio di automazione non raggiungibile host:{ipEndPoint?.ToString()}/Application/Configuration {ex.Message}";
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
                settings.Converters.Add(new IPAddressConverter());

                this.masConfiguration = JsonConvert.DeserializeObject<VertimagConfiguration>(jsonObject.ToString(), settings);
                this.masConfiguration.Validate();                

                return !(this.masConfiguration is null);
            }
            catch
            {
            }

            return false;
        }

        private void Next()
        {
            this.installationService.SetConfiguration(this.isMasConfiguration ? null : this.masIpAddress, this.masConfiguration);

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
