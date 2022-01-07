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

        #endregion

        #region Methods

        private void KeyboardButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void KeyboardButton_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void OnKeyboardOpenHandler(object sender, InputEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void OpenKeyboard()
        {
            if (this.IsEnabled && !this.ServerPassword.IsReadOnly && this.ServerPassword.IsEnabled)
            {
                this.ServerPassword.PopupKeyboard(PasswordBoxEdit.TextProperty, typeof(string), isPassword: true, caption: "Password", timeout: TimeSpan.FromSeconds(60));
            }
        }

        #endregion
    }
}
