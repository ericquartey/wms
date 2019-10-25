using System;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupStatusProvider
    {
        #region Methods

        [Obsolete]
        void CompleteVerticalOrigin();

        SetupStatusCapabilities Get();

        #endregion
    }
}
