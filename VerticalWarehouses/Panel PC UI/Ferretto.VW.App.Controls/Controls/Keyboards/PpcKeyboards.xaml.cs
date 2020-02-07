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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ferretto.VW.App.Controls.Keyboards
{
    public partial class PpcKeyboards : PpcDialogView
    {
        #region Fields

        private readonly Control _ctrl;

        private readonly DependencyProperty _property;

        #endregion

        #region Constructors

        public PpcKeyboards(Control tget, DependencyProperty prop) : this()
        {
            this._ctrl = tget ?? throw new ArgumentNullException(nameof(tget));
            this._property = prop;
            var srcValidationRules = tget.GetBindingExpression(prop)?.ParentBinding.ValidationRules;
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

        public PpcKeyboards(TextBox tget) : this(tget, TextBox.TextProperty)
        {
        }

        public PpcKeyboards()
        {
            this.Language = XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            this.InitializeComponent();
            this.textBox.Focus();
            this.Loaded += this.PpcKeyboards_Loaded;
        }

        #endregion

        #region Properties

        private PpcKeyboardsViewModel ViewModel => this.DataContext as PpcKeyboardsViewModel;

        #endregion

        #region Methods

        private void Keyboard_KeyboardCommand(object sender, App.Keyboards.Controls.KeyboardCommandEventArgs e)
        {
            // reset inactive timeout
            this.ViewModel.ResetTimer();

            // assign
            if (e.CommandKey == System.Windows.Input.Key.Enter)
            {
                if (this._ctrl != null)
                {
                    this._ctrl.SetValue(this._property, Convert.ChangeType(this.textBox.Text, this._property.PropertyType));
                }
            }

            // close
            if (e.CommandKey == System.Windows.Input.Key.Escape
                || e.CommandKey == System.Windows.Input.Key.Enter)
            {
                this.ViewModel.IsClosed = true;
            }
        }

        private void Keyboard_KeyboardLayoutChangeRequest(object sender, App.Keyboards.Controls.KeyboardLayoutChangeRequestEventArgs e)
        {
            // reset inactive timeout
            this.ViewModel.ResetTimer();

            this.ViewModel.KeyboardLayoutCode = e.LayoutCode;
        }

        private void PpcKeyboards_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.PpcKeyboards_Loaded;
            this.textBox.SelectAll();
        }

        #endregion
    }
}
