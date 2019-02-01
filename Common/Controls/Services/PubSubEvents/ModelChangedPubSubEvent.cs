using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class ModelChangedPubSubEvent<TModel> : Prism.Events.PubSubEvent, IPubSubEvent
        where TModel : IBusinessObject
    {
        #region Fields

        private readonly object modelId;

        #endregion Fields

        #region Constructors

        public ModelChangedPubSubEvent(object modelId)
        {
            this.modelId = modelId;
        }

        #endregion Constructors

        #region Properties

        public object ModelId => this.modelId;

        public string Token { get; }

        #endregion Properties
    }
}
