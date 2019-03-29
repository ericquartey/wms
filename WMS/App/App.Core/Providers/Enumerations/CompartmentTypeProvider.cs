using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class CompartmentTypeProvider : ICompartmentTypeProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ICompartmentTypesDataService compartmentTypesDataService;

        #endregion

        #region Constructors

        public CompartmentTypeProvider(
            WMS.Data.WebAPI.Contracts.ICompartmentTypesDataService compartmentTypesDataService)
        {
            this.compartmentTypesDataService = compartmentTypesDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<CompartmentType>> CreateAsync(CompartmentType model, int? itemId = null, int? maxCapacity = null)
        {
            // TODO: Task 823
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var compartmentType = await this.compartmentTypesDataService.CreateAsync(
                    new WMS.Data.WebAPI.Contracts.CompartmentType
                    {
                        Height = model.Height,
                        Id = model.Id,
                        Width = model.Width
                    }, itemId,
                    maxCapacity);

                model.Id = compartmentType.Id;

                return new OperationResult<CompartmentType>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<CompartmentType>(ex);
            }
        }

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            return (await this.compartmentTypesDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, string.Format(Common.Resources.MasterData.CompartmentTypeListFormat, c.Width, c.Height)));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.compartmentTypesDataService.GetAllCountAsync();
        }

        #endregion
    }
}
