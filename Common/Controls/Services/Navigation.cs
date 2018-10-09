using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class ItemChangedEvent<TModel, TId> : Prism.Events.PubSubEvent<TModel>, IEventArgs where TModel : IBusinessObject<TId>
    {
        #region Fields

        private readonly object itemId;

        #endregion Fields

        #region Constructors

        public ItemChangedEvent(object itemId)
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

        private readonly object itemId;
        private readonly string token;

        #endregion Fields

        #region Constructors

        public ItemSelectionChangedEvent(object itemId, string token)
        {
            this.token = token;
            this.itemId = itemId;
        }

        #endregion Constructors

        #region Properties

        public object ItemId => this.itemId;

        public string Token => this.token;

        #endregion Properties
    }

    public class RefreshItemsEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IEventArgs
    {
        #region Fields

        private readonly object itemId;

        #endregion Fields

        #region Constructors

        public RefreshItemsEvent(object itemId)
        {
            this.itemId = itemId;
        }

        #endregion Constructors

        #region Properties

        public object ItemId => this.itemId;

        public string Token => null;

        #endregion Properties
    }

    public class ShowDetailsEventArgs<TPayload, TId> : Prism.Events.PubSubEvent<TPayload>, IEventArgs where TPayload : IBusinessObject<TId>
    {
        #region Fields

        private readonly string token;

        #endregion Fields

        #region Constructors

        public ShowDetailsEventArgs(string token, bool isDetailsViewVisible)
        {
            this.token = token;
            this.IsDetailsViewVisible = isDetailsViewVisible;
        }

        #endregion Constructors

        #region Properties

        public bool IsDetailsViewVisible { get; private set; }

        public string Token => this.token;

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
