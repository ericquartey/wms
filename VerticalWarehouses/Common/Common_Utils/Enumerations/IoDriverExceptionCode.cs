using System;
using System.Collections.Generic;

namespace Ferretto.VW.Common_Utils.Enumerations
{
    public enum IoDriverExceptionCode
    {
        CreationFailure = 1,

        DeviceNotConnected,

        GetIpMasterFailed,

        IoClientCreationFailed,
    }
}
