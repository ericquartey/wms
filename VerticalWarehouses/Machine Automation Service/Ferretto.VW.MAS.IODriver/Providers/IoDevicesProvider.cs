using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.IODriver
{
    internal sealed class IoDevicesProvider : IIoDevicesProvider
    {
        #region Constructors

        public IoDevicesProvider()
        {
            var ioStatusList = new List<IoStatus>();
            foreach (var ioStatusIndex in System.Enum.GetValues(typeof(IoIndex)))
            {
                ioStatusList.Add(new IoStatus((IoIndex)ioStatusIndex));
            }

            this.Devices = ioStatusList.ToArray();
        }

        #endregion

        #region Properties

        public IEnumerable<IoStatus> Devices { get; }

        #endregion
    }
}
