using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using System.Collections.ObjectModel;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperatorController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IItemsDataService itemsDataService;

        private readonly IServiceProvider services;

        #endregion

        #region Constructors

        public OperatorController(IEventAggregator eventAggregator, IServiceProvider services, IItemsDataService itemsDataService)
        {
            this.eventAggregator = eventAggregator;
            this.services = services;
            this.itemsDataService = itemsDataService;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(ObservableCollection<Item>))]
        [ProducesResponseType(404)]
        [HttpGet("Items/{code}/{quantity}")]
        public async Task<ActionResult<ObservableCollection<Item>>> Items(string code, int quantity)
        {
            Console.WriteLine("****************************************************************** REQUEST ARRIVED ********************************************************************************************************************************");
            var item = await this.itemsDataService.GetAllAsync(search: code);
            if (item != null)
            {
                return this.Ok(await this.itemsDataService.GetAllAsync(skip: item[0].Id - (item[0].Id < (quantity / 2) ? item[0].Id : (quantity / 2)), take: quantity));
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
