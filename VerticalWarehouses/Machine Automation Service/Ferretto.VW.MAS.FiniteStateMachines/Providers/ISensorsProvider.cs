using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.Providers
{
    public interface ISensorsProvider
    {
        #region Methods

        bool[] GetAll();

        ShutterPosition GetShutterPosition(BayNumber bayNumber);

        #endregion
    }
}
