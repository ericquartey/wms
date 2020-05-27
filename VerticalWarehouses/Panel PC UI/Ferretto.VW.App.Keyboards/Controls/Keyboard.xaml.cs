using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.App.Keyboards.Controls
{
    public partial class Keyboard : UserControl
    {
        #region Fields

        public static readonly DependencyProperty KeyboardLayoutProperty =
            DependencyProperty.Register(nameof(KeyboardLayout), typeof(KeyboardLayout), typeof(Keyboard), new PropertyMetadata(OnKeyboardLayoutPropertyChanged));

        #endregion

        #region Constructors

        public Keyboard()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Events

        public event EventHandler<KeyboardCommandEventArgs> KeyboardCommand;

        public event EventHandler<KeyboardLayoutChangeRequestEventArgs> KeyboardLayoutChangeRequest;

        #endregion

        #region Properties

        public KeyboardLayout KeyboardLayout
        {
            get => (KeyboardLayout)this.GetValue(KeyboardLayoutProperty);
            set => this.SetValue(KeyboardLayoutProperty, value);
        }

        #endregion

        #region Methods

        internal void FireKeyboardCommand(Key key, string text) => this.OnKeyboardCommand(new KeyboardCommandEventArgs(key, text));

        internal void RequestLayoutChange(string layoutCode) => this.OnKeyboardLayoutChangeRequest(new KeyboardLayoutChangeRequestEventArgs(layoutCode));

        protected virtual void OnKeyboardCommand(KeyboardCommandEventArgs e)
        {
            this.KeyboardCommand?.Invoke(this, e);
        }

        protected virtual void OnKeyboardLayoutChangeRequest(KeyboardLayoutChangeRequestEventArgs e)
        {
            this.KeyboardLayoutChangeRequest?.Invoke(this, e);
        }

        private static void OnKeyboardLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Keyboard)d).OnKeyboardLayoutChanged(e.OldValue as KeyboardLayout, e.NewValue as KeyboardLayout);
        }

        private void OnKeyboardLayoutChanged(KeyboardLayout old, KeyboardLayout val)
        {
            this.keyboard.GenerateKeyboard(val);
        }

        #endregion
    }
}
