using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class BaseParametersImportExportViewModel : BaseMainViewModel
    {
        #region Fields

        private const string CONFIGURATIONNAME = "vertimag-configuration.{0}.json";

        private const string DEFAULTDEVICENAME = "D";

        private const string DEVICE = @"{0}:\";

        private const int SECSUPDATEDEVICESSTATUSINTERVAL = 3;

        private readonly IBayManager bayManager;

        private DispatcherTimer devicesStatusUpdateTimer;

        private string existingPath;

        private string fullPath;

        private bool isBusy;

        private bool isDeviceReady;

        #endregion

        #region Constructors

        public BaseParametersImportExportViewModel(IBayManager bayManager)
            : base(Services.PresentationMode.Installer)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public string ExistingPath
        {
            get => this.existingPath;
            set => this.SetProperty(ref this.existingPath, value);
        }

        public string FileName => string.Format(CONFIGURATIONNAME, this.bayManager.Identity.SerialNumber);

        public string FullPath
        {
            get => this.fullPath;
            set => this.SetProperty(ref this.fullPath, value);
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

        public bool IsDeviceReady
        {
            get => this.isDeviceReady;
            set => this.SetProperty(ref this.isDeviceReady, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.devicesStatusUpdateTimer?.Stop();
            this.devicesStatusUpdateTimer = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.IsBusy = true;

                this.RefreshDevicesStatus();

                this.StartMonitorDrive();
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

        private void DevicesStatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            this.RefreshDevicesStatus();
        }

        private void RefreshDevicesStatus()
        {
            var deviceName = string.Format(DEVICE, DEFAULTDEVICENAME);

            if (DriveInfo.GetDrives()
                .Select(d => d.Name)
                .FirstOrDefault(n => n.Equals(deviceName)) is string deviceFound)
            {
                var pathToCheck = $"{deviceFound}{this.FileName}";
                this.FullPath = pathToCheck;
                this.ExistingPath = File.Exists(pathToCheck) ? pathToCheck : null;
                this.isDeviceReady = true;
            }
            else
            {
                this.FullPath = null;
                this.ExistingPath = null;
                this.isDeviceReady = false;
            }

            this.RaisePropertyChanged();
        }

        private void StartMonitorDrive()
        {
            this.devicesStatusUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            this.devicesStatusUpdateTimer.Tick += new EventHandler(this.DevicesStatusUpdateTimer_Tick);
            this.devicesStatusUpdateTimer.Interval = new TimeSpan(0, 0, SECSUPDATEDEVICESSTATUSINTERVAL);
            this.devicesStatusUpdateTimer.Start();
        }

        #endregion
    }
}
