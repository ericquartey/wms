using System.Windows.Input;

namespace Ferretto.VW.App.Keyboards.Controls
{
    public class KeyboardCommandEventArgs
    {
        #region Constructors

        public KeyboardCommandEventArgs(Key key, string text)
        {
            this.CommandKey = key;
            this.CommandText = text;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="Key"/> sent by the keyboard, if any.
        /// </summary>
        public Key CommandKey { get; }

        /// <summary>
        /// Gets the text sent by the keyboard, if any.
        /// </summary>
        public string CommandText { get; }

        #endregion
    }
}
