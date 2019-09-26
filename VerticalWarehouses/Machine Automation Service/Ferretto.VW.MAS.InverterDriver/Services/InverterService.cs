using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.Services;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.InverterDriver.Services
{
    internal class InverterService : IInverterService
    {
        #region Fields

        private readonly Dictionary<InverterIndex, IInverterStatusBase> inverterStatuses;

        private readonly object syncRoot = new object();

        #endregion

        #region Constructors

        public InverterService()
        {
            this.inverterStatuses = new Dictionary<InverterIndex, IInverterStatusBase>();
        }

        #endregion

        #region Properties

        public IEnumerable<IInverterStatusBase> GetStatuses
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.inverterStatuses.Values.ToList();
                }
            }
        }

        public int StatusesCount
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.inverterStatuses.Count;
                }
            }
        }

        #endregion

        #region Methods

        public bool AddStatus(InverterIndex inverterIndex, IInverterStatusBase inverterStatus)
        {
            if (inverterIndex == InverterIndex.None)
            {
                return false;
            }

            lock (this.syncRoot)
            {
                if (this.inverterStatuses.ContainsKey(inverterIndex))
                {
                    return false;
                }

                this.inverterStatuses.Add(inverterIndex, inverterStatus);
            }

            return true;
        }

        public bool TryGetValue(InverterIndex inverterIndex, out IInverterStatusBase inverterStatuse)
        {
            inverterStatuse = null;

            lock (this.syncRoot)
            {
                if (this.inverterStatuses.TryGetValue(inverterIndex, out var statuses))
                {
                    inverterStatuse = statuses;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
