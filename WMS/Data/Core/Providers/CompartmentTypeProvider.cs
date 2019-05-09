using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class CompartmentTypeProvider : ICompartmentTypeProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        #endregion

        #region Constructors

        public CompartmentTypeProvider(
            DatabaseContext dataContext,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider)
        {
            this.dataContext = dataContext;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<CompartmentType>> CreateAsync(CompartmentType model)
        {
            return await this.CreateAsync(model, null, null);
        }

        public async Task<IOperationResult<CompartmentType>> CreateAsync(
            CompartmentType model,
            int? itemId,
            double? maxCapacity)
        {
            if (model == null ||
                !model.Height.HasValue ||
                !model.Width.HasValue)
            {
                throw new ArgumentNullException(nameof(model));
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var existingCompartmentType =
                    await this.dataContext.CompartmentTypes
                        .SingleOrDefaultAsync(
                            ct =>
                                ((int)ct.Width == (int)model.Width && (int)ct.Height == (int)model.Height)
                                ||
                                ((int)ct.Width == (int)model.Height && (int)ct.Height == (int)model.Width));

                if (existingCompartmentType == null)
                {
                    var entry = await this.dataContext.CompartmentTypes.AddAsync(
                                    new Common.DataModels.CompartmentType
                                    {
                                        Height = model.Height.Value,
                                        Width = model.Width.Value
                                    });

                    if (await this.dataContext.SaveChangesAsync() <= 0)
                    {
                        return new CreationErrorOperationResult<CompartmentType>();
                    }

                    existingCompartmentType = entry.Entity;
                    model.Id = entry.Entity.Id;
                }
                else
                {
                    model.Id = existingCompartmentType.Id;
                }

                if (itemId.HasValue)
                {
                    var result = await this.CreateOrUpdateItemCompartmentTypeAsync(
                                     itemId.Value,
                                     maxCapacity,
                                     existingCompartmentType.Id);

                    if (!result.Success)
                    {
                        return new CreationErrorOperationResult<CompartmentType>();
                    }
                }

                scope.Complete();
                return new SuccessOperationResult<CompartmentType>(model);
            }
        }

        public async Task<IEnumerable<CompartmentType>> GetAllAsync()
        {
            return await this.dataContext.CompartmentTypes
                       .Select(c => new CompartmentType
                       {
                           Id = c.Id,
                           Width = c.Width,
                           Height = c.Height,
                       })
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.CompartmentTypes.CountAsync();
        }

        public async Task<CompartmentType> GetByIdAsync(int id)
        {
            return await this.dataContext.CompartmentTypes
                       .Select(c => new CompartmentType
                       {
                           Id = c.Id,
                           Width = c.Width,
                           Height = c.Height,
                       })
                       .SingleOrDefaultAsync(a => a.Id == id);
        }

        private async Task<IOperationResult<ItemCompartmentType>> CreateOrUpdateItemCompartmentTypeAsync(
            int itemId,
            double? maxCapacity,
            int compartmentTypeId)
        {
            var existingIcTModel =
                await this.dataContext.ItemsCompartmentTypes
                    .SingleOrDefaultAsync(
                        ict =>
                            ict.CompartmentTypeId == compartmentTypeId
                            &&
                            ict.ItemId == itemId);

            return existingIcTModel != null
                       ? await this.itemCompartmentTypeProvider.UpdateAsync(
                             new ItemCompartmentType
                             {
                                 ItemId = existingIcTModel.ItemId,
                                 MaxCapacity = maxCapacity,
                                 CompartmentTypeId = existingIcTModel.CompartmentTypeId
                             })
                       : await this.itemCompartmentTypeProvider.CreateAsync(
                             new ItemCompartmentType
                             {
                                 ItemId = itemId,
                                 MaxCapacity = maxCapacity,
                                 CompartmentTypeId = compartmentTypeId
                             });
        }

        #endregion
    }
}
