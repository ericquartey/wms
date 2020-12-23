using System.Collections.Generic;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterReadingFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool IsCheckInverterVersion { get; }

        IEnumerable<object> Parameters { get; }

        #endregion
    }
}
