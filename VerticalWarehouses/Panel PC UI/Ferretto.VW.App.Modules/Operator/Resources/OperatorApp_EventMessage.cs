using Ferretto.VW.App.Modules.Operator.Resources.Enumerations;

namespace Ferretto.VW.App.Modules.Operator.Resources
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
