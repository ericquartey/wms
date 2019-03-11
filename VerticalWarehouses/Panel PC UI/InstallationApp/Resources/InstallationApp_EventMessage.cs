using Ferretto.VW.InstallationApp.Resources.Enumerables;

namespace Ferretto.VW.InstallationApp.Resources
{
    public class InstallationApp_EventMessage
    {
        #region Constructors

        public InstallationApp_EventMessage(InstallationApp_EventMessageType type)
        {
            this.Type = type;
        }

        #endregion

        #region Properties

        public InstallationApp_EventMessageType Type { get; set; }

        #endregion
    }
}
