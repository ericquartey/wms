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

        public void PushCreate<TKeySource>(Type modelType, IModel<TKeySource> sourceModel)
        {
            if (sourceModel == null)
            {
                throw new ArgumentNullException(nameof(sourceModel));
            }

            this.Push(null, modelType, sourceModel.Id.ToString(), sourceModel.GetType(), HubEntityOperation.Created);
        }

        public void PushCreate(Type modelType, string sourceModelId, Type sourceModelType)
        {
            this.Push(null, modelType, sourceModelId, sourceModelType, HubEntityOperation.Created);
        }

        public void PushCreate<TKey, TKeySource>(IModel<TKey> model, IModel<TKeySource> sourceModel)
        {
            this.Push(model, sourceModel, HubEntityOperation.Created);
        }

        public void PushCreate<TKey>(IModel<TKey> model)
        {
            this.Push(model, HubEntityOperation.Created);
        }

        public void PushDelete<TKeySource>(Type modelType, IModel<TKeySource> sourceModel)
        {
            if (sourceModel == null)
            {
                throw new ArgumentNullException(nameof(sourceModel));
            }

            this.Push(null, modelType, sourceModel.Id.ToString(), sourceModel.GetType(), HubEntityOperation.Deleted);
        }

        public void PushDelete(Type modelType, string sourceModelId, Type sourceModelType)
        {
            this.Push(null, modelType, sourceModelId, sourceModelType, HubEntityOperation.Deleted);
        }

        public void PushDelete<TKey, TKeySource>(IModel<TKey> model, IModel<TKeySource> sourceModel)
        {
            this.Push(model, sourceModel, HubEntityOperation.Deleted);
        }

        public void PushDelete<TKey>(IModel<TKey> model)
        {
            this.Push(model, HubEntityOperation.Deleted);
        }

        public void PushUpdate(Type modelType, string sourceModelId, Type sourceModelType)
        {
            this.Push(null, modelType, sourceModelId, sourceModelType, HubEntityOperation.Updated);
        }

        public void PushUpdate<TKeySource>(Type modelType, IModel<TKeySource> sourceModel)
        {
            if (sourceModel == null)
            {
                throw new ArgumentNullException(nameof(sourceModel));
            }

            this.Push(null, modelType, sourceModel.Id.ToString(), sourceModel.GetType(), HubEntityOperation.Updated);
        }

        public void PushUpdate<TKey, TKeySource>(IModel<TKey> model, IModel<TKeySource> sourceModel)
        {
            this.Push(model, sourceModel, HubEntityOperation.Updated);
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
                var targetAttribute = notification.ModelType
                    .GetCustomAttributes(typeof(ResourceAttribute), true)
                    .FirstOrDefault() as ResourceAttribute;

                var sourceAttribute = notification.SourceModelType
                    .GetCustomAttributes(typeof(ResourceAttribute), true)
                    .FirstOrDefault() as ResourceAttribute;

                if (targetAttribute == null || sourceAttribute == null)
                {
                    continue;
                }

                var eventDetails = new EntityChangedHubEvent
                {
                    Id = notification.ModelId,
                    EntityType = targetAttribute.ResourceName,
                    Operation = notification.OperationType,
                    SourceId = notification.SourceModelId,
                    SourceEntityType = sourceAttribute.ResourceName,
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

        private void Push<TKey, TKeySource>(IModel<TKey> model, IModel<TKeySource> sourceModel, HubEntityOperation operationType)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            this.Push(model.Id.ToString(), model.GetType(), sourceModel.Id.ToString(), sourceModel.GetType(), operationType);
        }

        private void Push(string modelId, Type modelType, HubEntityOperation operationType)
        {
            this.Notifications.Add(new Notification(modelId, modelType, modelId, modelType, operationType));
        }

        private void Push(string modelId, Type modelType, string sourceModelId, Type sourceModelType, HubEntityOperation operationType)
        {
            this.Notifications.Add(new Notification(modelId, modelType, sourceModelId, sourceModelType, operationType));
        }

        #endregion
    }
}
