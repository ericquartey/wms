using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IChangeRunningStateMessageData : IMessageData
    {
        #region Properties

        CommandAction CommandAction { get; }

        bool Enable { get; }

        Guid? MissionId { get; }

        StopRequestReason StopReason { get; }

        #endregion
    }
}
