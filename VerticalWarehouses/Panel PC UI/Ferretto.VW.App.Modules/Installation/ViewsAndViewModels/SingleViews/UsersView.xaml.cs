using System;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Editors;
using Ferretto.VW.App.Controls.Keyboards;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Installation.Views
{
    public partial class UsersView
    {
        #region Constructors

        public UsersView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void KeyboardButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var keyboardButton = sender as Button;
            this.SelectPasswordBoxEditFromButton(keyboardButton);
        }

        private void KeyboardButton_TouchUp(object sender, TouchEventArgs e)
        {
            var keyboardButton = sender as Button;
            this.SelectPasswordBoxEditFromButton(keyboardButton);
        }

        private void OnKeyboardOpenHandler(object sender, InputEventArgs e)
        {
            var passwordbox = sender as PasswordBoxEdit;
            this.SelectPasswordBoxEdit(passwordbox);
        }

        private void OpenKeyboard(PasswordBoxEdit passwordBoxEdit, string text)
        {
            if (this.IsEnabled && !passwordBoxEdit.IsReadOnly && passwordBoxEdit.IsEnabled)
            {
                passwordBoxEdit.PopupKeyboard(PasswordBoxEdit.TextProperty, typeof(string), isPassword: true, caption: text, timeout: TimeSpan.FromSeconds(60));
            }
        }

        private void SelectPasswordBoxEdit(PasswordBoxEdit passwordBoxEdit)
        {
            if (passwordBoxEdit == this.NewInstallerPassword)
            {
                this.OpenKeyboard(this.NewInstallerPassword, Localized.Get("InstallationApp.NewPassword"));
            }
            else if (passwordBoxEdit == this.ConfirmNewInstallerPassword)
            {
                this.OpenKeyboard(this.ConfirmNewInstallerPassword, Localized.Get("InstallationApp.ConfirmNewPassword"));
            }
            else if (passwordBoxEdit == this.NewOperatorPassword)
            {
                this.OpenKeyboard(this.NewOperatorPassword, Localized.Get("InstallationApp.NewPassword"));
            }
            else if (passwordBoxEdit == this.ConfirmNewOperatorPassword)
            {
                this.OpenKeyboard(this.ConfirmNewOperatorPassword, Localized.Get("InstallationApp.ConfirmNewPassword"));
            }
        }

        private void SelectPasswordBoxEditFromButton(Button button)
        {
            if (button == this.KeyboardButtonNewInstallerPassword)
            {
                this.OpenKeyboard(this.NewInstallerPassword, Localized.Get("InstallationApp.NewPassword"));
            }
            else if (button == this.KeyboardButtonConfirmNewInstallerPassword)
            {
                this.OpenKeyboard(this.ConfirmNewInstallerPassword, Localized.Get("InstallationApp.ConfirmNewPassword"));
            }
            else if (button == this.KeyboardButtonNewOperatorPassword)
            {
                this.OpenKeyboard(this.NewOperatorPassword, Localized.Get("InstallationApp.NewPassword"));
            }
            else if (button == this.KeyboardButtonConfirmNewOperatorPassword)
            {
                this.OpenKeyboard(this.ConfirmNewOperatorPassword, Localized.Get("InstallationApp.ConfirmNewPassword"));
            }
        }

        #endregion
    }
}
