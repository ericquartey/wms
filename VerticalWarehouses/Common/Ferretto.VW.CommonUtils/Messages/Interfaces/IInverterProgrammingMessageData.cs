using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IInverterProgrammingMessageData : IMessageData
    {
        #region Properties

        IEnumerable<InverterParametersData> InverterParametersData { get; set; }

        #endregion
    }
}
