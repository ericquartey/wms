﻿using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;

namespace Ferretto.VW.MAS.AutomationService.Interfaces
{
    public interface IIoDeviceProvider
    {
        #region Properties

        IEnumerable<IoDeviceInfo> GetStatuses { get; }

        #endregion
    }
}
