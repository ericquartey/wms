using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.IODriver.Interface.Services;
using Ferretto.VW.MAS.IODriver.IoDevices.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.MAS.IODriver.Services
{
    internal class IoDeviceService : IIoDeviceService
    {
        #region Fields

        private readonly Dictionary<IoIndex, IoStatus> ioStatuses;

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
                lock (this.ioStatuses)
                {
                    return this.ioStatuses.Values.ToList();
                }
            }
        }

        #endregion

        #region Methods

        public void AddIoStatus(IoIndex index)
        {
            lock (this.ioStatuses)
            {
                this.ioStatuses.Add(index, new IoStatus(index));
            }
        }

        public IoStatus GetStatus(IoIndex deviceIndex)
        {
            lock (this.ioStatuses)
            {
                if (this.ioStatuses.TryGetValue(deviceIndex, out var currentStatus))
                {
                    return currentStatus;
                }
            }
            return null;
        }

        public bool UpdateInputStates(bool[] inputData, IoIndex deviceIndex)
        {
            lock (this.ioStatuses)
            {
                if (this.ioStatuses.ContainsKey(deviceIndex))
                {
                    this.ioStatuses[deviceIndex].UpdateInputStates(inputData);
                    return true;
                }
            }

            return false;
        }

        public bool UpdateOutputStates(bool[] outputData, IoIndex deviceIndex)
        {
            lock (this.ioStatuses)
            {
                if (this.ioStatuses.ContainsKey(deviceIndex))
                {
                    this.ioStatuses[deviceIndex].UpdateOutputStates(outputData);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
