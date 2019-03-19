using Ferretto.VW.OperatorApp.Resources.Enumerations;

namespace Ferretto.VW.OperatorApp.Resources
{
    public class OperatorApp_EventMessage
    {
        #region Constructors

        public OperatorApp_EventMessage(OperatorApp_EventMessageType type)
        {
            this.Type = type;
        }

        #endregion

        #region Properties

        public OperatorApp_EventMessageType Type { get; set; }

        #endregion
    }
}
