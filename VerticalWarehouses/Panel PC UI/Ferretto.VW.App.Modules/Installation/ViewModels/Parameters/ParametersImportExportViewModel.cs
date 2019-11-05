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
    public class Folder
    {
        #region Properties

        public string FullPath { get; set; }

        #endregion
    }

    public class ParametersImportExportViewModel : BaseMainViewModel
    {
        #region Fields

        private DispatcherTimer devicesFolderUpdateTimer;

        private ObservableCollection<Folder> folders;

        private bool isBusy;

        private DelegateCommand restoreCommand;

        private DelegateCommand saveCommand;

        private string workingFolder;

        #endregion

        #region Constructors

        public ParametersImportExportViewModel() : base(Services.PresentationMode.Installer)
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
                    ((DelegateCommand)this.saveCommand).RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public ICommand RestoreCommand =>
            this.saveCommand
                    ??
                    (this.restoreCommand = new DelegateCommand(
                    async () => await this.RestoreAsync(), this.CanRestore));

        public ICommand SaveCommand =>
                    this.saveCommand
                   ??
                   (this.saveCommand = new DelegateCommand(
                       async () => await this.SaveAsync(), this.CanSave));

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

        private bool CanRestore()
        {
            return !this.IsBusy;
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.Folders));
        }

        private async Task RestoreAsync()
        {
            try
            {
                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                // TO DO save configuration
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
                this.IsBackNavigationAllowed = true;
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                // TO DO restore configuration
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
                this.IsBackNavigationAllowed = true;
            }
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
}
