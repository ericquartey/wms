using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        #endregion Fields

        #region Constructors

        public ItemsController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion Constructors

        #region Methods

        [HttpGet]
        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            using (var databaseContext = (DatabaseContext)this.serviceProvider.GetService(typeof(DatabaseContext)))
            {
                return databaseContext.Items.Select(i => new Item
                {
                    Id = i.Id
                }
                ).ToList();
            }
        }

        [HttpPost]
        public async Task<List<Mission>> WithdrawAsync(ItemWithdraw itemWithdraw)
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
