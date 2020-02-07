using System;
using System.Collections.Generic;
using System.Globalization;
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

        private readonly Type _outputType;

        private readonly DependencyProperty _property;

        #endregion

        #region Constructors

        public PpcKeyboards(Control control, DependencyProperty dependencyProperty) : this(control, dependencyProperty, dependencyProperty?.PropertyType)
        {
        }

        public PpcKeyboards(Control control, DependencyProperty dependencyProperty, Type outputType) : this()
        {
            this._ctrl = control ?? throw new ArgumentNullException(nameof(control));
            this._property = dependencyProperty ?? throw new ArgumentNullException(nameof(dependencyProperty));
            this._outputType = outputType ?? throw new ArgumentNullException(nameof(outputType));

            var srcValidationRules = control.GetBindingExpression(dependencyProperty)?.ParentBinding.ValidationRules;
            var tgetRules = this.textBox.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.ValidationRules;
            if (srcValidationRules?.Count > 0)
            {
                if (tgetRules != null)
                {
                    foreach (var rule in srcValidationRules)
                    {
                        tgetRules.Add(rule);
                    }
                }
            }

            if (outputType != typeof(object) && outputType != typeof(string))
            {
                tgetRules.Add(new TypePreservingValidationRule(outputType));
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
            this.Unloaded += this.PpcKeyboards_Unloaded;
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
                if (!this.ViewModel.IsValid)
                {
                    // trying to commit an invalid value, exit.
                    return;
                }

                if (this._ctrl != null)
                {
                    IFormatProvider culture = this.Language.GetEquivalentCulture();
                    this._ctrl.SetValue(this._property, Convert.ChangeType(this.textBox.Text, this._outputType, culture));
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
            if (this.textBox.Language?.GetEquivalentCulture() != this.Language.GetEquivalentCulture())
            {
                this.textBox.Language = this.Language;
            }
            this.textBox.TextChanged += this.TextBox_TextChanged;
        }

        private void PpcKeyboards_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= this.PpcKeyboards_Unloaded;
            this.textBox.TextChanged -= this.TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.IsValid = !Validation.GetHasError((DependencyObject)sender);
        }

        #endregion

        #region Classes

        private class TypePreservingValidationRule : ValidationRule
        {
            #region Fields

            private readonly Type _type;

            #endregion

            #region Constructors

            public TypePreservingValidationRule(Type type)
            {
                this._type = type;
            }

            #endregion

            #region Methods

            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                try
                {
                    Convert.ChangeType(value, this._type, cultureInfo);
                    return ValidationResult.ValidResult;
                }
                catch (Exception exc)
                {
                    return new ValidationResult(false, exc.Message);
                }
            }

            #endregion
        }

        #endregion
    }
}
