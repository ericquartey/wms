using System;

namespace Ferretto.VW.Installer.Core
{
    public class InstallationFinishedEventArgs : EventArgs
    {
        #region Constructors

        public InstallationFinishedEventArgs(bool success)
        {
            this.Success = success;
        }

        #endregion

        #region Properties

        public bool Success { get; }

        #endregion
    }
}
