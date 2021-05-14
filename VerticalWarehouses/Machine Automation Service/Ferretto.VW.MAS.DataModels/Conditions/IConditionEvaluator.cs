using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public interface IConditionEvaluator
    {
        #region Methods

        bool IsSatisfied(BayNumber bayNumber);

        #endregion
    }
}
