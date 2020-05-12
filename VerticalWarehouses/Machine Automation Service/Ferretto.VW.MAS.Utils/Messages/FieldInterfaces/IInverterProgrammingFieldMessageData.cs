using System.Collections.Generic;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterProgrammingFieldMessageData : IFieldMessageData
    {
        #region Properties

        IEnumerable<object> Parameters { get; }

        #endregion
    }
}
