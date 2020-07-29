using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using NLog;

namespace Ferretto.VW.App.Services
{
    internal sealed class UsbWatcherService : IUsbWatcherService, IDisposable
    {
        #region Fields

        private readonly List<DriveInfo> _state = new List<DriveInfo>();

        private readonly ManagementEventWatcher eventWatcher;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private ReadOnlyCollection<DriveInfo> drives;

        private bool isDisposed;

        private bool isEnabled;

        #endregion

        #region Constructors

        public UsbWatcherService()
        {
            var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent");
            this.eventWatcher = new ManagementEventWatcher(query);
            this.drives = new ReadOnlyCollection<DriveInfo>(this._state);
        }

        #endregion

        #region Events

        public event EventHandler<DrivesChangedEventArgs> DrivesChanged;

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> Drives => this.drives ?? new ReadOnlyCollection<DriveInfo>(Array.Empty<DriveInfo>());

        #endregion

        #region Methods

        public void Disable()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(UsbWatcherService));
            }

            if (this.isEnabled)
            {
                this.eventWatcher.EventArrived -= this.Watcher_EventArrived;
                this.isEnabled = false;

                this.logger.Debug("USB watcher service disabled.");
            }
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.Disable();

                this.eventWatcher.Dispose();

                this.isDisposed = true;
                this.logger.Debug("USB watcher service disposed.");
            }
        }

        public void Enable()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(UsbWatcherService));
            }

            if (this.isEnabled)
            {
                return;
            }

            this.isEnabled = true;
            this.ReloadDriveInfos();

            this.eventWatcher.EventArrived += this.Watcher_EventArrived;
            this.eventWatcher.Start();

            this.logger.Debug("USB watcher service enabled.");
        }

        private void ReloadDriveInfos()
        {
            if (this.isDisposed || !this.isEnabled)
            {
                return;
            }

            var driveInfos = DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.Removable);
            var attached = driveInfos.Where(d => !this._state.Any(d0 => d.Name.Equals(d0.Name, StringComparison.OrdinalIgnoreCase))).ToArray();
            var detached = this._state.Where(d0 => !driveInfos.Any(d => d0.Name.Equals(d.Name, StringComparison.OrdinalIgnoreCase))).ToArray();

            if (attached.Any() || detached.Any())
            {
                this._state.Clear();
                this._state.AddRange(driveInfos);
                this.drives = new ReadOnlyCollection<DriveInfo>(this._state);

                this.logger.Debug($"USB watcher detected {attached.Length} added and {detached.Length} removed devices.");
                this.DrivesChanged?.Invoke(this, new DrivesChangedEventArgs(detached, attached));
            }
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (this.isDisposed || !this.isEnabled)
            {
                return;
            }

            this.ReloadDriveInfos();
        }

        #endregion
    }
}
