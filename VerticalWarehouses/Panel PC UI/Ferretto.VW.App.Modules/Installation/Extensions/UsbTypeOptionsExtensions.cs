using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace Ferretto.VW.App
{
    public static class UsbTypeOptionsExtensions
    {
        #region Methods

        public static IEnumerable<FileInfo> FindFiles(this DriveInfo drive, string searchPattern)
        {
            if (drive is null)
            {
                return Array.Empty<FileInfo>();
            }

            try
            {
                return drive.RootDirectory.GetFiles(searchPattern);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);

                return Array.Empty<FileInfo>();
            }
        }

        public static IEnumerable<string> FindFiles(this IEnumerable<DriveInfo> drives, string searchPattern)
        {
            if (drives is null)
            {
                return Array.Empty<string>();
            }

            return drives
                .SelectMany(drive => drive.FindFiles(searchPattern))?
                .Select(f => f.FullName);
        }

        #endregion
    }
}
