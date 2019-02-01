using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class ModelSelectionChangedPubSubEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IPubSubEvent
        where TModel : IBusinessObject
    {
        #region Fields

        private readonly int? modelId;

        private readonly string token;

        #endregion Fields

        #region Constructors

        public ModelSelectionChangedPubSubEvent(int? modelId, string token)
        {
            this.modelId = modelId;
            this.token = token;
        }

        #endregion Constructors

        #region Properties

        public int? ModelId => this.modelId;

        public string Token => this.token;

        #endregion Properties
    }
}
