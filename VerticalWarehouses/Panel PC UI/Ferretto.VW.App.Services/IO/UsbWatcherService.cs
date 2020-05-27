using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;

namespace Ferretto.VW.App.Services.IO
{
    public class UsbWatcherService : IDisposable
    {
        #region Fields

        private readonly List<DriveInfo> _state = new List<DriveInfo>();

        private readonly ManagementEventWatcher _watcher;

        private ReadOnlyCollection<DriveInfo> _driveInfos = null;

        #endregion

        #region Constructors

        public UsbWatcherService()
        {
            var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent");
            this._watcher = new ManagementEventWatcher(query);
            this._driveInfos = new ReadOnlyCollection<DriveInfo>(this._state);
        }

        #endregion

        #region Events

        public event EventHandler<DrivesChangeEventArgs> DrivesChange;

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> Drives => this._driveInfos;

        #endregion

        #region Methods

        public void Dispose()
        {
            this._watcher.EventArrived -= this.Watcher_EventArrived;
            this._watcher.Dispose();
        }

        public void Start()
        {
            this.ReloadDriveInfos();

            this._watcher.EventArrived += this.Watcher_EventArrived;
            this._watcher.Start();
        }

        private void OnUsbDrivesChanged(IEnumerable<DriveInfo> removed, IEnumerable<DriveInfo> added)
        {
            this.DrivesChange?.Invoke(this, new DrivesChangeEventArgs(removed, added));
        }

        private void ReloadDriveInfos()
        {
            var driveInfos = DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.Removable);
            var attached = driveInfos.Where(d => !this._state.Any(d0 => d.Name.Equals(d0.Name, StringComparison.OrdinalIgnoreCase))).ToList();
            var detached = this._state.Where(d0 => !driveInfos.Any(d => d0.Name.Equals(d.Name, StringComparison.OrdinalIgnoreCase))).ToList();

            if (attached.Any() || detached.Any())
            {
                this._state.Clear();
                this._state.AddRange(driveInfos);
                this._driveInfos = new ReadOnlyCollection<DriveInfo>(this._state);
                this.OnUsbDrivesChanged(detached, attached);
            }
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            this.ReloadDriveInfos();
        }

        #endregion
    }
}
