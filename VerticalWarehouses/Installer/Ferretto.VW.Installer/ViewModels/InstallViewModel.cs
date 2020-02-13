using System;
using System.Linq;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.ViewModels
{
    [POCOViewModel]
    public class InstallViewModel : IOperationResult, ISupportServices
    {        
        IServiceContainer serviceContainer = null;
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

        private RelayCommand openFileCommand;

        private bool isSuccessful;
        readonly InstallationService installationService;



        public virtual string Filter { get; set; }
        public virtual int FilterIndex { get; set; }
        public virtual string Title { get; set; }
        public virtual bool DialogResult { get; protected set; }
        public virtual string ResultFileName { get; protected set; }
        public virtual string FileBody { get; set; }


        #region SaveFileDialogService specific properties
        public virtual string DefaultExt { get; set; }
        public virtual string DefaultFileName { get; set; }
        public virtual bool OverwritePrompt { get; set; }

        public string SoftwareVersion => string.Format("Welcome to installation {0}", this.installationService?.SoftwareVersion);
        #endregion
        public string PanelPcMasVersion => string.Format("Panel pc ver.{0} Machine automation service ver.{1}", this.installationService?.PanelPcVersion, this.installationService?.MasVersion);     

        protected ISaveFileDialogService SaveFileDialogService { get { return this.ServiceContainer.GetService<ISaveFileDialogService>(); } }
        protected IOpenFileDialogService OpenFileDialogService { get { return this.ServiceContainer.GetService<IOpenFileDialogService>(); } }
        public ICommand OpenFileCommand =>
                        this.openFileCommand
                        ??
                        (this.openFileCommand = new RelayCommand(this.OpenFile, this.CanOpenFile));

        public bool IsSuccessful => this.isSuccessful;        

        private bool CanOpenFile()
        {
            return true;
        }

        public InstallViewModel(InstallationService installationService)
        {
            this.installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));

            this.Filter = "Conf. files (.json)|*.json";            
            this.FilterIndex = 1;
            this.Title = "Caricamento file di configurazione";
            this.DefaultExt = "json";
            this.DefaultFileName = "Configurazione";
            this.OverwritePrompt = true;
        }

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

    }
}
