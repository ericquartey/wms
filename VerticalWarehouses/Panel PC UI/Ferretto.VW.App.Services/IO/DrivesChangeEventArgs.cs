using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Ferretto.VW.App.Services.IO
{
    public class DrivesChangeEventArgs : EventArgs
    {
        #region Constructors

        internal DrivesChangeEventArgs(IEnumerable<DriveInfo> detached, IEnumerable<DriveInfo> attached)
        {
            this.Detached = new ReadOnlyCollection<DriveInfo>(AmendList(detached));
            this.Attached = new ReadOnlyCollection<DriveInfo>(AmendList(attached));
        }

        #endregion

        #region Properties

        public ReadOnlyCollection<DriveInfo> Attached { get; }

        public ReadOnlyCollection<DriveInfo> Detached { get; }

        #endregion

        #region Methods

        private static IList<T> AmendList<T>(IEnumerable<T> items)
                                    => (items ?? Array.Empty<T>()).ToList();

        #endregion
    }
}
