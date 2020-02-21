using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.Utils.Missions;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineMissionsVolatileProvider : IMachineMissionsVolatileProvider
    {
        #region Constructors

        public MachineMissionsVolatileProvider()
        {
            this.MachineMissions = new List<IMission>();
        }

        #endregion

        #region Properties

        public List<IMission> MachineMissions { get; set; }

        #endregion
    }
}
