﻿namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IPowerEnableFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool Enable { get; }

        #endregion
    }
}
