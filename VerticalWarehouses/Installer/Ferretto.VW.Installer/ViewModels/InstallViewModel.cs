using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.ViewModels
{
    [POCOViewModel]
    public class InstallViewModel : Core.BindableBase, IOperationResult, ISupportServices
    {
        #region Fields

        private readonly InstallationService installationService;

        private bool isSuccessful;

        private IPAddress masIpAddress;

        private RelayCommand openFileCommand;

        private IServiceContainer serviceContainer = null;

        #endregion

        #region Constructors

        public InstallViewModel(InstallationService installationService)
        {
            this.installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));

            this.Inistialize();
        }

        #endregion

        #region Properties

        public virtual string DefaultExt { get; set; }

        public virtual string DefaultFileName { get; set; }

        public virtual bool DialogResult { get; protected set; }

        public virtual string FileBody { get; set; }

        public virtual string Filter { get; set; }

        public virtual int FilterIndex { get; set; }

        public bool IsSuccessful => this.isSuccessful;

        public IPAddress MasIpAddress
        {
            get => this.masIpAddress;
            set => this.SetProperty(ref this.masIpAddress, value);
        }

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
            }
            else
            {
                var file = this.OpenFileDialogService.Files.First();
                this.ResultFileName = file.Name;
                using (var stream = file.OpenText())
                {
                    this.FileBody = stream.ReadToEnd();
                }
            }
        }

        public void Save()
        {
            this.isSuccessful = true;
        }

        private bool CanOpenFile()
        {
            return true;
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
        }

        #endregion
    }
}
