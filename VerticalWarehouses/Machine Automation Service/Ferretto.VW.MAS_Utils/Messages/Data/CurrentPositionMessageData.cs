using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class CurrentPositionMessageData : IMessageData
    {
        #region Constructors

        public CurrentPositionMessageData(decimal currentPosition)
        {
            this.CurrentPosition = currentPosition;
        }

        #endregion

        #region Properties

        public decimal CurrentPosition { get; set; }

        public MessageVerbosity Verbosity => throw new NotImplementedException();

        #endregion
    }
}
