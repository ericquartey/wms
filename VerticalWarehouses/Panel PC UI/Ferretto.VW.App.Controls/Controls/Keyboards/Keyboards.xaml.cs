using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Controls.Keyboards
{
    public partial class Keyboards : PpcDialogView
    {
        #region Fields

        public static readonly DependencyProperty KeyboardsssProperty = DependencyProperty.Register(
            nameof(Keyboardsss),
            typeof(KeyboardDefinition),
            typeof(Keyboards),
            new PropertyMetadata(null, new PropertyChangedCallback(OnKeyboardsssChanged)));

        #endregion

        #region Constructors

        public Keyboards()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public KeyboardDefinition Keyboardsss
        {
            get => (KeyboardDefinition)this.GetValue(KeyboardsssProperty);
            set => this.SetValue(KeyboardsssProperty, value);
        }

        #endregion

        #region Methods

        public void OnCreateKeyboard()
        {
            var keyboard = this.Keyboardsss.Layouts.Single(s => s.Id.Equals(this.Keyboardsss.ActiveLayout));
            this.Keyboard_Grid.Width = keyboard.Width;
            this.Keyboard_Grid.Height = keyboard.Height;

            this.Keyboard_Grid.Children.Clear();
            this.Keyboard_Grid.RowDefinitions.Clear();
            this.Keyboard_Grid.ColumnDefinitions.Clear();

            for (int i = 0; i < keyboard.Rows.Count; i++)
            {
                this.Keyboard_Grid.RowDefinitions.Add(new RowDefinition());
            }

            var cols = keyboard.Rows.Select(s => s.Keys.Count).Max();

            for (int i = 0; i < cols; i++)
            {
                this.Keyboard_Grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            int row = 0;
            foreach (var r in keyboard.Rows)
            {
                int col = 0;
                foreach (var k in r.Keys)
                {
                    PpcButton b = new PpcButton();

                    if (col == 0 && r.LeftMargin > 0)
                    {
                        b.Margin = new Thickness(r.LeftMargin, 4, 4, 4);
                    }

                    b.Height = keyboard.KeyHeight;
                    b.Width = keyboard.KeyWidth;
                    b.Content = k.Text ?? k.Value;
                    b.Style = this.FindResource("PpcButtonNumKeyStyle") as Style;

                    Grid.SetRow(b, row);
                    Grid.SetRowSpan(b, k.RowSpan + 1);

                    Grid.SetColumn(b, col);
                    Grid.SetColumnSpan(b, k.ColSpan + 1);

                    this.Keyboard_Grid.Children.Add(b);

                    col++;
                }
                row++;
            }
        }

        private static void OnKeyboardsssChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Keyboards keyboard)
            {
                keyboard.OnCreateKeyboard();
            }
        }

        #endregion
    }
}
