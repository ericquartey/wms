using System;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupStatusProvider
    {
        #region Methods

        void CompleteVerticalOrigin();

        SetupStatusCapabilities Get();

        #endregion
    }
}
