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

namespace Ferretto.VW.App.Controls.Controls.Keyboards
{
    /// <summary>
    /// Interaction logic for PpcKeyboards.xaml
    /// </summary>
    public partial class PpcKeyboards : PpcDialogView
    {
        #region Fields

        private readonly TextBox _textBox;

        #endregion

        #region Constructors

        public PpcKeyboards(TextBox textBox) : this()
        {
            this._textBox = textBox;
            var srcValidationRules = this.textBox.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.ValidationRules;
            if (srcValidationRules?.Count > 0)
            {
                var tgetRules = this.textBox.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.ValidationRules;
                if (tgetRules != null)
                {
                    foreach (var rule in srcValidationRules)
                    {
                        tgetRules.Add(rule);
                    }
                }
            }
        }

        public PpcKeyboards()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void Keyboard_KeyboardCommand(object sender, App.Keyboards.Controls.KeyboardCommandEventArgs e)
        {
            if (e.CommandKey == System.Windows.Input.Key.Enter)
            {
                if (this._textBox != null)
                {
                    this._textBox.Text = this.textBox.Text;
                }
                this.Hide();
            }
            else if (e.CommandKey == System.Windows.Input.Key.Escape)
            {
                this.Hide();
            }
        }

        #endregion
    }
}
