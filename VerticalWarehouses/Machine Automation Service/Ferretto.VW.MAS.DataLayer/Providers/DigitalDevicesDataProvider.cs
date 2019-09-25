using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal sealed class DigitalDevicesDataProvider : IDigitalDevicesDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public DigitalDevicesDataProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new System.ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public IEnumerable<Inverter> GetAllInverters()
        {
            return this.dataContext.Inverters.ToArray();
        }

        public IEnumerable<IoDevice> GetAllIoDevices()
        {
            return this.dataContext.IoDevices.ToArray();
        }

        public Inverter GetInverterByIndex(InverterIndex index)
        {
            var inverter = this.dataContext.Inverters.SingleOrDefault(i => i.Index == index);
            if (inverter is null)
            {
                throw new EntityNotFoundException((int)index);
            }

            return inverter;
        }

        #endregion
    }
}
