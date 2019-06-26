using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class CompartmentProvider
    {
        #region Methods

        public async Task<IEnumerable<Compartment>> GetAllAsync(
                                                    int skip,
                                                    int take,
                                                    IEnumerable<SortOption> orderBySortOptions = null,
                                                    string whereString = null,
                                                    string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<Compartment, Common.DataModels.Compartment>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var model in models)
            {
                SetPolicies(model);
            }

            return models;
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<Compartment, Common.DataModels.Compartment>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<CompartmentDetails> GetByIdAsync(int id)
        {
            var allowedItemsCount =
                await this.DataContext.Compartments
                    .Where(c => c.Id == id)
                    .SelectMany(c => c.CompartmentType.ItemsCompartmentTypes)
                    .CountAsync();

            var model = await this.GetAllDetailsBase()
                .SingleOrDefaultAsync(c => c.Id == id);

            if (model != null)
            {
                model.AllowedItemsCount = allowedItemsCount;
                SetPolicies(model);
            }

            return model;
        }

        public async Task<IEnumerable<Compartment>> GetByItemIdAsync(int id)
        {
            return await this.GetAllBase()
                .Where(c => c.ItemId == id)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.DataContext.Compartments,
                this.GetAllBase());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
                    "Major Code Smell",
    "S4058:Overloads with a \"StringComparison\" parameter should be used",
    Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<Compartment, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsDouble = double.TryParse(search, out var searchAsDouble);

            return (c) =>
                (c.CompartmentStatusDescription != null && c.CompartmentStatusDescription.Contains(search))
                || (c.ItemDescription != null && c.ItemDescription.Contains(search))
                || (c.ItemMeasureUnit != null && c.ItemMeasureUnit.Contains(search))
                || (c.LoadingUnitCode != null && c.LoadingUnitCode.Contains(search))
                || (c.Lot != null && c.Lot.Contains(search))
                || (c.MaterialStatusDescription != null && c.MaterialStatusDescription.Contains(search))
                || (c.Sub1 != null && c.Sub1.Contains(search))
                || (c.Sub2 != null && c.Sub2.Contains(search))
                || (successConversionAsDouble
                    && Equals(c.Stock, searchAsDouble));
        }

        private IQueryable<Compartment> GetAllBase()
        {
            return this.DataContext.Compartments
                .Select(c => new Compartment
                {
                    AisleName = c.LoadingUnit.Cell.Aisle.Name,
                    AreaName = c.LoadingUnit.Cell.Aisle.Area.Name,
                    CompartmentStatusDescription = c.CompartmentStatus.Description,
                    HasRotation = c.HasRotation,
                    Depth = c.HasRotation ? c.CompartmentType.Width : c.CompartmentType.Depth,
                    Id = c.Id,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    ItemDescription = c.Item.Description,
                    ItemId = c.ItemId,
                    ItemMeasureUnit = c.Item.MeasureUnit.Description,
                    LoadingUnitCode = c.LoadingUnit.Code,
                    LoadingUnitId = c.LoadingUnitId,
                    Lot = c.Lot,
                    MaterialStatusDescription = c.MaterialStatus.Description,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    Width = c.HasRotation ? c.CompartmentType.Depth : c.CompartmentType.Width,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                });
        }

        #endregion
    }
}
