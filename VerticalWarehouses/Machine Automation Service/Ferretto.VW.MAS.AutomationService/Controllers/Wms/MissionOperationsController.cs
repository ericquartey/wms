using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class MissionOperationsController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IHubContext<OperatorHub> hubContext;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

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
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IHubContext<OperatorHub> hubContext)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            this.missionOperationsProvider = missionOperationsProvider ?? throw new ArgumentNullException(nameof(missionOperationsProvider));
            this.missionOperationsWmsWebService = missionOperationsWmsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWmsWebService));
            this.missionsWmsWebService = missionsWmsWebService ?? throw new ArgumentNullException(nameof(missionsWmsWebService));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("{id}/abort")]
        public async Task<ActionResult> AbortAsync(int id, string userName = null)
        {
            await this.missionOperationsProvider.AbortAsync(id, userName);

            return this.Ok();
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteAsync(int id, double quantity, string printerName, string barcode, double wastedQuantity, string toteBarcode, string userName = null, int? nrLabels = null)
        {
            await this.missionOperationsProvider.CompleteAsync(id, quantity, printerName, barcode, wastedQuantity, toteBarcode, userName, nrLabels);

            await this.hubContext.Clients.All.SendAsync(nameof(IOperatorHub.ProductsChanged));

            return this.Ok();
        }

        [HttpPost("{id}/execute")]
        public async Task<ActionResult<MissionOperation>> ExecuteAsync(int id, string userName = null)
        {
            var operation = await this.missionOperationsWmsWebService.ExecuteAsync(id, userName);

            return this.Ok(operation);
        }

        [HttpGet("{type}/reasons")]
        public async Task<ActionResult<IEnumerable<OperationReason>>> GetAllReasonsAsync(MissionOperationType type)
        {
            return this.Ok(await this.missionOperationsProvider.GetReasonsAsync(type));
        }

        [HttpGet("{id}/aggregate")]
        public async Task<ActionResult<MissionOperation>> GetByAggregateAsync(int id)
        {
            return this.Ok(await this.missionOperationsWmsWebService.AggregateAsync(id));
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

        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<OperationReason>>> GetOrdersAsync()
        {
            // custom siderpol
            var orders = await this.missionOperationsProvider.GetOrdersAsync();
            return this.Ok(orders.OrderBy(o => o.Name));
        }

        [HttpGet("{id}/get-put-list")]
        public async Task<ActionResult<IEnumerable<MissionOperation>>> GetPutListsAsync(int id)
        {
            return this.Ok(await this.missionOperationsWmsWebService.GetPutListsAsync(id));
        }

        [HttpGet("socket-link-operation")]
        public async Task<ActionResult<SocketLinkOperation>> GetSocketLinkOperationAsync()
        {
            return this.Ok(this.machineVolatileDataProvider.SocketLinkOperation[this.BayNumber]);
        }

        [HttpGet("get-unit-id")]
        public ActionResult<int> GetUnitId(int missionId)
        {
            var missionOperationsCount = this.missionOperationsProvider.GetUnitId(missionId, this.BayNumber);

            return this.Ok(missionOperationsCount);
        }

        [HttpPost("{id}/partially-complete")]
        public async Task<ActionResult> PartiallyCompleteAsync(int id, double quantity, double wastedQuantity, string printerName, bool emptyCompartment = false, bool fullCompartment = false, string userName = null, int? nrLabels = null)
        {
            await this.missionOperationsProvider.PartiallyCompleteAsync(id, quantity, wastedQuantity, printerName, emptyCompartment, fullCompartment, userName, nrLabels);

            await this.hubContext.Clients.All.SendAsync(nameof(IOperatorHub.ProductsChanged));

            return this.Ok();
        }

        [HttpPost("{id}/socket-link-complete")]
        public async Task<ActionResult> SocketLinkCompleteAsync(string id, double quantity, DateTimeOffset completedTime)
        {
            var operation = this.machineVolatileDataProvider.SocketLinkOperation[this.BayNumber];
            if (operation is null || operation.Id != id)
            {
                throw new InvalidOperationException($"Socket link operation {id} not active.");
            }
            //if (operation.IsCompleted)
            //{
            //    throw new InvalidOperationException($"Socket link operation {id} already completed.");
            //}
            operation.IsCompleted = true;
            operation.ConfirmedQuantity = quantity;
            operation.CompletedTime = completedTime;
            this.machineVolatileDataProvider.SocketLinkOperation[this.BayNumber] = operation;
            return this.Ok();
        }

        /// <summary>
        /// Sospende l'operazione
        /// </summary>
        /// <param name="id">Id della singola operazione che si vuole sospendere</param>
        /// <param name="userName">nome utente</param>
        /// <returns></returns>
        [HttpPost("{id}/suspend")]
        public async Task<ActionResult<MissionOperation>> SuspendAsync(int id, string userName = null)
        {
            var operation = await this.missionOperationsProvider.SuspendAsync(id, userName);

            return this.Ok(operation);
        }

        #endregion
    }
}
