using System;
using Ferretto.VW.App.Controls.Keyboards;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class AddMatrixView
    {
        #region Constructors

        public AddMatrixView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

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
