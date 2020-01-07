using System.Collections.Generic;
using Ferretto.VW.Devices.BarcodeReader;

namespace Ferretto.VW.App.Accessories
{
    public class UserActionEventArgs : ActionEventArgs
    {
        #region Constructors

        public UserActionEventArgs(string code, string userAction) : base(code)
        {
            this.UserAction = userAction;
        }

        #endregion

        #region Properties

        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        public string UserAction { get; }

        #endregion
    }
}
