using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        #region Fields

        private readonly IServiceProvider serviceProvider;
        private const string DEFAULT_ORDERBY_FIELD = nameof(Item.Code);

        #endregion Fields

        #region Constructors

        public ItemsController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion Constructors

        #region Methods

        [HttpGet]
        public IEnumerable<Item> GetAll(int skip = 0, int take = int.MaxValue, string orderBy = DEFAULT_ORDERBY_FIELD)
        {
            using (var databaseContext = (DatabaseContext)this.serviceProvider.GetService(typeof(DatabaseContext)))
            {
                var orderByField = string.IsNullOrWhiteSpace(orderBy) ? DEFAULT_ORDERBY_FIELD : orderBy;
                var skipValue = skip < 0 ? 0 : skip;
                var takeValue = take < 0 ? int.MaxValue : take;

                var expression = CreateSelectorExpression<Common.DataModels.Item, string>(orderByField);

                return databaseContext.Items
                    .Skip(skipValue)
                    .Take(takeValue)
                    .OrderBy(expression)
                    .Select(i => new Item
                    {
                        Id = i.Id,
                        Code = i.Code
                    }
                    )
                    .ToList();
            }
        }

        public static Expression<Func<T, TResult>> CreateSelectorExpression<T, TResult>(string propertyName)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, TResult>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, propertyName),
                                                                    parameterExpression);
        }

        [HttpPost]
        public List<Mission> Withdraw(ItemWithdraw itemWithdraw)
        {
            var mission1Quantity = itemWithdraw.Quantity / 2;
            return new List<Mission>
            {
#pragma warning disable IDE0009
                new Mission
                {
                    Id = 1,
                    ItemId = itemWithdraw.ItemId,
                    Quantity = mission1Quantity,
                    BayId = itemWithdraw.BayId,
                    Type = MissionType.Withdrawal
                },
                new Mission
                {
                    Id = 2,
                    ItemId = itemWithdraw.ItemId,
                    Quantity = itemWithdraw.Quantity  - mission1Quantity,
                    BayId = itemWithdraw.BayId,
                    Type = MissionType.Withdrawal
                }
 #pragma warning restore IDE0009
            };
        }

        #endregion Methods
    }
}
