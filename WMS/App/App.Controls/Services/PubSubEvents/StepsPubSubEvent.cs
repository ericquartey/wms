using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.App.Controls.Services
{
    public class StepsPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Constructors

        public StepsPubSubEvent(CommandExecuteType commandExecute)
        {
            this.CommandExecute = commandExecute;
        }

        #endregion

        #region Properties

        public bool CanExecute { get; set; }

        public CommandExecuteType CommandExecute { get; set; }

        public string Token { get; }

        #endregion
    }
}
