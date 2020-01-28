using System;
using System.Collections.Generic;
using System.Windows;

namespace Ferretto.VW.App.Keyboards
{
    public class KeyboardRow
    {
        #region Properties

        public IEnumerable<KeyboardCell> Cells { get; set; } = Array.Empty<KeyboardCell>();

        public GridLength Height { get; set; } = GridLength.Auto;

        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;

        public Thickness? KeyMargin { get; set; }

        public Thickness? KeyPadding { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        #endregion
    }
}
