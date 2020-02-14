using System.Windows;

namespace Ferretto.VW.App.Keyboards
{
    public class KeyboardCell : KeyboardKeyContainer
    {
        #region Properties

        public KeyboardKey Key { get; set; }

        public GridLength? Width { get; set; }

        #endregion
    }
}
