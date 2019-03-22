namespace Ferretto.Common.Controls
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interactivity;
    using DevExpress.Mvvm.UI;
    using DevExpress.Xpf.Bars;

    public class WmsTooltipViewAppear : TriggerAction<ContentElement>
    {
        #region Fields

        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register(nameof(Tooltip), typeof(string), typeof(WmsTooltipViewAppear));

        #endregion

        #region Properties

        public WmsGridControl Grid { get; set; }

        public string Tooltip
        {
            get => (string)this.GetValue(TooltipProperty);
            set => this.SetValue(TooltipProperty, value);
        }

        #endregion

        #region Methods

        protected override void Invoke(object inputArgs)
        {
            if (inputArgs is ItemClickEventArgs clickEventArgs)
            {
                if (clickEventArgs.Source is ActionBarItem actionBarItem)
                {
                    var gridParent = LayoutTreeHelper.GetVisualParents(actionBarItem)
                        .OfType<Grid>()
                        .FirstOrDefault();

                    this.Grid = LayoutTreeHelper.GetVisualChildren(gridParent)
                        .OfType<WmsGridControl>()
                        .FirstOrDefault();
                }
            }

            if (this.Grid?.GetSelectedRowHandles().Count() == 1)
            {
                var rowHandle = this.Grid.GetSelectedRowHandles().FirstOrDefault();

                var cell = this.Grid.View.GetCellElementByRowHandleAndColumn(rowHandle, this.Grid.Columns[this.Tooltip]);

                if (cell != null)
                {
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
            }
        }

        #endregion
    }
}
