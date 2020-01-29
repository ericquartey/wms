using System;
using System.Collections.Generic;
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

            this.dotCom.Key = new Keyboards.KeyboardKey
            {
                Command = new Keyboards.KeyboardKeyCommand
                {
                    Command = ".com"
                }
            };
            this.backSpace.Key = new Keyboards.KeyboardKey
            {
                Command = new Keyboards.KeyboardKeyCommand
                {
                    Command = "{Back}",
                    Caption = "{icon:BackspaceSolid}"
                }
            };
        }

        #endregion

        #region Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.keyboard.GenerateKeyboard(new KeyboardLayout
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
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = @"\" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "1" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "2" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "3" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "4" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "5" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "6" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "7" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "8" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "9" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "0" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "{Back}", Caption = "{icon:BackspaceSolid}" }}
                              },
                          }
                      },
                      new KeyboardRow
                      {
                          Cells = new[]
                          {
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "q" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "w" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "e" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "r" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "t" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "y" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "u" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "i" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "o" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "p" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "è" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "+" }}
                              },
                          }
                      },
                      new KeyboardRow
                      {
                          Cells = new[]
                          {
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "a" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "s" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "d" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "f" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "g" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "h" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "j" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "k" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "l" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "ò" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "à" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "ù" }}
                              },
                          }
                      },
                      new KeyboardRow
                      {
                          Cells = new[]
                          {
                              //new KeyboardCell
                              //{
                              //    Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "{CapsLock}", Caption="{icon:CaretSquareUpSolid}" }}
                              //},
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "z" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "x" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "c" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "v" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "b" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "n" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "m" }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "," }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "." }}
                              },
                              new KeyboardCell
                              {
                                  Key = new KeyboardKey{ Command = new KeyboardKeyCommand{ Command = "-" }}
                              },
                          }
                      }
                  }
            });
        }

        #endregion
    }
}
