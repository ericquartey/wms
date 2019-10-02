using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.IODriver.Interface.Services;

namespace Ferretto.VW.MAS.IODriver.Services
{
    internal class IoDeviceService : IIoDeviceService
    {
        #region Constructors

        public IoDeviceService()
        {
            var ioStatusList = new List<IoStatus>();
            foreach (var ioStatusIndex in System.Enum.GetValues(typeof(IoIndex)))
            {
                ioStatusList.Add(new IoStatus((IoIndex)ioStatusIndex));
            }

            this.IoStatuses = ioStatusList.ToArray();
        }

        #endregion

        #region Properties

        public IEnumerable<IoStatus> IoStatuses { get; }

        #endregion
    }
}
