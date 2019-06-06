using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Core.Extensions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class AreaProvider : IAreaProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService;

        private readonly WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        #endregion

        #region Constructors

        public AreaProvider(
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService,
            WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService)
        {
            this.areasDataService = areasDataService;
            this.itemsDataService = itemsDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemArea>> CreateAllowedByItemIdAsync(
                                                      int id,
                                                      int itemId)
        {
            try
            {
                var result = await this.itemsDataService.CreateAllowedAreaAsync(itemId, id);

                var itemArea = new ItemArea
                {
                    Id = result.Id,
                    AreaId = result.AreaId,
                    ItemId = result.ItemId
                };

                return new OperationResult<ItemArea>(true, itemArea);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemArea>(ex);
            }
        }

        public async Task<IOperationResult<AllowedItemArea>> DeleteAllowedByItemIdAsync(int id, int itemId)
        {
            try
            {
                await this.itemsDataService.DeleteAllowedAreaAsync(itemId, id);

                return new OperationResult<AllowedItemArea>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<AllowedItemArea>(ex);
            }
        }

        public async Task<IEnumerable<Area>> GetAllAsync()
        {
            try
            {
                return (await this.areasDataService.GetAllAsync())
                    .Select(a => new Area
                    {
                        Id = a.Id,
                        Name = a.Name,
                    });
            }
            catch
            {
                return new List<Area>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.areasDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<IOperationResult<IEnumerable<AllowedItemArea>>> GetAllowedByItemIdAsync(int id)
        {
            try
            {
                var items = await this.itemsDataService.GetAllowedAreasAsync(id);

                var result = items
                    .Select(i => new AllowedItemArea
                    {
                        Id = i.Id,
                        Name = i.Name,
                        TotalStock = i.TotalStock,
                        Policies = i.GetPolicies(),
                    });

                return new OperationResult<IEnumerable<AllowedItemArea>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<AllowedItemArea>>(e);
            }
        }

        public async Task<IOperationResult<IEnumerable<Area>>> GetAreasWithAvailabilityAsync(int id)
        {
            try
            {
                var result = (await this.itemsDataService.GetAreasWithAvailabilityAsync(id))
                    .Select(a => new Area
                    {
                        Id = a.Id,
                        Name = a.Name
                    });

                return new OperationResult<IEnumerable<Area>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<Area>>(e);
            }
        }

        public async Task<Area> GetByIdAsync(int id)
        {
            try
            {
                var area = await this.areasDataService.GetByIdAsync(id);
                return new Area
                {
                    Id = area.Id,
                    Name = area.Name,
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<IOperationResult<IEnumerable<Area>>> GetByItemIdAsync(int id)
        {
            try
            {
                var result = (await this.itemsDataService.GetAreasAsync(id))
                    .Select(a => new Area
                    {
                        Id = a.Id,
                        Name = a.Name
                    });

                return new OperationResult<IEnumerable<Area>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<Area>>(e);
            }
        }

        #endregion
    }
}
