using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Hubs.Models;

namespace Ferretto.WMS.App.Controls.Services
{
    public class ModelChangedPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Fields

        private readonly HubEntityOperation operationType;

        #endregion

        #region Constructors

        public ModelChangedPubSubEvent(
            string resourceName,
            string resourceId,
            HubEntityOperation operationType,
            string sourceResourceName,
            string sourceResourceId)
        {
            this.ResourceId = resourceId;
            this.ResourceName = resourceName;
            this.operationType = operationType;
            this.SourceResourceName = sourceResourceName;
            this.SourceResourceId = sourceResourceId;
        }

        #endregion

        #region Properties

        public HubEntityOperation OperationType => this.operationType;

        public string ResourceId { get; }

        public string ResourceName { get; }

        public string SourceResourceId { get; }

        public string SourceResourceName { get; }

        public string Token { get; }

        #endregion
    }
}
