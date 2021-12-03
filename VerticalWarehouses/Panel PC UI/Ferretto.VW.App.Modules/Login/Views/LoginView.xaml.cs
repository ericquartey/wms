using System;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls.Keyboards;
using Ferretto.VW.App.Controls.Keyboards;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Modules.Login.Views
{
    public partial class LoginView
    {
        #region Fields

        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        #endregion

        #region Constructors

        public LoginView()
        {
            this.InitializeComponent();

            this.DispatcherTimer_Tick(null, null);
        }

        #endregion

        #region Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.dispatcherTimer.Tick += this.DispatcherTimer_Tick;
            this.dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            this.dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.txtTime.Text = string.Format(Localized.Instance.CurrentCulture, "{0:f}", DateTime.Now);
        }

        private void OnKeyboardOpenHandler(object sender, InputEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void OnKeyboardOpenHandlerOld(object sender, InputEventArgs e)
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

        private void OpenKeyboard()
        {
            var txt = this.txtPassword;
            if (this.IsEnabled && !txt.IsReadOnly && txt.IsEnabled)
            {
                txt.PopupKeyboard(DevExpress.Xpf.Editors.PasswordBoxEdit.TextProperty, typeof(string), isPassword: true, caption: "Password", timeout: TimeSpan.FromSeconds(60));
            }
        }

        private void OpenKeyboard_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void OpenKeyboard_TouchUp(object sender, TouchEventArgs e)
        {
            this.OpenKeyboard();
        }

        #endregion
    }
}
