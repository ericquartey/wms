using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class ItemChangedEvent<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs where TPayload : IBusinessObject
    {
        #region Constructors

        public ItemChangedEvent(TPayload changedItem)
        {
            this.ChangedItem = changedItem;
        }

        #endregion Constructors

        #region Properties

        public TPayload ChangedItem { get; set; }

        public string Token => null;

        #endregion Properties
    }

    public class ItemSelectionChangedEvent<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs where TPayload : IBusinessObject
    {
        #region Fields

        private readonly string token;

        #endregion Fields

        #region Constructors

        public ItemSelectionChangedEvent(string token, TPayload selectedItem)
        {
            this.token = token;
            this.SelectedItem = selectedItem;
        }

        #endregion Constructors

        #region Properties

        public TPayload SelectedItem { get; private set; }

        public string Token => this.token;

        #endregion Properties
    }

    public class RefreshItemsEvent<TItem> : Prism.Events.PubSubEvent<TItem>, IEventArgs
    {
        #region Properties

        public TItem Item { get; set; }

        public string Token => null;

        #endregion Properties
    }

    public class ShowDetailsEventArgs<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs where TPayload : IBusinessObject
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

    public class StatusEvent : Prism.Events.PubSubEvent, IEventArgs
    {
        #region Constructors

        public StatusEvent(string info)
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
