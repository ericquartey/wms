using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IPowerEnableMessageData : IMessageData
    {
        #region Properties

        CommandAction CommandAction { get; }

        List<BayNumber> ConfiguredBays { get; }

        bool Enable { get; }

        #endregion
    }
}
