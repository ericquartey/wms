using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class MachineStatisticsProvider : Interfaces.IMachineStatisticsProvider
    {
        #region Fields

        private readonly DataLayerContext dataLayerContext;

        #endregion

        #region Constructors

        public MachineStatisticsProvider(DataLayerContext dataLayerContext)
        {
            if (dataLayerContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataLayerContext));
            }

            this.dataLayerContext = dataLayerContext;
        }

        #endregion

        #region Methods

        public MachineStatistics GetMachineStatistics()
        {
            return this.dataLayerContext.MachineStatistics.FirstOrDefault();
        }

        #endregion
    }
}
