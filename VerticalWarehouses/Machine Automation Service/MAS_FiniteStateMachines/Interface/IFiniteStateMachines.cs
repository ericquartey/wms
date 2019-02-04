using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IFiniteStateMachines
    {
        #region Methods

        void Destroy();

        void DoHoming();

        void DoVerticalHoming();

        void MakeOperationByInverter(IdOperation code);

        #endregion Methods
    }
}
