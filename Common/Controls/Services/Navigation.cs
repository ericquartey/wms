using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class ItemChangedEvent<TModel, TId> : Prism.Events.PubSubEvent<TModel>, IEventArgs where TModel : IBusinessObject<TId>
    {
        #region Fields

        private readonly object itemId;

        #endregion Fields

        #region Constructors

        public ItemChangedEvent(TId itemId)
        {
            this.itemId = itemId;
        }

        #endregion Constructors

        #region Properties

        public object ItemId => this.itemId;

        public string Token => null;

        #endregion Properties
    }

    public class ItemSelectionChangedEvent<TModel, TId> : Prism.Events.PubSubEvent<TModel>, IEventArgs where TModel : IBusinessObject<TId>
    {
        #region Fields

        private readonly TId modelId;
        private readonly string token;

        #endregion Fields

        #region Constructors

        public ItemSelectionChangedEvent(TId modelId, string token)
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

    public class RefreshItemsEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IEventArgs
    {
        #region Fields

        private readonly object itemId;

        #endregion Fields

        #region Constructors

        public RefreshItemsEvent(int itemId)
        {
            this.itemId = itemId;
        }

        #endregion Constructors

        #region Properties

        public object ItemId => this.itemId;

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
