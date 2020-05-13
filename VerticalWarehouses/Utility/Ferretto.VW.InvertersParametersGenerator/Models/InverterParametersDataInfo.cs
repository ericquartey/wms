using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.InvertersParametersGenerator.Models
{
    public class InverterParametersDataInfo : InverterParametersData
    {
        #region Constructors

        public InverterParametersDataInfo(InverterType inverterType, byte inverterIndex, string description) : base(inverterIndex, description)
        {
            this.Type = inverterType;
        }

        #endregion

        #region Properties

        public InverterType Type { get; }

        #endregion
    }
}
