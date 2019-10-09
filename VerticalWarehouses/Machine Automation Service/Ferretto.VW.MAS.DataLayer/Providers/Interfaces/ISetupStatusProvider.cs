using System;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupStatusProvider
    {
        #region Methods

        [Obsolete]
        void CompleteVerticalOffset();

        [Obsolete]
        void CompleteVerticalOrigin();

        [Obsolete]
        void CompleteVerticalResolution();

        SetupStatusCapabilities Get();

        #endregion
    }
}
