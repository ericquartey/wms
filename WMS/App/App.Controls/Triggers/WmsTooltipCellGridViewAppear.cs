using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;

namespace Ferretto.WMS.App.Controls
{
    public class WmsTooltipCellGridViewAppear : TriggerAction<ContentElement>
    {
        #region Fields

        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register(
            nameof(Tooltip),
            typeof(string),
            typeof(WmsTooltipCellGridViewAppear));

        #endregion

        #region Properties

        public string Tooltip
        {
            get => (string)this.GetValue(TooltipProperty);
            set => this.SetValue(TooltipProperty, value);
        }

        #endregion

        #region Methods

        protected override void Invoke(object parameter)
        {
            if (!(parameter is ItemClickEventArgs clickEventArgs) ||
                !(clickEventArgs.Source is ActionBarItem actionBarItem))
            {
                return;
            }

            var mainGrid = LayoutTreeHelper.GetVisualParents(actionBarItem)
                .OfType<GridControl>()
                .FirstOrDefault();

            if (mainGrid == null)
            {
                var gridParent = LayoutTreeHelper.GetVisualParents(actionBarItem)
                    .OfType<Grid>()
                    .FirstOrDefault();

                mainGrid = LayoutTreeHelper.GetVisualChildren(gridParent)
                    .OfType<GridControl>()
                    .FirstOrDefault();
            }

            if (mainGrid == null)
            {
                return;
            }

            var selectionGrid = mainGrid;
            if (!mainGrid.GetSelectedRowHandles().Any())
            {
                selectionGrid = mainGrid.View.FocusedView.DataControl as GridControl;
            }

            if (selectionGrid?.GetSelectedRowHandles().Count() != 1)
            {
                return;
            }

            var cell = selectionGrid.View.GetCellElementByRowHandleAndColumn(
                selectionGrid.GetSelectedRowHandles().FirstOrDefault(),
                selectionGrid.Columns[this.Tooltip]);
            if (cell == null)
            {
                return;
            }

            var toolTip = new ToolTip();
            if (cell.ToolTip is ToolTip cellTooltip)
            {
                toolTip.Content = cellTooltip.Content;
                toolTip.ContentTemplate = cellTooltip.ContentTemplate;
                toolTip.PlacementTarget = cell;
                toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                toolTip.StaysOpen = false;
                toolTip.IsOpen = true;
            }
        }

        #endregion
    }
}
