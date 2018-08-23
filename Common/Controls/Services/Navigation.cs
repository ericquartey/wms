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
}
