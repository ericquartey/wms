namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupStatusProvider
    {
        #region Methods

        void CompleteBeltBurnishing();

        void CompleteVerticalOffset();

        void CompleteVerticalOrigin();

        void CompleteVerticalResolution();

        SetupStatusCapabilities Get();

        void IncreaseBeltBurnishingCycle();

        void ResetBeltBurnishing();

        #endregion
    }
}
