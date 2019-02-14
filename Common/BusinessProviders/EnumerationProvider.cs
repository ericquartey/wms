using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class EnumerationProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public EnumerationProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public static IEnumerable<Enumeration> GetAllItemManagementTypes()
        {
            var values = System.Enum.GetValues(typeof(ItemManagementType));

            return values.Cast<ItemManagementType>()
                .Select(x => new Enumeration((int)x, System.Enum.GetName(typeof(ItemManagementType), x)));
        }

        public IQueryable<Enumeration> GetAislesByAreaId(int areaId)
        {
            return this.dataContext.Aisles
                .AsNoTracking()
                .Include(a => a.Area)
                .Where(a => a.AreaId == areaId)
                .OrderBy(a => a.Area.Name)
                .ThenBy(a => a.Name)
                .Select(a => new Enumeration(
                    a.Id,
                    $"{a.Area.Name} - {a.Name}"));
        }

        #endregion
    }
}
