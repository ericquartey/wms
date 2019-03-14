using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Interfaces;

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
