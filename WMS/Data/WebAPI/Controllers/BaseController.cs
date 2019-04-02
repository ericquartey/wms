using System.Threading.Tasks;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Hubs;
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
