using System;
using System.Collections.Generic;

namespace Ferretto.VW.App.Services
{
    public interface IUsbWatcherService
    {
        #region Events

        event EventHandler<DrivesChangedEventArgs> DrivesChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the set of available USB drives.
        /// </summary>
        /// <remarks>
        /// An empty collection is returned if no USB drives are available.
        /// </remarks>
        IEnumerable<System.IO.DriveInfo> Drives { get; }

        #endregion

        #region Methods

        void Disable();

        void Enable();

        #endregion
    }
}
