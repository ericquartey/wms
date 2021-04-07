using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class MissionOperationsController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IHubContext<OperatorHub> hubContext;

        private readonly IMissionOperationsProvider missionOperationsProvider;

        private readonly IMissionOperationsWmsWebService missionOperationsWmsWebService;

        private readonly IMissionsWmsWebService missionsWmsWebService;

        #endregion

        #region Constructors

        public MissionOperationsController(
            IMissionOperationsProvider missionOperationsProvider,
            IMissionOperationsWmsWebService missionOperationsWmsWebService,
            IMissionsWmsWebService missionsWmsWebService,
            IBaysDataProvider baysDataProvider,
            IHubContext<OperatorHub> hubContext)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            this.missionOperationsProvider = missionOperationsProvider ?? throw new ArgumentNullException(nameof(missionOperationsProvider));
            this.missionOperationsWmsWebService = missionOperationsWmsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWmsWebService));
            this.missionsWmsWebService = missionsWmsWebService ?? throw new ArgumentNullException(nameof(missionsWmsWebService));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("{id}/abort")]
        public async Task<ActionResult> AbortAsync(int id)
        {
            await this.missionOperationsProvider.AbortAsync(id);

            return this.Ok();
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteAsync(int id, double quantity, string printerName, string barcode)
        {
            await this.missionOperationsProvider.CompleteAsync(id, quantity, printerName, barcode);

            await this.hubContext.Clients.All.SendAsync(nameof(IOperatorHub.ProductsChanged));

            return this.Ok();
        }

        [HttpPost("{id}/execute")]
        public async Task<ActionResult<MissionOperation>> ExecuteAsync(int id)
        {
            var operation = await this.missionOperationsWmsWebService.ExecuteAsync(id);

            return this.Ok(operation);
        }

        [HttpGet("{type}/reasons")]
        public async Task<ActionResult<IEnumerable<OperationReason>>> GetAllReasonsAsync(MissionOperationType type)
        {
            return this.Ok(await this.missionOperationsProvider.GetReasonsAsync(type));
        }

        [HttpGet("count")]
        public ActionResult<int> GetByBayCount()
        {
            var missionOperationsCount = this.missionOperationsProvider.GetCountByBay(this.BayNumber);

            return this.Ok(missionOperationsCount);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MissionOperation>> GetByIdAsync(int id)
        {
            return this.Ok(await this.missionOperationsWmsWebService.GetByIdAsync(id));
        }

        [HttpGet("get-unit-id")]
        public ActionResult<int> GetUnitId(int missionId)
        {
            var missionOperationsCount = this.missionOperationsProvider.GetUnitId(missionId, this.BayNumber);

            return this.Ok(missionOperationsCount);
        }

        [HttpPost("{id}/partially-complete")]
        public async Task<ActionResult> PartiallyCompleteAsync(int id, double quantity, double wastedQuantity, string printerName, bool emptyCompartment = false, bool fullCompartment = false)
        {
            await this.missionOperationsProvider.PartiallyCompleteAsync(id, quantity, wastedQuantity, printerName, emptyCompartment, fullCompartment);

            await this.hubContext.Clients.All.SendAsync(nameof(IOperatorHub.ProductsChanged));

            return this.Ok();
        }

        #endregion
    }
}
