using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class MachineProvider : Interfaces.IMachineProvider
    {
        #region Fields

        private const int MaxDrawerGrossWeight = 990;

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

        public bool IsOneTonMachine()
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.StructuralProperties)
                .Single();

            var maximumLoadOnBoard = elevator.StructuralProperties.MaximumLoadOnBoard;
            return maximumLoadOnBoard == MaxDrawerGrossWeight;
        }

        #endregion
    }
}
