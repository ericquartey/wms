using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.Controls.Services
{
    public class ModelChangedPubSubEvent<TModel, TKey> : Prism.Events.PubSubEvent, IPubSubEvent
        where TModel : IModel<TKey>
    {
        #region Fields

        private readonly TKey modelId;

        #endregion

        #region Constructors

        public ModelChangedPubSubEvent(TKey modelId)
        {
            this.modelId = modelId;
        }

        #endregion

        #region Properties

        public TKey ModelId => this.modelId;

        public string Token { get; }

        #endregion
    }
}
