using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.Controls
{
    public class AutoResizedGridView : GridView
    {
        protected override void PrepareItem(ListViewItem item)
        {
            // double availWidth = item.ActualWidth;
            foreach (var column in this.Columns)
            {
                // Setting NaN for the column width automatically determines the required
                // width enough to hold the content completely.

                // If the width is NaN, first set it to ActualWidth temporarily.
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                }

                // Finally, set the column with to NaN. This raises the property change
                // event and re computes the width.
                column.Width = double.NaN;
            }

            base.PrepareItem(item);
        }
    }
}
