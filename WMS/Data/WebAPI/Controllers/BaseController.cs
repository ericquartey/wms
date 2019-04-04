using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    public class BaseController : ControllerBase
    {
        #region Fields

        private readonly IHubContext<SchedulerHub, ISchedulerHub> schedulerHubContext;

        #endregion

        #region Constructors

        protected BaseController(IHubContext<SchedulerHub, ISchedulerHub> schedulerHubContext)
        {
            this.schedulerHubContext = schedulerHubContext;
        }

        #endregion

        #region Properties

        public IHubContext<SchedulerHub, ISchedulerHub> SchedulerHubContext => this.schedulerHubContext;

        #endregion

        #region Methods

        public BadRequestObjectResult BadRequest<T>(IOperationResult<T> operationResult)
            where T : class
        {
            return this.BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = operationResult?.Description
            });
        }

        public BadRequestObjectResult BadRequest(System.Exception exception)
        {
            return this.BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = exception?.Message
            });
        }

        protected async Task NotifyEntityUpdatedAsync(string entityType, int? id, HubEntityOperation operation)
        {
            if (id.HasValue == false)
            {
                return;
            }

            await this.schedulerHubContext.Clients.All.EntityUpdated(new EntityChangedHubEvent { Id = id.Value, EntityType = entityType, Operation = operation });
        }

        #endregion
    }
}
