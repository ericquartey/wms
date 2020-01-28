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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.VW.App.Keyboards
{
    public partial class Keyboard : UserControl
    {
        #region Fields

        public static readonly DependencyProperty LayoutProperty =
            DependencyProperty.Register("Layout", typeof(KeyboardLayout), typeof(Keyboard), new PropertyMetadata(OnLayoutPropertyChanged));

        #endregion

        #region Constructors

        public Keyboard()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public KeyboardLayout Layout
        {
            get => (KeyboardLayout)this.GetValue(LayoutProperty);
            set => this.SetValue(LayoutProperty, value);
        }

        #endregion

        #region Methods

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Keyboard)d).OnLayoutChanged(e.OldValue as KeyboardLayout, e.NewValue as KeyboardLayout);
        }

        private void OnLayoutChanged(KeyboardLayout old, KeyboardLayout val)
        {
            this.keyboard.GenerateKeyboard(val);
        }

        #endregion
    }
}
