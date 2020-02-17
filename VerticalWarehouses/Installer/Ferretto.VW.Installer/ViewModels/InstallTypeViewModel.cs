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

        private bool isMasConfiguration;

        private bool isMasConfigurationValid;

        private bool isSlaveConfigurationValid;

        private bool isSuccessful;

        private VertimagConfiguration masConfiguration;

        private IPAddress masIpAddress;

        private string message;

        private RelayCommand nextCommand;

        private RelayCommand openFileCommand;

        private IServiceContainer serviceContainer = null;

        #endregion

        #region Constructors

        public InstallTypeViewModel(InstallationService installationService)
        {
            this.installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));

            this.Inistialize();
        }

        #endregion

        #region Properties

        public virtual string DefaultExt { get; set; }

        public virtual string DefaultFileName { get; set; }

        public virtual bool DialogResult { get; protected set; }

        public virtual string Filter { get; set; }

        public virtual int FilterIndex { get; set; }

        public bool IsMasConfiguration
        {
            get => this.isMasConfiguration;
            set => this.SetProperty(ref this.isMasConfiguration, value, this.EvaluateCanNext);
        }

        public bool IsSuccessful => this.isSuccessful;

        public IPAddress MasIpAddress
        {
            get => this.masIpAddress;
            set => this.SetProperty(ref this.masIpAddress, value, this.EvaluateCanNext);
        }

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        public ICommand NextCommand =>
                        this.nextCommand
                ??
                (this.nextCommand = new RelayCommand(this.Next, this.CanNext));

        public ICommand OpenFileCommand =>
                                this.openFileCommand
                        ??
                        (this.openFileCommand = new RelayCommand(this.OpenFile, this.CanOpenFile));

        public virtual bool OverwritePrompt { get; set; }

        public string PanelPcMasVersion => string.Format("Panel pc ver.{0} Machine automation service ver.{1}", this.installationService?.PanelPcVersion, this.installationService?.MasVersion);

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

        public void OpenFile()
        {
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
                this.LoadConfiguration(file);
            }

            this.EvaluateCanNext();
        }

        public void Save()
        {
            this.isSuccessful = true;
        }

        private static void ValidateJson(JObject jsonObject)
        {
            using (var streamReader = new StreamReader("./configuration/schemas/vertimag-configuration-schema.json"))
            {
                using (var textReader = new JsonTextReader(streamReader))
                {
                    var schema = JSchema.Load(textReader);
                    jsonObject.Validate(schema);
                }
            }
        }

        private bool CanNext()
        {
            return this.canNext;
        }

        private bool CanOpenFile()
        {
            return true;
        }

        private bool CheckMasHost()
        {
            IPAddress ip = null;
            int? ipPort = null;
            try
            {
                if (int.TryParse(ConfigurationManager.AppSettings.GetInstallDefaultMasIpport(), out var port))
                {
                    ipPort = port;
                    var ipEndpoint = new IPEndPoint(this.masIpAddress, port);
                    using (var client = new TcpClient(ipEndpoint))
                    {
                        return true;
                    }
                }
            }
            catch (SocketException ex)
            {
                this.Message = $"Servizio di automazione non raggiungibile host:{ip?.ToString()}:{ipPort} {ex.Message}";
            }

            return false;
        }

        private void EvaluateCanNext()
        {
            this.canNext = false;

            if (this.isMasConfiguration && this.isMasConfigurationValid)
            {
                this.canNext = true;
            }

            if (!this.isMasConfiguration)
            {
                this.isSlaveConfigurationValid = (this.masIpAddress is null);
                if (!(this.masIpAddress is null))
                {
                    this.isSlaveConfigurationValid = this.CheckMasHost();
                }
                this.canNext = this.isSlaveConfigurationValid;
            }

            this.RaiseCanExecuteChanged();
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

        private void LoadConfiguration(IFileInfo configurationFilePath)
        {
            try
            {
                string fileContents;
                using (var stream = configurationFilePath.OpenText())
                {
                    fileContents = stream.ReadToEnd();
                }

                var jsonObject = JObject.Parse(fileContents);

                ValidateJson(jsonObject);

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new IPAddressConverter());

                this.masConfiguration = JsonConvert.DeserializeObject<VertimagConfiguration>(jsonObject.ToString(), settings);
                this.masConfiguration.Validate();

                this.isMasConfigurationValid = !(this.masConfiguration is null);
            }
            catch
            {
                this.isMasConfigurationValid = false;
            }
        }

        private void Next()
        {
            this.installationService.SetConfiguration(this.isMasConfiguration ? null : this.masIpAddress, this.masConfiguration);

            this.installationService.SetOperation(OperationMode.InstallBay);
        }

        private void RaiseCanExecuteChanged()
        {
            this.nextCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
