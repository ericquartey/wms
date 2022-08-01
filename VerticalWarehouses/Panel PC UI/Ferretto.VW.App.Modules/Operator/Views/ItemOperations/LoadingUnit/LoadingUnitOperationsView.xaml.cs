using System;
using System.Windows.Controls;
using Ferretto.VW.App.Controls.Keyboards;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class LoadingUnitOperationsView : UserControl
    {
        #region Constructors

        public LoadingUnitOperationsView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void DataGrid2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataGrid2.SelectedItem != null)
            {
                this.DataGrid2.ScrollIntoView(this.DataGrid2.SelectedItem);
            }
        }

        private void KeyboardButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OpenKeybard();
        }

        private void KeyboardButton_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            this.OpenKeybard();
        }

        private void OpenKeybard()
        {
            if (this.IsEnabled && this.SearchText.IsEnabled && !this.SearchText.IsReadOnly)
            {
                this.SearchText.PopupKeyboard(caption: Localized.Get("OperatorApp.ItemSearchKeySearch"), timeout: TimeSpan.FromSeconds(60));
            }
        }

        #endregion
    }
}
