using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterCurrentErrorFieldMessageData : FieldMessageData, IInverterCurrentErrorFieldMessageData
    {
        #region Constructors

        public InverterCurrentErrorFieldMessageData(MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
        }

        #endregion
    }
}
