using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class ModelChangedEvent<TModel, TId> : Prism.Events.PubSubEvent<TModel>, IEventArgs where TModel : IBusinessObject
    {
        #region Fields

        private readonly object modelId;

        #endregion Fields

        #region Constructors

        public ModelChangedEvent(TId modelId)
        {
            this.modelId = modelId;
        }

        #endregion Constructors

        #region Properties

        public object ModelId => this.modelId;

        public string Token => null;

        #endregion Properties
    }

    public class ModelSelectionChangedEvent<TModel, TId> : Prism.Events.PubSubEvent<TModel>, IEventArgs where TModel : IBusinessObject
    {
        #region Fields

        private readonly TId modelId;
        private readonly string token;

        #endregion Fields

        #region Constructors

        public ModelSelectionChangedEvent(TId modelId, string token)
        {
            this.modelId = modelId;
            this.token = token;
        }

        #endregion Constructors

        #region Properties

        public TId ModelId => this.modelId;
        public bool ModelIdHasValue => default(TId).Equals(this.modelId) == false;
        public string Token => this.token;

        #endregion Properties
    }

    public class RefreshModelsEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IEventArgs
    {
        #region Fields

        private readonly object modelId;

        #endregion Fields

        #region Constructors

        public RefreshModelsEvent(int modelId)
        {
            this.modelId = modelId;
        }

        #endregion Constructors

        #region Properties

        public object ModelId => this.modelId;

        public string Token => null;

        #endregion Properties
    }

    public class StatusEventArgs : Prism.Events.PubSubEvent, IEventArgs
    {
        #region Constructors

        public StatusEventArgs(string info)
        {
            this.Info = info;
        }

        #endregion Constructors

        #region Properties

        public string Info { get; set; }

        public string Token => null;

        #endregion Properties
    }
}
