using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Mvvm.UI;

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for WmsLabel.xaml
    /// </summary>
    public partial class WmsLabel : UserControl
    {
        #region Fields

        public static readonly DependencyProperty OriginalTitleProperty = DependencyProperty.RegisterAttached(
                   nameof(OriginalTitle), typeof(string), typeof(WmsLabel), new UIPropertyMetadata());

        // private
        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
           nameof(Title), typeof(string), typeof(WmsLabel));

        private const int EXTRATEXTOFFSET = 10;

        private const int STARTMAXLENGTHCHECK = 5;

        private const int TITLEMAXPERCLENGTH = 20;

        private const string TRIMTEXT = "...";

        private bool adjustSizeAfterFirstUpdate;

        private double defaultControlWidth;

        private double endTitleTextMargin;

        private double titleWidth;

        #endregion

        #region Constructors

        public WmsLabel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public Typeface GetInterface => new Typeface(
                    this.FontFamily,
                    this.FontStyle,
                    this.FontWeight,
                    this.FontStretch);

        public string OriginalTitle { get => (string)this.GetValue(OriginalTitleProperty); set => this.SetValue(OriginalTitleProperty, value); }

        public string Title { get => (string)this.GetValue(TitleProperty); set => this.SetValue(TitleProperty, value); }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var trimTextWidth = this.GetTextWidth(TRIMTEXT);
            this.endTitleTextMargin = trimTextWidth + this.WmsIcon.Width + EXTRATEXTOFFSET;
            this.SizeChanged += this.WmsLabel_SizeChanged;
        }

        public void Show(bool show)
        {
            this.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowIcon(bool show)
        {
            this.WmsIcon.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EvaluateTitle()
        {
            var parentGrid = this as UserControl;
            if (parentGrid == null)
            {
                return;
            }

            this.TitleLabel.ToolTip = null;
            if (this.defaultControlWidth.Equals(0))
            {
                this.adjustSizeAfterFirstUpdate = true;
                this.SetInitialSizeToAdjustTitle(parentGrid);
                return;
            }

            if (this.adjustSizeAfterFirstUpdate)
            {
                this.adjustSizeAfterFirstUpdate = false;
                if (this.TitleLabel.ActualWidth + this.endTitleTextMargin < this.ActualWidth)
                {
                    return;
                }
            }

            double maxTextWidth = 0;
            if (this.defaultControlWidth >= parentGrid.ActualWidth)
            {
                this.SetEditorCoreWidth(parentGrid.ActualWidth);
                maxTextWidth = parentGrid.ActualWidth - this.endTitleTextMargin;
                if (maxTextWidth < 0)
                {
                    return;
                }
            }
            else
            {
                this.SetEditorCoreWidth(this.defaultControlWidth);
                if (this.titleWidth <= parentGrid.ActualWidth)
                {
                    this.ShowTitle(this.Title);
                    return;
                }

                if (this.titleWidth > parentGrid.ActualWidth - this.endTitleTextMargin)
                {
                    maxTextWidth = parentGrid.ActualWidth - this.endTitleTextMargin;
                }
            }

            var currtext = this.GetTextSize(maxTextWidth);
            this.ShowTitle(currtext);
        }

        private string GetTextSize(double maxTextWidth)
        {
            var posChar = (this.Title.Length < STARTMAXLENGTHCHECK) ? this.Title.Length : STARTMAXLENGTHCHECK;
            StringBuilder currText = new StringBuilder(this.Title.Substring(0, posChar));
            while (posChar < this.Title.Length)
            {
                currText.Append(this.Title[posChar]);
                if (this.GetTextWidth(currText.ToString()) > maxTextWidth)
                {
                    if (this.Title.Equals(currText.ToString(), StringComparison.Ordinal) == false)
                    {
                        currText.Append(TRIMTEXT);
                    }

                    break;
                }

                posChar++;
            }

            return currText.ToString();
        }

        private double GetTextWidth(string text) => new FormattedText(text, System.Threading.Thread.CurrentThread.CurrentCulture,
                                                         this.FlowDirection, this.GetInterface, this.FontSize, this.Foreground,
                                                         VisualTreeHelper.GetDpi(this).PixelsPerDip).Width;

        private void SetEditorCoreWidth(double width)
        {
            if (!(this.Parent is Grid parentGrid))
            {
                return;
            }

            if (!(parentGrid.Children[1] is Grid gridEditorCore))
            {
                return;
            }

            gridEditorCore.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        private void SetInitialSizeToAdjustTitle(FrameworkElement parentControl)
        {
            this.titleWidth = this.GetTextWidth(this.Title);
            this.defaultControlWidth = parentControl.ActualWidth;
            this.SetEditorCoreWidth(this.defaultControlWidth);
            if (this.titleWidth <= parentControl.ActualWidth)
            {
                this.ShowTitle(this.Title);
                return;
            }

            var maxTextWidth = this.defaultControlWidth + (this.defaultControlWidth * TITLEMAXPERCLENGTH / 100) - this.endTitleTextMargin;
            if (this.titleWidth <= maxTextWidth)
            {
                maxTextWidth = this.titleWidth;
            }

            var text = this.GetTextSize(maxTextWidth);
            this.ShowTitle(text);
            this.SetInputControlSize(maxTextWidth);
        }

        private void SetInputControlSize(double width)
        {
            var childEditor = LayoutTreeHelper.GetVisualParents(this as DependencyObject).OfType<ContentControl>().FirstOrDefault(x => x.Name == "PART_Root");
            if (childEditor != null)
            {
                var editorControl = LayoutTreeHelper.GetVisualParents(childEditor as DependencyObject).FirstOrDefault() as FrameworkElement;
                var newWidth = width + this.endTitleTextMargin;
                var size = new Size(newWidth, editorControl.ActualHeight);
                editorControl.Measure(size);
                this.Arrange(new Rect(new Size(newWidth, this.DesiredSize.Height)));
            }
        }

        private void ShowTitle(string text)
        {
            this.TitleLabel.Content = text;
            if (text.Equals(this.Title, StringComparison.Ordinal) == false)
            {
                this.TitleLabel.ToolTip = this.Title;
            }
        }

        private void WmsLabel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.EvaluateTitle();
        }

        #endregion
    }
}
