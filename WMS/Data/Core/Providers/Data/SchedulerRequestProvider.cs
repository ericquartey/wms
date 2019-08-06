using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.Common.Utils.Extensions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class SchedulerRequestProvider : BaseProvider, ISchedulerRequestProvider
    {
        #region Fields

        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public SchedulerRequestProvider(
            DatabaseContext dataContext,
            INotificationService notificationService,
            IMapper mapper)
            : base(dataContext, notificationService)
        {
            this.mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<SchedulerRequest>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .ToArrayAsync<SchedulerRequest, Common.DataModels.SchedulerRequest>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<SchedulerRequest, Common.DataModels.SchedulerRequest>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<SchedulerRequest> GetByIdAsync(int id)
        {
            var result = await this.GetAllBase()
                .SingleOrDefaultAsync(i => i.Id == id);

            // TODO: compute policies
            return result;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.DataContext.SchedulerRequests,
                this.GetAllBase());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<SchedulerRequest, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsDouble = double.TryParse(search, out var searchAsDouble);

            return (i) =>
                (i.BayDescription != null && i.BayDescription.Contains(search))
                || (i.ItemDescription != null && i.ItemDescription.Contains(search))
                || (i.ListDescription != null && i.ListDescription.Contains(search))
                || (i.ListRowCode != null && i.ListRowCode.Contains(search))
                || i.OperationType.ToString().Contains(search)
                || (successConversionAsDouble
                    && Equals(i.RequestedQuantity, searchAsDouble));
        }

        private IQueryable<SchedulerRequest> GetAllBase()
        {
            return this.DataContext.SchedulerRequests
               .ProjectTo<SchedulerRequest>(this.mapper.ConfigurationProvider);
        }

        #endregion
    }
}
