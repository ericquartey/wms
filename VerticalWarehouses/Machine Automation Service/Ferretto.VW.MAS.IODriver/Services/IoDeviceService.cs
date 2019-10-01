using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.IODriver.Interface.Services;

namespace Ferretto.VW.MAS.IODriver.Services
{
    internal class IoDeviceService : IIoDeviceService
    {
        #region Fields

        private readonly Dictionary<IoIndex, IoStatus> ioStatuses;

        private readonly object syncRoot = new object();

        #endregion

        #region Constructors

        public IoDeviceService()
        {
            this.ioStatuses = new Dictionary<IoIndex, IoStatus>();
        }

        #endregion

        #region Properties

        public IEnumerable<IoStatus> GetStatuses
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.ioStatuses.Values.ToList();
                }
            }
        }

        #endregion

        #region Methods

        public IoStatus AddIoStatus(IoIndex index)
        {
            lock (this.syncRoot)
            {
                if (this.ioStatuses.ContainsKey(index))
                {
                    return this.ioStatuses[index];
                }

                var ioStatus = new IoStatus(index);

                this.ioStatuses.Add(index, ioStatus);

                return ioStatus;
            }
        }

        #endregion
    }
}
