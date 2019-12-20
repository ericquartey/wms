using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IBayProvider
    {
        #region Methods

        void LightOn(bool enable, BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
