using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Drivers;

namespace Ferretto.WMS.Scheduler
{
    public class Engine : IEngine
    {
        #region Fields

        private readonly DatabaseContext databaseContext;
        private readonly IEnumerable<IMachineScheduler> machines = new List<IMachineScheduler>();

        #endregion Fields

        #region Constructors

        public Engine(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        #endregion Constructors

        #region Methods

        public void SetupMachines()
        {
            this.databaseContext.Machines.Select(m => m);
        }

        #endregion Methods
    }
}
