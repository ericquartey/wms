using System.Windows;

namespace Ferretto.VW.App.Keyboards
{
    public abstract class KeyboardKeyContainer
    {
        #region Properties

        public Thickness? KeyMargin { get; set; }

        public double? KeyMinWidth { get; set; }

        public Thickness? KeyPadding { get; set; }

        public string KeyStyleResource { get; set; }

        #endregion
    }
}
