using System;
using System.Windows;

namespace Ferretto.WMS.App.Controls
{
    public class ProgressBarEdit : DevExpress.Xpf.Editors.ProgressBarEdit
    {
        #region Fields

        public static readonly DependencyProperty TextOutProperty = DependencyProperty.Register(
                   nameof(TextOut), typeof(string), typeof(ProgressBarEdit), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
                   nameof(Type), typeof(ProgressBarEditType), typeof(ProgressBarEdit), new FrameworkPropertyMetadata(OnTypeChanged));

        #endregion

        #region Properties

        public int HeightBar { get; set; }

        public string TextOut { get => (string)this.GetValue(TextOutProperty); set => this.SetValue(TextOutProperty, value); }

        public ProgressBarEditType Type { get => (ProgressBarEditType)this.GetValue(TypeProperty); set => this.SetValue(TypeProperty, value); }

        #endregion

        #region Methods

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressBarEdit progressBar)
            {
                var type = (ProgressBarEditType)e.NewValue;
                switch (type)
                {
                    case ProgressBarEditType.Percent:
                        progressBar.ContentDisplayMode = DevExpress.Xpf.Editors.ContentDisplayMode.None;
                        progressBar.Orientation = System.Windows.Controls.Orientation.Horizontal;
                        progressBar.ShowBorder = false;
                        progressBar.IsPercent = true;
                        progressBar.Height = 35;
                        break;

                    case ProgressBarEditType.MinMaxCurrentH:
                        progressBar.ContentDisplayMode = DevExpress.Xpf.Editors.ContentDisplayMode.Value;
                        progressBar.Orientation = System.Windows.Controls.Orientation.Horizontal;
                        progressBar.Height = 50;
                        progressBar.ShowBorder = false;
                        progressBar.IsPercent = false;
                        progressBar.Maximum = progressBar.TextOut != null ? double.Parse(progressBar.TextOut) : 100;
                        break;

                    case ProgressBarEditType.MinMaxCurrentV:
                        progressBar.ContentDisplayMode = DevExpress.Xpf.Editors.ContentDisplayMode.Value;
                        progressBar.Orientation = System.Windows.Controls.Orientation.Vertical;
                        progressBar.Width = 80;
                        progressBar.ShowBorder = true;

                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        #endregion
    }
}
