using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Ferretto.VW.App.Controls;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class BaseParametersImportExportViewModel : BaseMainViewModel
    {
        #region Fields

        private DispatcherTimer devicesFolderUpdateTimer;

        private ObservableCollection<Folder> folders;

        private bool isBusy;

        private string workingFolder;

        #endregion

        #region Constructors

        public BaseParametersImportExportViewModel() : base(Services.PresentationMode.Installer)
        {
        }

        #endregion

        #region Properties

        public ObservableCollection<Folder> Folders
        {
            get
            {
                this.folders = new ObservableCollection<Folder>();

                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (drive.DriveType == DriveType.Fixed)
                    {
                        var newFolder = new Folder();
                        newFolder.FullPath = drive.Name;
                        this.folders.Add(newFolder);
                    }
                }

                return this.folders;
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaisePropertyChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public string WorkingFolder
        {
            get => this.workingFolder;
            set => this.SetProperty(ref this.workingFolder, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.devicesFolderUpdateTimer.Stop();
            this.devicesFolderUpdateTimer = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.IsBusy = true;

                this.StartMonitorDrive();

                this.IsBackNavigationAllowed = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public virtual void RaisePropertyChanged()
        {
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.Folders));
        }

        private void StartMonitorDrive()
        {
            this.devicesFolderUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            this.devicesFolderUpdateTimer.Tick += new EventHandler(this.dispatcherTimer_Tick);
            this.devicesFolderUpdateTimer.Interval = new TimeSpan(0, 5, 0);
            this.devicesFolderUpdateTimer.Start();
        }

        #endregion
    }

    public class Folder
    {
        #region Properties

        public string FullPath { get; set; }

        #endregion
    }
}
