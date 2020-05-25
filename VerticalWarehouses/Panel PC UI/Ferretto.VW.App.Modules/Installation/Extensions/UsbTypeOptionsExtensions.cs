using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ferretto.VW.App
{
    public static class UsbTypeOptionsExtensions
    {
        #region Methods

        public static IEnumerable<FileInfo> FindFiles(this DriveInfo drive, string searchPattern)
        {
            try
            {
                return (drive ?? throw new ArgumentNullException(nameof(drive)))
                       .RootDirectory.GetFiles(searchPattern);
            }
            catch
            {
                return Array.Empty<FileInfo>();
            }
        }

        public static IEnumerable<string> FindFiles(this IEnumerable<DriveInfo> drives, string searchPattern)
            => (drives ?? throw new ArgumentNullException(nameof(drives))).SelectMany(drive => drive.FindFiles(searchPattern))?.Select(f => f.FullName);

        #endregion
    }
}
