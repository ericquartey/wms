using System;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Controls
{
    public static class PpcControlExtensions
    {
        public static TextBox FindTextBox(this PpcTextBox ppcTextbox)
            => (ppcTextbox ?? throw new ArgumentNullException(nameof(ppcTextbox))).FindName("InputTextBox") as TextBox;
    }
}
