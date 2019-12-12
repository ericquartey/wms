using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls.Keyboards;

namespace Ferretto.VW.App.Modules.Login.Views
{
    public partial class LoginView
    {
        #region Constructors

        public LoginView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void OnKeyboardOpenHandler(object sender, InputEventArgs e)
        {
            var ppcKeyboard = new PpcKeyboard();
            var vmKeyboard = new PpcKeypadsPopupViewModel();
            ppcKeyboard.DataContext = vmKeyboard;
            vmKeyboard.Update("Password", this.txtPassword.Password);
            ppcKeyboard.Topmost = false;
            ppcKeyboard.ShowInTaskbar = false;
            PpcMessagePopup.ShowDialog(ppcKeyboard);
            this.txtPassword.Password = vmKeyboard.ScreenText;
        }

        #endregion
    }
}
