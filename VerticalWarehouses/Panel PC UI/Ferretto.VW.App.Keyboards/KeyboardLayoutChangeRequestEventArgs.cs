using System;

namespace Ferretto.VW.App.Keyboards
{
    public class KeyboardLayoutChangeRequestEventArgs : EventArgs
    {
        #region Constructors

        internal KeyboardLayoutChangeRequestEventArgs(string layoutCode)
        {
            this.LayoutCode = layoutCode;
        }

        #endregion

        #region Properties

        public string LayoutCode { get; }

        #endregion
    }
}
