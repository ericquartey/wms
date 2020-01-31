using System;
using System.Collections.Generic;
using System.Windows;

namespace Ferretto.VW.App.Keyboards
{
    public class KeyboardRow : KeyboardKeyContainer
    {
        #region Properties

        public IEnumerable<KeyboardCell> Cells { get; set; } = Array.Empty<KeyboardCell>();

        public GridLength Height { get; set; } = GridLength.Auto;

        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;

        public VerticalAlignment VerticalAlignment { get; set; }

        #endregion
    }
}
