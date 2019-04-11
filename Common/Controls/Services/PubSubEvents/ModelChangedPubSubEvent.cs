using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Hubs;

namespace Ferretto.WMS.App.Controls.Services
{
    public class ModelChangedPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Fields

        private readonly HubEntityOperation operationType;

        private readonly object resourceId;

        private readonly string resourceName;

        #endregion

        #region Constructors

        public ModelChangedPubSubEvent(
            string resourceName,
            object resourceId,
            HubEntityOperation operationType)
        {
            this.resourceId = resourceId;
            this.resourceName = resourceName;
            this.operationType = operationType;
        }

        #endregion

        #region Properties

        public HubEntityOperation OperationType => this.operationType;

        public object ResourceId => this.resourceId;

        public string ResourceName => this.resourceName;

        public string Token { get; }

        #endregion
    }
}
