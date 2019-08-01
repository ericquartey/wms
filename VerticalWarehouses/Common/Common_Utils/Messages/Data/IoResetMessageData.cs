﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ResetSecurityMessageData : IMessageData
    {
        #region Properties

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        #endregion
    }
}
