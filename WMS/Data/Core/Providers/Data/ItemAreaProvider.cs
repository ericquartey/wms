using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemAreaProvider : BaseProvider, IItemAreaProvider
    {
        #region Constructors

        public ItemAreaProvider(
            DatabaseContext dataContext,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemArea>> CreateAsync(int id, int itemId)
        {
            try
            {
                var entry = await this.DataContext.ItemsAreas.AddAsync(new Common.DataModels.ItemArea
                {
                    AreaId = id,
                    ItemId = itemId
                });

                var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
                if (changedEntitiesCount > 0)
                {
                    var model = new ItemArea { AreaId = entry.Entity.AreaId, ItemId = entry.Entity.ItemId };

                    this.NotificationService.PushCreate(model);

                    return new SuccessOperationResult<ItemArea>(model);
                }

                return new CreationErrorOperationResult<ItemArea>();
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityOperationResult<ItemArea>(ex);
            }
        }

        public async Task<IOperationResult<ItemArea>> DeleteAsync(int id, int itemId)
        {
            var existingModel = (await this.GetByItemIdAsync(itemId)).SingleOrDefault(m => m.Id == id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemArea>();
            }

            if (!existingModel.CanDelete())
            {
                return new UnprocessableEntityOperationResult<ItemArea>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            this.DataContext.ItemsAreas.Remove(new Common.DataModels.ItemArea { AreaId = id, ItemId = itemId });

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                var model = new ItemArea
                {
                    AreaId = id,
                    ItemId = itemId
                };

                this.NotificationService.PushDelete(existingModel);

                return new SuccessOperationResult<ItemArea>(model);
            }

            return new UnprocessableEntityOperationResult<ItemArea>();
        }

        public async Task<IEnumerable<AllowedItemArea>> GetByItemIdAsync(int id)
        {
            var areasWithItem = await this.DataContext.Compartments
                .Where(c => c.ItemId == id)
                .Select(c => new
                {
                    AreaId = c.LoadingUnit.Cell.Aisle.AreaId,
                })
                .Distinct()
                .ToArrayAsync();

            var models = await this.DataContext.ItemsAreas
                .Where(x => x.ItemId == id)
                .GroupJoin(
                    this.DataContext.Compartments,
                    ia => new { ia.ItemId, ia.AreaId },
                    c => new { ItemId = c.ItemId.Value, c.LoadingUnit.Cell.Aisle.AreaId },
                    (ia, c) => new
                    {
                        a = ia.Area,
                        c,
                    })
                .SelectMany(
                    j => j.c.DefaultIfEmpty(),
                    (j, c) => new
                    {
                        a = j.a,
                        stock = c != null ? c.Stock : 0,
                    })
                .GroupBy(
                    x => x.a,
                    (key, group) => new
                    {
                        Id = key.Id,
                        Name = key.Name,
                        TotalStock = group.Sum(g => g.stock),
                    })
                .Select(x => new AllowedItemArea
                {
                    Id = x.Id,
                    Name = x.Name,
                    TotalStock = x.TotalStock,
                    IsItemInArea = areasWithItem.Any(a => a.AreaId == x.Id)
                })
                .ToArrayAsync();

            foreach (var model in models)
            {
                SetPolicies(model);
            }

            return models;
        }

        private static void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy((model as IItemAreaDeletePolicy).ComputeDeletePolicy());
        }

        #endregion
    }
}
