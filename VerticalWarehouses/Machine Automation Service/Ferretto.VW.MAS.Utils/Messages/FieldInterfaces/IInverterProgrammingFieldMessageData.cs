using System.Collections.Generic;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterProgrammingFieldMessageData : IFieldMessageData
    {
        #region Properties

        byte InverterIndex { get; }

        bool IsCheckInverterVersion { get; }

        IEnumerable<object> Parameters { get; }

        #endregion
    }
}
