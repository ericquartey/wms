using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class ActionUpdatedEventArgs
    {
        #region Constructors

        public ActionUpdatedEventArgs(ActionUpdateData data)
        {
            this.Data = data;
        }

        #endregion

        #region Properties

        public ActionUpdateData Data { get; set; }

        #endregion
    }
}
