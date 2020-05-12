using System.Windows;

namespace Ferretto.VW.App.Keyboards
{
    /// <summary>
    /// Defines a zone in the keyboard's layout (Grid).
    /// </summary>
    public class KeyboardZone
    {
        #region Properties

        public int Column { get; set; }

        public int ColumnSpan { get; set; } = 1;

        /// <summary>
        /// Gets or sets the zone identifier that has to be matched by any <see cref="KeyboardSet.Zone"/>.
        /// </summary>
        public string Id { get; set; }

        public int Row { get; set; }

        public int RowSpan { get; set; } = 1;

        /// <summary>
        /// Gets or sets the width of the GridColumn containing this set of Cells. Defaults to 1*.
        /// </summary>
        public GridLength? Width { get; set; }

        #endregion
    }
}
