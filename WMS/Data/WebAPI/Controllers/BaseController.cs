using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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

        protected ObjectResult NegativeResponse<T>(IOperationResult<T> operationResult)
        {
            if (operationResult == null)
            {
                throw new System.ArgumentNullException(nameof(operationResult));
            }

            if (operationResult.Success)
            {
                throw new System.InvalidOperationException();
            }

            switch (operationResult)
            {
                case UnprocessableEntityOperationResult<T> result:
                    {
                        return this.UnprocessableEntity(new ProblemDetails
                        {
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = result?.Description
                        });
                    }

                case BadRequestOperationResult<T> result:
                    {
                        return this.BadRequest(new ProblemDetails
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Detail = result?.Description
                        });
                    }

                case NotFoundOperationResult<T> result:
                    {
                        return this.NotFound(new ProblemDetails
                        {
                            Status = StatusCodes.Status404NotFound,
                            Detail = result?.Description
                        });
                    }

                default:
                    throw new System.InvalidOperationException();
            }
        }

        protected async Task NotifyEntityUpdatedAsync(string entityType, int? id, HubEntityOperation operation)
        {
            if (!id.HasValue || this.dataHubContext.Clients == null)
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

        protected async Task NotifyEntityUpdatedAsync(string entityType, HubEntityOperation operation)
        {
            await this.NotifyEntityUpdatedAsync(entityType, null, operation);
        }

        #endregion
    }
}
