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
            this.keyboard.KeyboardLayout = new KeyboardLayout
            {
                KeyMargin = new Thickness(5),
                KeyPadding = new Thickness(15, 5, 15, 5),
                Rows = new[]
                  {
                      new KeyboardRow
                      {
                          Cells = new[]
                          {
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = @"\" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "1" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "2" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "3" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "4" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "5" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "6" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "7" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "8" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "9" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "0" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "{Back}", Caption = "{icon:BackspaceSolid}" }}
                              },
                          }
                      },
                      new KeyboardRow
                      {
                          Cells = new[]
                          {
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "q" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "w" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "e" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "r" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "t" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "y" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "u" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "i" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "o" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "p" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "è" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "+" }}
                              },
                          }
                      },
                      new KeyboardRow
                      {
                          Cells = new[]
                          {
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "a" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "s" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "d" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "f" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "g" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "h" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "j" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "k" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "l" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "ò" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "à" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "ù" }}
                              },
                          }
                      },
                      new KeyboardRow
                      {
                          Cells = new[]
                          {
                              new KeyboardCell
                              {
                                  Width = new GridLength(2, GridUnitType.Star),
                                  Key = new KeyboardKey{
                                      Command = new KeyboardKeyCommand{ CommandText = "{layout:Uppercase}", Caption="{icon:CaretSquareUpSolid}" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "z" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "x" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "c" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "v" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "b" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "n" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "m" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "," }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "." }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ CommandText = "-" }}
                              },
                          }
                      }
                  }
            };
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
