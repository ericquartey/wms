using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Extensions;
using Ferretto.WMS.App.Core.Interfaces;
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

        public async Task<IOperationResult<CompartmentType>> DeleteAsync(int id)
        {
            try
            {
                await this.compartmentTypesDataService.DeleteAsync(id);

                return new OperationResult<CompartmentType>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<CompartmentType>(ex);
            }
        }

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            try
            {
                return (await this.compartmentTypesDataService.GetAllAsync())
                    .Select(c => new Enumeration(c.Id, string.Format(General.CompartmentTypeListFormat, c.Width, c.Height)));
            }
            catch
            {
                return new List<Enumeration>();
            }
        }

        public async Task<IEnumerable<CompartmentType>> GetAllAsync(int skip, int take, IEnumerable<SortOption> orderBySortOptions = null, string whereString = null, string searchString = null)
        {
            try
            {
                var compartmentTypes = await this.compartmentTypesDataService
                    .GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString);

                return compartmentTypes
                    .Select(i => new CompartmentType
                    {
                        Id = i.Id,
                        Height = i.Height,
                        Width = i.Width,
                        CompartmentsCount = i.CompartmentsCount,
                        HeightDescription = string.Format(
                            General.CompartmentTypeDimensionFormat,
                            i.Height),
                        WidthDescription = string.Format(
                            General.CompartmentTypeDimensionFormat,
                            i.Width),
                        Policies = i.GetPolicies()
                    });
            }
            catch
            {
                return new List<CompartmentType>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.compartmentTypesDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            try
            {
                return await this.compartmentTypesDataService.GetAllCountAsync(whereString, searchString);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return await this.compartmentTypesDataService.GetUniqueValuesAsync(propertyName);
            }
            catch
            {
                return new List<object>();
            }
        }

        #endregion
    }
}
