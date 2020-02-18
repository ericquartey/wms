using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ferretto.VW.App.Keyboards;
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
            SaveFileDialog saveFileDialog = new SaveFileDialog
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
