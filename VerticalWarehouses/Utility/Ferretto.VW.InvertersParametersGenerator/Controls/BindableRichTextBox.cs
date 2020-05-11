using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Ferretto.VW.InvertersParametersGenerator.Controls
{
    public sealed class BindableRichTextBox : RichTextBox
    {
        #region Fields

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(string),
            typeof(BindableRichTextBox),
            new PropertyMetadata(OnSourceChanged));

        #endregion

        #region Properties

        public string Source
        {
            get => this.GetValue(SourceProperty) as string;
            set => this.SetValue(SourceProperty, value);
        }

        #endregion

        #region Methods

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is BindableRichTextBox rtf && rtf.Source != null)
            {
                rtf.Document = rtf.Document ?? new FlowDocument();

                using (var stream = new MemoryStream(Encoding.Default.GetBytes(e.NewValue as string)))
                {
                    var textRange = new TextRange(rtf.Document.ContentStart, rtf.Document.ContentEnd);
                    textRange.Load(stream, string.IsNullOrEmpty(e.NewValue as string) ? DataFormats.Text : DataFormats.Rtf);
                }
            }
        }

        #endregion
    }
}
