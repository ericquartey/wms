using System;
using System.Windows.Input;
using DevExpress.Xpf.Editors;
using Ferretto.VW.App.Controls.Keyboards;

namespace Ferretto.VW.App.Installation.Views
{
    public partial class DatabaseBackupView
    {
        #region Constructors

        public DatabaseBackupView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        private void KeyboardButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OpenKeyboard((PasswordBoxEdit)sender);
        }

        private void KeyboardButton_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            this.OpenKeyboard((PasswordBoxEdit)sender);
        }

        private void OnKeyboardOpenHandler(object sender, InputEventArgs e)
        {
            this.OpenKeyboard((PasswordBoxEdit)sender);
        }

        private void OpenKeyboard(PasswordBoxEdit passwordBoxEdit)
        {
            if (this.IsEnabled && !passwordBoxEdit.IsReadOnly && passwordBoxEdit.IsEnabled)
            {
                passwordBoxEdit.PopupKeyboard(PasswordBoxEdit.TextProperty, typeof(string), isPassword: true, caption: "Password", timeout: TimeSpan.FromSeconds(60));
            }
        }

        #endregion Methods
    }
}
