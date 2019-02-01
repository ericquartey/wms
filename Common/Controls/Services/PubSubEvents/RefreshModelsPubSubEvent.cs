using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class RefreshModelsPubSubEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IPubSubEvent
    {
        #region Fields

        private readonly object modelId;

        #endregion Fields

        #region Constructors

        public RefreshModelsPubSubEvent(int modelId)
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
