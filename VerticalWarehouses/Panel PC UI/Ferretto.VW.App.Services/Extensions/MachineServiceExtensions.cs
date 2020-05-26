using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto
{
    public static class MachineServiceExtensions
    {
        #region Methods

        public static bool DrawerInLocationById(this IEnumerable<LoadingUnit> loadUnits, int id)
        {
            return loadUnits.Any(f => f.Id == id && f.Status == LoadingUnitStatus.InLocation);
        }

        public static bool DrawerInBay(this IEnumerable<LoadingUnit> loadUnits)
        {
            return loadUnits.Any(f => f.Status == LoadingUnitStatus.InBay);
        }

        public static bool DrawerInBayById(this IEnumerable<LoadingUnit> loadUnits, int id)
        {
            return loadUnits.Any(f => f.Id == id && f.Status == LoadingUnitStatus.InBay);
        }

        public static bool DrawerExists(this IEnumerable<LoadingUnit> loadUnits, int id)
        {
            return loadUnits.Any(f => f.Id == id);
        }

        #endregion
    }
}
