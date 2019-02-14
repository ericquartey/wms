using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;

namespace Ferretto.Common.BusinessProviders
{
    public class CompartmentTypeProvider : ICompartmentTypeProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ICompartmentTypesDataService compartmentTypesDataService;

        private readonly IDatabaseContextService dataContext;

        private readonly ItemCompartmentTypeProvider itemCompartmentTypeProvider;

        #endregion

        #region Constructors

        public CompartmentTypeProvider(
            IDatabaseContextService context,
            ItemCompartmentTypeProvider itemCompartmentTypeProvider,
            WMS.Data.WebAPI.Contracts.ICompartmentTypesDataService compartmentTypesDataService)
        {
            this.dataContext = context;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
            this.compartmentTypesDataService = compartmentTypesDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult> AddAsync(CompartmentType model, int? itemId = null, int? maxCapacity = null)
        {
            // TODO: Task 823
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dc = this.dataContext.Current)
                {
                    var compartmentType = dc.CompartmentTypes
                        .SingleOrDefault(ct =>
                            (ct.Width == model.Width && ct.Height == model.Height)
                            ||
                            (ct.Width == model.Height && ct.Height == model.Width));

                    if (compartmentType == null)
                    {
                        var entry = dc.CompartmentTypes.Add(new DataModels.CompartmentType
                        {
                            Height = model.Height,
                            Width = model.Width
                        });

                        var changedEntitiesCount = await dc.SaveChangesAsync();
                        if (changedEntitiesCount > 0)
                        {
                            compartmentType = entry.Entity;
                            model.Id = entry.Entity.Id;
                        }
                        else
                        {
                            return new OperationResult(false);
                        }
                    }

                    if (itemId.HasValue)
                    {
                        var result = await this.itemCompartmentTypeProvider.AddAsync(
                            new ItemCompartmentType
                            {
                                ItemId = itemId.Value,
                                MaxCapacity = maxCapacity,
                                CompartmentTypeId = compartmentType.Id
                            });

                        if (result.Success)
                        {
                            await dc.SaveChangesAsync();
                        }
                        else
                        {
                            return new OperationResult(false);
                        }
                    }

                    return new OperationResult(true, entityId: compartmentType.Id);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public Task<IOperationResult> AddAsync(CompartmentType model)
        {
            return this.AddAsync(model, null, null);
        }

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            return (await this.compartmentTypesDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, string.Format(Resources.MasterData.CompartmentTypeListFormat, c.Width, c.Height)));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.compartmentTypesDataService.GetAllCountAsync();
        }

        #endregion
    }
}
