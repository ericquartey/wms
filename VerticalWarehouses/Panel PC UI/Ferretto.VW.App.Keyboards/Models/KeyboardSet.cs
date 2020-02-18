using System;
using System.Collections.Generic;
using System.Windows;

namespace Ferretto.VW.App.Keyboards
{
    /// <summary>
    /// Once deserialized, renders into a single-row Grid that contains <see cref="KeyboardCell"/> items.
    /// </summary>
    public class KeyboardSet : KeyboardKeyContainer
    {
        #region Properties

        public IEnumerable<KeyboardCell> Cells { get; set; } = Array.Empty<KeyboardCell>();

        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;

        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Stretch;

        /// <summary>
        /// Refers to the <see cref="KeyboardZone.Id"/> this set belongs to.
        /// </summary>
        public string Zone { get; set; }

        #endregion
    }
}
