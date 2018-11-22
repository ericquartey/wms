using System.Windows;

namespace Ferretto.Common.Controls
{
    public partial class TextBox : System.Windows.Controls.TextBox
    {
        #region Fields

        public static readonly DependencyProperty IsMultilineProperty = DependencyProperty.Register(
                    nameof(IsMultiline), typeof(bool), typeof(TextBox), new PropertyMetadata(default(bool)));

        #endregion Fields

        #region Properties

        public bool IsMultiline
        {
            get => (bool)this.GetValue(IsMultilineProperty);
            set => this.SetValue(IsMultilineProperty, value);
        }

        #endregion Properties
    }
}
