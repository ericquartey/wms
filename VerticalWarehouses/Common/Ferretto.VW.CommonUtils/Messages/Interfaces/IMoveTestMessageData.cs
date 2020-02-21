using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMoveTestMessageData : IMessageData
    {
        #region Properties

        int ExecutedCycles { get; set; }

        List<int> LoadUnitsToTest { get; set; }

        int RequiredCycles { get; set; }

        #endregion
    }
}
