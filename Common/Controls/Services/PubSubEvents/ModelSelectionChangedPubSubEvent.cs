using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.Controls.Services
{
    public class ModelSelectionChangedPubSubEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IPubSubEvent
        where TModel : IModel<int>
    {
        #region Fields

        private readonly int? modelId;

        private readonly string token;

        #endregion

        #region Constructors

        public ModelSelectionChangedPubSubEvent(int? modelId, string token)
        {
            this.modelId = modelId;
            this.token = token;
        }

        #endregion

        #region Properties

        public int? ModelId => this.modelId;

        public string Token => this.token;

        #endregion
    }
}
