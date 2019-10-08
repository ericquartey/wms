namespace Ferretto.VW.MAS.DataLayer
{
    public interface IVerticalOriginVolatileSetupStatusProvider
    {
        #region Methods

        void Complete();

        SetupStepStatus Get();

        #endregion
    }
}
