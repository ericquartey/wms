namespace Ferretto.Common.Controls
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interactivity;

    public class WmsTooltipViewAppear : TriggerAction<ContentElement>
    {
        #region Fields

        public static readonly DependencyProperty GridProperty = DependencyProperty.Register(nameof(Grid), typeof(object), typeof(WmsTooltipViewAppear));

        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register(nameof(Tooltip), typeof(string), typeof(WmsTooltipViewAppear));

        #endregion

        #region Properties

        public WmsGridControl Grid
        {
            get => (WmsGridControl)this.GetValue(GridProperty);
            set => this.SetValue(GridProperty, value);
        }

        public string Tooltip
        {
            get => (string)this.GetValue(TooltipProperty);
            set => this.SetValue(TooltipProperty, value);
        }

        #endregion

        #region Methods

        protected override void Invoke(object parameter)
        {
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
