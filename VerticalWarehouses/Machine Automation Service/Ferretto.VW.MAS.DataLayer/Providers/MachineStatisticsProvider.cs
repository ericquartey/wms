using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class MachineStatisticsProvider : Interfaces.IMachineStatisticsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public MachineStatisticsProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public MachineStatistics GetMachineStatistics()
        {
            return this.dataContext.MachineStatistics.FirstOrDefault();
        }

        #endregion
    }
}
