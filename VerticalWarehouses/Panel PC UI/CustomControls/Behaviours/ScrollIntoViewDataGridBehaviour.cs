using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Ferretto.VW.App.Controls.Behaviours
{
    public class ScrollIntoViewDataGridBehaviour : Behavior<DataGrid>
    {
        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private static void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid
                &&
                dataGrid?.SelectedItem != null)
            {
                dataGrid.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        dataGrid.UpdateLayout();
                        if (dataGrid.SelectedItem != null)
                        {
                            dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                        }
                    }));
            }
        }

        #endregion
    }
}
