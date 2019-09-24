using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class MachineProvider : Interfaces.IMachineProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public MachineProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public Machine Get()
        {
            return this.dataContext.Machines.Single();
        }

        public MachineStatistics GetStatistics()
        {
            return this.dataContext.MachineStatistics.FirstOrDefault();
        }

        #endregion
    }
}
