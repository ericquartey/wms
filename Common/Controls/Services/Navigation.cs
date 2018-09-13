using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class ItemSelectionChangedEvent<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs
    {
        public TPayload SelectedItem { get; private set; }

        public string Token => null;

        public ItemSelectionChangedEvent(TPayload selectedItem)
        {
            this.SelectedItem = selectedItem;
        }
    }

    public class ShowDetailsEventArgs<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs
    {
        public string Token => null;

        public bool IsDetailsViewVisible { get; private set; }

        public ShowDetailsEventArgs(bool isDetailsViewVisible)
        {
            this.IsDetailsViewVisible = isDetailsViewVisible;
        }
    }

    public class RefreshItemsEvent : Prism.Events.PubSubEvent, IEventArgs
    {
        public string Token => null;
    }

    public class ItemChangedEvent<TPayload> : Prism.Events.PubSubEvent<TPayload>, IEventArgs
    {
        public TPayload ChangedItem { get; set; }

        public string Token => null;

        public ItemChangedEvent(TPayload changedItem)
        {
            this.ChangedItem = changedItem;
        }
    }
}
