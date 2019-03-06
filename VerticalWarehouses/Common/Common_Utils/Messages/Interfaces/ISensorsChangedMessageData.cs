using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface ISensorsChangedMessageData : IMessageData
    {
        #region Properties

        bool[] SensorsStates { get; set; }

        #endregion
    }
}
