using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.App.Controls.Services
{
    public class ItemsPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Constructors

        public ItemsPubSubEvent(CommandExecuteType commandExecute)
        {
            this.CommandExecute = commandExecute;
        }

        #endregion

        #region Properties
        public string Token { get; }

        public bool CanExecute { get; set; }

        public CommandExecuteType CommandExecute { get; set; }

        #endregion
    }
}
