using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public interface IInverterReadingMessageData : IMessageData
    {
        #region Properties

        IEnumerable<InverterParametersData> InverterParametersData { get; set; }

        #endregion
    }
}
