using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    [StyleTypedProperty(Property = nameof(BusyIndicator.BusyStyle), StyleTargetType = typeof(Control))]
    public class BusyIndicator : Decorator
    {
        #region Fields

        public static readonly DependencyProperty BusyHorizontalAlignmentProperty = DependencyProperty.Register(
          nameof(BusyHorizontalAlignment),
          typeof(HorizontalAlignment),
          typeof(BusyIndicator),
          new FrameworkPropertyMetadata(HorizontalAlignment.Center));

        public static readonly DependencyProperty BusyStyleProperty =
            DependencyProperty.Register(
            nameof(BusyStyle),
            typeof(Style),
            typeof(BusyIndicator),
            new FrameworkPropertyMetadata(OnBusyStyleChanged));

        public static readonly DependencyProperty BusyVerticalAlignmentProperty = DependencyProperty.Register(
          nameof(BusyVerticalAlignment),
          typeof(VerticalAlignment),
          typeof(BusyIndicator),
          new FrameworkPropertyMetadata(VerticalAlignment.Center));

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(BusyIndicator),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ShowModeProperty =
                        DependencyProperty.Register(
                            nameof(ShowMode),
                            typeof(IndicatorType),
                            typeof(BusyIndicator),
                        new FrameworkPropertyMetadata(IndicatorType.Default, OnIndicatorTypeChanged));

        private const string suffixStyle = "Style";
        private readonly BackgroundVisualHost busyHost = new BackgroundVisualHost();

        #endregion Fields

        #region Constructors

        static BusyIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BusyIndicator),
                new FrameworkPropertyMetadata(typeof(BusyIndicator)));
        }

        public BusyIndicator()
        {
            this.AddLogicalChild(this.busyHost);
            this.AddVisualChild(this.busyHost);

            this.SetBinding(this.busyHost, IsBusyProperty, BackgroundVisualHost.IsContentShowingProperty);
            this.SetBinding(this.busyHost, BusyHorizontalAlignmentProperty, BackgroundVisualHost.HorizontalAlignmentProperty);
            this.SetBinding(this.busyHost, BusyVerticalAlignmentProperty, BackgroundVisualHost.VerticalAlignmentProperty);
        }

        #endregion Constructors

        #region Properties

        public HorizontalAlignment BusyHorizontalAlignment
        {
            get => (HorizontalAlignment)this.GetValue(BusyHorizontalAlignmentProperty);
            set => this.SetValue(BusyHorizontalAlignmentProperty, value);
        }

        public Style BusyStyle
        {
            get => (Style)this.GetValue(BusyStyleProperty);
            set => this.SetValue(BusyStyleProperty, value);
        }

        public VerticalAlignment BusyVerticalAlignment
        {
            get => (VerticalAlignment)this.GetValue(BusyVerticalAlignmentProperty);
            set => this.SetValue(BusyVerticalAlignmentProperty, value);
        }

        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }

        public IndicatorType ShowMode
        {
            get => (IndicatorType)this.GetValue(ShowModeProperty);
            set => this.SetValue(ShowModeProperty, value);
        }

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                if (this.Child != null)
                {
                    yield return this.Child;
                }

                yield return this.busyHost;
            }
        }

        protected override int VisualChildrenCount => this.Child != null ? 2 : 1;

        #endregion Properties

        #region Methods

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var ret = new Size(0, 0);
            if (this.Child != null)
            {
                this.Child.Arrange(new Rect(arrangeSize));
                ret = this.Child.RenderSize;
                this.Child.Visibility = this.IsBusy ? Visibility.Hidden : Visibility.Visible;
            }

            this.busyHost.Arrange(new Rect(arrangeSize));

            return new Size(Math.Max(ret.Width, this.busyHost.RenderSize.Width), Math.Max(ret.Height, this.busyHost.RenderSize.Height));
        }

        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            if (this.Child != null)
            {
                switch (index)
                {
                    case 0:
                        return this.Child;

                    case 1:
                        return this.busyHost;
                }
            }
            else if (index == 0)
            {
                return this.busyHost;
            }

            throw new IndexOutOfRangeException(Errors.BusyIndicatorInvalidIndex);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var ret = new Size(0, 0);
            if (this.Child != null)
            {
                this.Child.Measure(constraint);
                ret = this.Child.DesiredSize;
            }

            this.busyHost.Measure(constraint);

            return new Size(Math.Max(ret.Width, this.busyHost.DesiredSize.Width), Math.Max(ret.Height, this.busyHost.DesiredSize.Height));
        }

        private static void OnBusyStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var busyIndicator = (BusyIndicator)d;
            if (e.NewValue is Style newStyle)
            {
                SetStyleToContent(busyIndicator, newStyle);
            }
        }

        private static void OnIndicatorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var busyIndicator = (BusyIndicator)d;
            if (e.NewValue is IndicatorType decType)
            {
                var newType = $"{nameof(BusyIndicator)}{decType.ToString()}{suffixStyle}";
                if (Application.Current.Resources[newType] is Style styleToApply)
                {
                    SetStyleToContent(busyIndicator, styleToApply);
                }
                else
                {
                    throw new ArgumentException(string.Format(Errors.BusyIndicatorStyleNotFound, newType, decType));
                }
            }
        }

        private static void SetStyleToContent(BusyIndicator busyIndicator, Style styleToApply)
        {
            busyIndicator.busyHost.CreateContent = () => new Control { Style = styleToApply };
        }

        private void SetBinding(DependencyObject obj, DependencyProperty source, DependencyProperty target)
        {
            var binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath(source);
            BindingOperations.SetBinding(obj, target, binding);
        }

        #endregion Methods
    }
}
