using System.Collections.Generic;
using Ferretto.VW.MAS.Utils.Missions;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineMissionsVolatileProvider
    {
        #region Properties

        List<IMission> MachineMissions { get; set; }

        #endregion
    }
}
