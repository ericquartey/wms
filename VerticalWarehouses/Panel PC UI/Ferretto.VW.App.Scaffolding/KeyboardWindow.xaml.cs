using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Ferretto.VW.App.Scaffolding
{
    /// <summary>
    /// Interaction logic for KeyboardWindow.xaml
    /// </summary>
    public partial class KeyboardWindow : Window
    {
        #region Constructors

        public KeyboardWindow()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = "layout.json",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, this.keyboard.KeyboardLayout.ToJson());
            }
        }

        private void Keyboard_LayoutChangeRequest(object sender, Keyboards.Controls.KeyboardLayoutChangeRequestEventArgs e)
        {
        }

        #endregion
    }
}
