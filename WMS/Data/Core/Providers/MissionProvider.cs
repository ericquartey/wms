using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class MissionProvider : IMissionProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public MissionProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Mission>> GetAllAsync()
        {
            return await this.dataContext.Missions
                       .Select(m => new Mission
                       {
                           Id = m.Id,
                           BayId = m.BayId,
                           CellId = m.CellId,
                           CompartmentId = m.CompartmentId,
                           Lot = m.Lot,
                           ItemId = m.ItemId,
                           ItemListId = m.ItemListId,
                           ItemListRowId = m.ItemListRowId,
                           LoadingUnitId = m.LoadingUnitId,
                           MaterialStatusId = m.MaterialStatusId,
                           PackageTypeId = m.PackageTypeId,
                           Quantity = m.RequiredQuantity,
                           RegistrationNumber = m.RegistrationNumber,
                           Status = (MissionStatus)m.Status,
                           Sub1 = m.Sub1,
                           Sub2 = m.Sub2,
                           Type = (MissionType)m.Type
                       })
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.Missions.CountAsync();
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            return await this.dataContext.Missions
                       .Select(m => new Mission
                       {
                           Id = m.Id,
                           BayId = m.BayId,
                           CellId = m.CellId,
                           CompartmentId = m.CompartmentId,
                           Lot = m.Lot,
                           ItemId = m.ItemId,
                           ItemListId = m.ItemListId,
                           ItemListRowId = m.ItemListRowId,
                           LoadingUnitId = m.LoadingUnitId,
                           MaterialStatusId = m.MaterialStatusId,
                           PackageTypeId = m.PackageTypeId,
                           Quantity = m.RequiredQuantity,
                           RegistrationNumber = m.RegistrationNumber,
                           Status = (MissionStatus)m.Status,
                           Sub1 = m.Sub1,
                           Sub2 = m.Sub2,
                           Type = (MissionType)m.Type
                       })
                       .SingleOrDefaultAsync(m => m.Id == id);
        }

        #endregion
    }
}
