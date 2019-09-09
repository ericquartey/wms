using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Prism.Events;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.DTOs;
using Microsoft.AspNetCore.Http;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using System;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadingUnitsController : BaseAutomationController
    {
        #region Fields

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            IEventAggregator eventAggregator,
            ILoadingUnitsProvider loadingUnitStatisticsProvider,
            IMachinesDataService machinesDataService)
            : base(eventAggregator)
        {
            if (loadingUnitStatisticsProvider is null)
            {
                throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            }

            if (machinesDataService is null)
            {
                throw new System.ArgumentNullException(nameof(machinesDataService));
            }

            this.loadingUnitsProvider = loadingUnitStatisticsProvider;
            this.machinesDataService = machinesDataService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<DataModels.LoadingUnit>> GetAll()
        {
            var loadingUnits = this.loadingUnitsProvider.GetAll();
            return this.Ok(loadingUnits);
        }

        [HttpGet("statistics/space")]
        public async Task<ActionResult<IEnumerable<LoadingUnitSpaceStatistics>>> GetSpaceStatisticsAsync()
        {
            var statistics = this.loadingUnitsProvider.GetSpaceStatistics();

            try
            {
                var machineId = 1; // TODO HACK remove this hardcoded value
                var loadingUnits = await this.machinesDataService.GetLoadingUnitsByIdAsync(machineId);
                foreach (var stat in statistics)
                {
                    var loadingUnit = loadingUnits.SingleOrDefault(l => l.Code == stat.Code);
                    if (loadingUnit != null)
                    {
                        stat.CompartmentsCount = loadingUnit.CompartmentsCount;
                        stat.AreaFillPercentage = (decimal?)loadingUnit.AreaFillRate.Value * 100;
                    }
                }
            }
            catch (System.Exception)
            {
                // do nothing:
                // data from WMS will remain to its default values
            }

            return this.Ok(statistics);
        }

        [HttpGet("statistics/weight")]
        public async Task<ActionResult<IEnumerable<LoadingUnitWeightStatistics>>> GetWeightStatisticsAsync()
        {
            var statistics = this.loadingUnitsProvider.GetWeightStatistics();
            try
            {
                var machineId = 1; // TODO HACK remove this hardcoded value
                var loadingUnits = await this.machinesDataService.GetLoadingUnitsByIdAsync(machineId);
                foreach (var stat in statistics)
                {
                    var loadingUnit = loadingUnits.SingleOrDefault(l => l.Code == stat.Code);
                    if (loadingUnit != null)
                    {
                        stat.CompartmentsCount = loadingUnit.CompartmentsCount;
                    }
                }
            }
            catch (System.Exception)
            {
                // do nothing:
                // data from WMS will remain to its default values
            }

            return this.Ok(statistics);
        }

        [HttpPost("start-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StartMoving([FromBody]MoveDrawerMessageDataDTO data)
        {
            var drawerOperationData = new DrawerOperationMessageData(
               data.DrawerOperation,
               DrawerOperationStep.None);

            drawerOperationData.Source = DrawerDestination.InternalBay1Up; // TODO HACK remove this hardcoded value
            drawerOperationData.Destination = DrawerDestination.Cell;

            this.PublishCommand(
                drawerOperationData,
                "Execute Drawer Operation Command",
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation);

            return this.Accepted();
        }

        [HttpGet("stop-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.PublishCommand(
                null,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }

        #endregion
    }
}
