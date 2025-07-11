﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IInverterParametersMessageData : IMessageData
    {
        #region Properties

        short Code { get; set; }

        int Datset { get; set; }

        bool IsReadMessage { get; set; }

        MessageType Type { get; set; }

        string Value { get; set; }

        #endregion
    }
}
