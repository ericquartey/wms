using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class SensorsChangedMessageData : ISensorsChangedMessageData
    {
        #region Properties

        public bool[] SensorsStates { get; set; }

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        #endregion
    }
}
