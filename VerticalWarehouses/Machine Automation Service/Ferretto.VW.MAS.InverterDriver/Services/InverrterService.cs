using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.InverterDriver.Interface.Services;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.InverterDriver.Services
{
    internal class InverterService : IInverterService
    {
        #region Fields

        private readonly Dictionary<InverterIndex, IInverterStatusBase> inverterStatuses;

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
                lock (this.inverterStatuses)
                {
                    var statuses = new List<IInverterStatusBase>();
                    foreach (var status in this.inverterStatuses.ToList())
                    {
                        statuses.Add((IInverterStatusBase)status.Value);
                    }
                    return statuses;
                    //return this.inverterStatuses.ToList().Select(i => i.Value.OfType<IInverterStatusBase>());
                }
            }
        }

        public int StatusesCount
        {
            get
            {
                lock (this.inverterStatuses)
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

            if (this.inverterStatuses.ContainsKey(inverterIndex))
            {
                return false;
            }

            this.inverterStatuses.Add(inverterIndex, inverterStatus);
            return true;
        }

        public IInverterStatusBase GetStatus(InverterIndex inverterIndex)
        {
            lock (this.inverterStatuses)
            {
                if (this.inverterStatuses.TryGetValue(inverterIndex, out var currentStatus))
                {
                    return currentStatus;
                }
            }

            return null;
        }

        public bool TryGetValue(InverterIndex inverterIndex, out IInverterStatusBase inverterStatuse)
        {
            inverterStatuse = null;

            if (this.inverterStatuses.TryGetValue(inverterIndex, out var statuses))
            {
                inverterStatuse = statuses;
                return true;
            }

            return false;
        }

        #endregion
    }
}
