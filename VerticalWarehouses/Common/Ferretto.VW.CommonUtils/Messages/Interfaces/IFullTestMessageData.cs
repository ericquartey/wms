using System.Collections.Generic;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IFullTestMessageData : IMessageData
    {
        #region Properties

        CommandAction CommandAction { get; }

        int CyclesDone { get; set; }

        int CyclesTodo { get; }

        List<int> LoadUnitIds { get; }

        #endregion
    }
}
