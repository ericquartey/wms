using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;
using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App.Controls
{
    /// <summary>
    /// Interaction logic for WmsLabel.xaml
    /// </summary>
    public partial class WmsLabel : UserControl
    {
        #region Fields

        public static readonly DependencyProperty TrimTitleProperty = DependencyProperty.RegisterAttached(
            nameof(TrimTitle), typeof(bool), typeof(WmsLabel), new FrameworkPropertyMetadata(true));

        private const int EXTRA_TEXT_OFFSET = 10;

        private const int START_MAX_LENGTH_CHECK = 5;

        private const int TITLE_MAX_PERCENT_LENGTH = 20;

        private const string TRIM_TEXT = "...";

        private string additionalInfo;

        private bool adjustSizeAfterFirstUpdate;

        private SolidColorBrush colorRequiredIcon;

        private double defaultControlWidth;

        private double endTitleTextMargin;

        private string title;

        #endregion

        #region Constructors

        public WmsLabel()
        {
            this.InitializeComponent();
            this.Loaded += this.On_Loaded;
        }

        #endregion

        #region Properties

        public string AdditionalInfo
        {
            get => this.additionalInfo;
            set
            {
                this.additionalInfo = value;
                this.SetSizedText();
            }
        }

        public Typeface GetInterface => new Typeface(
                    this.FontFamily,
            this.FontStyle,
            this.FontWeight,
            this.FontStretch);

        public string Title
        {
            get => this.title;
            set
            {
                this.title = value;
                this.EvaluateTitle();
            }
        }

        public bool TrimTitle
        {
            get => (bool)this.GetValue(TrimTitleProperty);
            set => this.SetValue(TrimTitleProperty, value);
        }

        private string CompleteTitle =>
            this.AdditionalInfo == null ? this.Title : $"{this.Title} {this.AdditionalInfo}";

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var trimTextWidth = this.GetTextWidth(TRIM_TEXT);
            this.endTitleTextMargin = trimTextWidth + this.WmsIcon.Width + EXTRA_TEXT_OFFSET;
            this.SizeChanged += this.WmsLabel_SizeChanged;
        }

        public void ShowIcon(bool show)
        {
            this.WmsIcon.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private static SolidColorBrush ConvertColor(ColorRequired color)
        {
            switch (color)
            {
                case ColorRequired.CreateMode:
                    return (SolidColorBrush)Application.Current.Resources[nameof(ColorRequired.CreateMode)];

                case ColorRequired.EditMode:
                    return (SolidColorBrush)Application.Current.Resources[nameof(ColorRequired.EditMode)];

                default:
                    return (SolidColorBrush)Application.Current.Resources[nameof(ColorRequired.Default)];
            }
        }

        private void EvaluateTitle()
        {
            this.TitleLabel.ToolTip = null;
            if (this.defaultControlWidth.Equals(0))
            {
                this.adjustSizeAfterFirstUpdate = true;
                this.SetInitialSizeToAdjustTitle();
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

            this.SetSizedText();
        }

        private string GetSizedText(double maxTextWidth)
        {
            if (!this.TrimTitle)
            {
                return this.CompleteTitle;
            }

            var posChar = (this.CompleteTitle.Length < START_MAX_LENGTH_CHECK)
                ? this.CompleteTitle.Length
                : START_MAX_LENGTH_CHECK;
            var currText = new StringBuilder(this.CompleteTitle.Substring(0, posChar));

            var maxTextWidthReached = false;
            while (posChar < this.CompleteTitle.Length && maxTextWidthReached == false)
            {
                currText.Append(this.CompleteTitle[posChar]);
                var currentTextWidth = this.GetTextWidth(currText.ToString());
                maxTextWidthReached = currentTextWidth > maxTextWidth;

                if (maxTextWidthReached
                    && this.CompleteTitle.Equals(currText.ToString(), StringComparison.Ordinal) == false)
                {
                    currText.Append(TRIM_TEXT);
                }

                posChar++;
            }

            return currText.ToString();
        }

        private double GetTextWidth(string text) => new FormattedText(
            text,
            System.Threading.Thread.CurrentThread.CurrentCulture,
            this.FlowDirection,
            this.GetInterface,
            this.FontSize,
            this.Foreground,
            VisualTreeHelper.GetDpi(this).PixelsPerDip).Width;

        private void On_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is WmsLabel))
            {
                return;
            }

            var parent = LayoutTreeHelper.GetVisualParents(this)
                .OfType<ITitleControl>()
                .FirstOrDefault();
            if (!(parent is FrameworkElement element))
            {
                return;
            }

            var showProperty = (bool)element.GetValue(Controls.ShowTitle.ShowProperty);
            this.Show(showProperty, element);

            this.SetColorRequiredIcon();
        }

        private void SetColorRequiredIcon()
        {
            if (this.DataContext is IExtensionDataEntityViewModel viewModel)
            {
                this.colorRequiredIcon = ConvertColor(viewModel.ColorRequired);
                if (this.colorRequiredIcon != null)
                {
                    this.WmsIcon.ColorizeBrush = this.colorRequiredIcon;
                }
            }
        }

        private void SetInitialSizeToAdjustTitle()
        {
            if (this.CompleteTitle == null)
            {
                return;
            }

            var titleWidth = this.GetTextWidth(this.CompleteTitle);
            this.defaultControlWidth = this.ActualWidth;
            if (titleWidth <= this.ActualWidth)
            {
                this.ShowTitle(this.Title);
                return;
            }

            var maxTextWidth = this.defaultControlWidth + (this.defaultControlWidth * TITLE_MAX_PERCENT_LENGTH / 100) -
                this.endTitleTextMargin;
            if (titleWidth <= maxTextWidth)
            {
                maxTextWidth = titleWidth;
            }

            var text = this.GetSizedText(maxTextWidth);
            this.ShowTitle(text);
            this.SetInputControlSize(maxTextWidth);
        }

        private void SetInputControlSize(double width)
        {
            var childEditor = LayoutTreeHelper.GetVisualParents(this).OfType<ContentControl>()
                .FirstOrDefault(x => x.Name == "PART_Root");
            if (childEditor == null)
            {
                return;
            }

            var editorControl =
                LayoutTreeHelper.GetVisualParents(childEditor).FirstOrDefault() as
                    FrameworkElement;
            var newWidth = width + this.endTitleTextMargin;
            if (editorControl == null || (int)newWidth == 0)
            {
                return;
            }

            var size = new Size(newWidth, editorControl.ActualHeight);
            editorControl.Measure(size);
            this.Arrange(new Rect(new Size(newWidth, this.DesiredSize.Height)));
        }

        private void SetSizedText()
        {
            double maxTextWidth = 0;
            if (this.defaultControlWidth >= this.ActualWidth)
            {
                maxTextWidth = this.ActualWidth - this.endTitleTextMargin;
                if (maxTextWidth < 0)
                {
                    return;
                }
            }
            else
            {
                var titleWidth = this.GetTextWidth(this.CompleteTitle);

                if (titleWidth <= this.ActualWidth)
                {
                    this.ShowTitle(this.CompleteTitle);
                    return;
                }

                if (titleWidth > this.ActualWidth - this.endTitleTextMargin)
                {
                    maxTextWidth = this.ActualWidth - this.endTitleTextMargin;
                }
            }

            var currentText = this.GetSizedText(maxTextWidth);
            this.ShowTitle(currentText);
        }

        private void Show(bool isVisible, DependencyObject parent)
        {
            this.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            if (!isVisible || parent == null)
            {
                return;
            }

            this.ShowBusinessObjectValue(parent);
        }

        private void ShowBusinessObjectValue(DependencyObject parent)
        {
            var type = this.DataContext.GetType();
            var bindingExpression = BindingOperations.GetBindingExpression(
                parent,
                BaseEdit.EditValueProperty);

            var showRequiredIcon = true;
            if (bindingExpression == null)
            {
                DependencyProperty property;
                if (parent.GetType() == typeof(ComboBox))
                {
                    property = ComboBox.BusinessObjectValueProperty;
                }
                else if (parent.GetType() == typeof(LookUpEdit))
                {
                    property = LookUpEdit.BusinessObjectValueProperty;
                }
                else if (parent.GetType() == typeof(InfoText))
                {
                    property = ContentProperty;
                    showRequiredIcon = false;
                }
                else
                {
                    return;
                }

                bindingExpression = BindingOperations.GetBindingExpression(
                    parent,
                    property);
            }

            var path = bindingExpression?.ParentBinding.Path.Path;
            var localizedFieldName = FormControl.RetrieveLocalizedFieldName(type, path);
            var isFieldRequired = FormControl.IsFieldRequired(type, path);
            this.Title = localizedFieldName;
            this.ShowIcon(isFieldRequired && showRequiredIcon);
        }

        private void ShowTitle(string text)
        {
            this.TitleLabel.Content = text;
            if (text.Equals(this.CompleteTitle, StringComparison.Ordinal) == false)
            {
                this.TitleLabel.ToolTip = this.CompleteTitle;
            }
        }

        private void WmsLabel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.EvaluateTitle();
        }

        #endregion
    }
}
