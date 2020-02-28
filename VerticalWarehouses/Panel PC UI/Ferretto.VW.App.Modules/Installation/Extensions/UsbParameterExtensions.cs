using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ferretto.VW.App
{
    public static class UsbParameterExtensions
    {
        #region Methods

        public static IEnumerable<FileInfo> FindConfigurationFiles(this DriveInfo drive)
        {
            try
            {
#if DEBUG
                return (drive ?? throw new ArgumentNullException(nameof(drive)))
                       .RootDirectory.GetFiles("*.json");
#else
                return (drive ?? throw new ArgumentNullException(nameof(drive)))
                       .RootDirectory.GetFiles("*.json", System.IO.SearchOption.AllDirectories);
#endif
            }
            catch
            {
                return Array.Empty<FileInfo>();
            }
        }

        public static IEnumerable<FileInfo> FindConfigurationFiles(this IEnumerable<DriveInfo> drives)
            => (drives ?? throw new ArgumentNullException(nameof(drives))).SelectMany(drive => drive.FindConfigurationFiles());

        public static IEnumerable<DriveInfo> Writable(this IEnumerable<DriveInfo> drives)
            => (drives ?? throw new ArgumentNullException(nameof(drives))).Where(drive => drive.AvailableFreeSpace > 0L);

        #endregion
    }
}
