using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class ItemChangedEvent<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs where TPayload : IBusinessObject
    {
        public ItemChangedEvent(TPayload changedItem)
        {
            this.ChangedItem = changedItem;
        }

        public TPayload ChangedItem { get; set; }

        public string Token => null;
    }

    public class ItemSelectionChangedEvent<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs where TPayload : IBusinessObject
    {
        public ItemSelectionChangedEvent(TPayload selectedItem)
        {
            this.SelectedItem = selectedItem;
        }

        public TPayload SelectedItem { get; private set; }

        public string Token => null;
    }

    public class RefreshItemsEvent<TItem> : Prism.Events.PubSubEvent<TItem>, IEventArgs
    {
        public TItem Item { get; set; }

        public string Token => null;
    }

    public class ShowDetailsEventArgs<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs where TPayload : IBusinessObject
    {
        public ShowDetailsEventArgs(bool isDetailsViewVisible)
        {
            this.IsDetailsViewVisible = isDetailsViewVisible;
        }

        public bool IsDetailsViewVisible { get; private set; }
        public string Token => null;
    }

    public class StatusEvent : Prism.Events.PubSubEvent, IEventArgs
    {
        public StatusEvent(string info)
        {
            this.Info = info;
        }

        public string Info { get; set; }

        public string Token => null;
    }
}
