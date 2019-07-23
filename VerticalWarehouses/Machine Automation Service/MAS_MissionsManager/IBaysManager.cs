using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.MissionsManager
{
    public interface IBaysManager
    {
        #region Properties

        IEnumerable<Bay> Bays { get; }

        #endregion

        #region Methods

        Task SetupBaysAsync();

        #endregion
    }
}
