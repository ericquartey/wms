using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterProgrammingFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterParametersData InverterParametersData { get; set; }

        #endregion
    }
}
