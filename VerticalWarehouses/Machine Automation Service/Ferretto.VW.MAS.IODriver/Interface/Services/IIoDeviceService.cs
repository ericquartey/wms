using System.Collections.Generic;
using Ferretto.VW.MAS.IODriver.IoDevices.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.IODriver.Interface.Services
{
    public interface IIoDeviceService
    {
        #region Properties

        IEnumerable<IoStatus> GetStatuses { get; }

        #endregion

        #region Methods

        void AddIoStatus(IoIndex index);

        IoStatus GetStatus(IoIndex deviceIndex);

        bool UpdateInputStates(bool[] inputData, IoIndex deviceIndex);

        bool UpdateOutputStates(bool[] outputData, IoIndex deviceIndex);

        #endregion
    }
}
