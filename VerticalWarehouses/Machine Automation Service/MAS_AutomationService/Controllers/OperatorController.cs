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
    [Route("1.0.0/Operator/[controller]")]
    [ApiController]
    public class OperatorController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IItemsDataService itemsDataService;

        private readonly IMissionsDataService missionsDataService;

        private readonly IServiceProvider services;

        #endregion

        #region Constructors

        public OperatorController(IEventAggregator eventAggregator, IServiceProvider services, IItemsDataService itemsDataService, IMissionsDataService missionsDataService)
        {
            this.eventAggregator = eventAggregator;
            this.services = services;
            this.itemsDataService = itemsDataService;
            this.missionsDataService = missionsDataService;
        }

        #endregion

        #region Methods

        [HttpGet("Pick/{missionId}/{evadedQuantity}")]
        public async void PickAsync(int missionId, int evadedQuantity)
        {
            await this.missionsDataService.CompleteItemAsync(missionId, evadedQuantity);
        }

        #endregion
    }
}
