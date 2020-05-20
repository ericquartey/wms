using System;

namespace Ferretto.VW.CommonUtils
{
    public class ActionEventArgs
    {
        #region Constructors

        public ActionEventArgs(string code)
        {
            if (code is null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            this.Code = code;
        }

        #endregion

        #region Properties

        public string Code { get; }

        #endregion
    }
}
