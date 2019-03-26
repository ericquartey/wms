using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.Controls.Services
{
#pragma warning disable S2326 // Unused type parameters should be removed: in this case, the TModel parameter is used as a filter by the EventService bus

    public class ModelChangedPubSubEvent<TModel, TKey> : Prism.Events.PubSubEvent, IPubSubEvent
#pragma warning restore S2326 // Unused type parameters should be removed
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
