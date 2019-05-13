using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NSwag.Annotations;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    public class BaseController : ControllerBase
    {
        #region Fields

        private readonly IHubContext<DataHub, IDataHub> dataHubContext;

        #endregion

        #region Constructors

        protected BaseController(IHubContext<DataHub, IDataHub> dataHubContext)
        {
            this.dataHubContext = dataHubContext;
        }

        #endregion

        #region Properties

        public IHubContext<DataHub, IDataHub> DataHubContext => this.dataHubContext;

        #endregion

        #region Methods

        [SwaggerIgnore]
        public BadRequestObjectResult BadRequest<T>(IOperationResult<T> operationResult)
            where T : class
        {
            return this.BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = operationResult?.Description
            });
        }

        [SwaggerIgnore]
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
            if (id.HasValue == false || this.dataHubContext.Clients == null)
            {
                return;
            }

            var eventDetails = new EntityChangedHubEvent
            {
                Id = id.Value,
                EntityType = entityType,
                Operation = operation
            };

            await this.dataHubContext.Clients.All.EntityUpdated(eventDetails);
        }

        #endregion
    }
}
