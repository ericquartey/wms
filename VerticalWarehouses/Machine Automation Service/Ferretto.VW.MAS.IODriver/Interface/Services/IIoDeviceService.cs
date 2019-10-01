using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.IODriver.Interface.Services
{
    public interface IIoDeviceService
    {
        #region Properties

        IEnumerable<IoStatus> GetStatuses { get; }

        #endregion

        #region Methods

        IoStatus AddIoStatus(IoIndex index);

        #endregion
    }
}
