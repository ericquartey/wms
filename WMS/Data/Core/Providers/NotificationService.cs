using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.Hubs.Models;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly IHubContext<DataHub, IDataHub> hubContext;

        #endregion

        #region Constructors

        public NotificationService(IHubContext<DataHub, IDataHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        #endregion

        #region Properties

        protected ISet<Notification> Notifications { get; } = new HashSet<Notification>();

        #endregion

        #region Methods

        public void Clear()
        {
            this.Notifications.Clear();
        }

        public void PushCreate(Type modelType)
        {
            this.Push(null, modelType, HubEntityOperation.Created);
        }

        public void PushCreate<TKey>(IModel<TKey> model)
        {
            this.Push(model, HubEntityOperation.Created);
        }

        public void PushDelete(Type modelType)
        {
            this.Push(null, modelType, HubEntityOperation.Deleted);
        }

        public void PushDelete<TKey>(IModel<TKey> model)
        {
            this.Push(model, HubEntityOperation.Deleted);
        }

        public void PushUpdate(Type modelType)
        {
            this.Push(null, modelType, HubEntityOperation.Updated);
        }

        public void PushUpdate<TKey>(IModel<TKey> model)
        {
            this.Push(model, HubEntityOperation.Updated);
        }

        public virtual async Task SendNotificationsAsync()
        {
            if (this.hubContext.Clients == null)
            {
                this.Notifications.Clear();
                return;
            }

            foreach (var notification in this.Notifications)
            {
                var attribute = notification.ModelType
                    .GetCustomAttributes(typeof(ResourceAttribute), true)
                    .FirstOrDefault() as ResourceAttribute;

                if (attribute == null)
                {
                    continue;
                }

                var eventDetails = new EntityChangedHubEvent
                {
                    Id = notification.ModelId,
                    EntityType = attribute.ResourceName,
                    Operation = notification.OperationType,
                };

                await this.hubContext.Clients.All.EntityUpdated(eventDetails);
            }

            this.Notifications.Clear();
        }

        private void Push<TKey>(IModel<TKey> model, HubEntityOperation operationType)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            this.Push(model.Id.ToString(), model.GetType(), operationType);
        }

        private void Push(string modelId, Type modelType, HubEntityOperation operationType)
        {
            this.Notifications.Add(new Notification(modelId, modelType, operationType));
        }

        #endregion
    }
}
