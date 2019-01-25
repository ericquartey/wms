using System.Windows;
using DevExpress.Xpf.Editors;

namespace Ferretto.Common.Controls
{
    public class TextBox : TextEdit
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
