using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.SocketLink
{
    internal enum ExtractCommandResponseCode
    {
        REQUEST_ACCEPTED = 0,

        TRAY_NUMBER_NOT_CORRECT = 1,

        TRAY_ALREDY_REQUESTED = 2,

        TRY_CONTAINED_IN_A_BLOCKED_SHELF_POSITION = 3,

        EXIT_BAY_NOT_CORRECT = 4
    }

    internal enum StoreCommandResponseCode
    {
        REQUEST_ACCEPTED = 0,

        NO_TRAY_CURRENTLY_PRESENT = 1,

        TRAY_ALREDY_REQUESTED = 2,

        BAY_NOT_CORRECT = 3
    }

    internal interface ISocketLinkSyncProvider
    {
        #region Methods

        string ProcessCommands(string buffer);

        #endregion
    }
}
