using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Xpf.Gauges;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for PpcGauge.xaml
    /// </summary>
    public partial class PpcGauge : UserControl
    {
        #region Fields

        public static readonly DependencyProperty GaugeEndAngleProperty = DependencyProperty.Register(nameof(GaugeEndAngle), typeof(double?), typeof(PpcGauge), new PropertyMetadata(null));

        public static readonly DependencyProperty GaugeLayoutModeProperty = DependencyProperty.Register(nameof(GaugeLayoutMode), typeof(ArcScaleLayoutMode), typeof(PpcGauge), new PropertyMetadata(ArcScaleLayoutMode.ThreeQuarters));

        public static readonly DependencyProperty GaugeStartAngleProperty = DependencyProperty.Register(nameof(GaugeStartAngle), typeof(double?), typeof(PpcGauge), new PropertyMetadata(null));

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(nameof(LabelText), typeof(string), typeof(PpcGauge), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LabelVisibleProperty = DependencyProperty.Register(nameof(LabelVisible), typeof(bool), typeof(PpcGauge));

        public static readonly DependencyProperty MajorIntervalCountProperty = DependencyProperty.Register(nameof(MajorIntervalCount), typeof(int), typeof(PpcGauge), new PropertyMetadata(10));

        public static readonly DependencyProperty MajorIntervalShowFirstProperty = DependencyProperty.Register(nameof(MajorIntervalShowFirst), typeof(bool), typeof(PpcGauge), new PropertyMetadata(true));

        public static readonly DependencyProperty MajorIntervalShowLastProperty = DependencyProperty.Register(nameof(MajorIntervalShowLast), typeof(bool), typeof(PpcGauge), new PropertyMetadata(true));

        public static readonly DependencyProperty MajorIntervalVisibleProperty = DependencyProperty.Register(nameof(MajorIntervalVisible), typeof(bool), typeof(PpcGauge));

        public static readonly DependencyProperty MarkerFillProperty = DependencyProperty.Register(nameof(MarkerFill), typeof(SolidColorBrush), typeof(PpcGauge));

        public static readonly DependencyProperty MarkerHeightProperty = DependencyProperty.Register(nameof(MarkerHeight), typeof(double), typeof(PpcGauge));

        public static readonly DependencyProperty MarkerOrientationProperty = DependencyProperty.Register(nameof(MarkerOrientation), typeof(ArcScaleMarkerOrientation), typeof(PpcGauge));

        public static readonly DependencyProperty MarkerVisibleProperty = DependencyProperty.Register(nameof(MarkerVisible), typeof(bool), typeof(PpcGauge));

        public static readonly DependencyProperty MarkerWidthProperty = DependencyProperty.Register(nameof(MarkerWidth), typeof(double), typeof(PpcGauge));

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(PpcGauge));

        public static readonly DependencyProperty MinorIntervalCountProperty = DependencyProperty.Register(nameof(MinorIntervalCount), typeof(int), typeof(PpcGauge), new PropertyMetadata(1));

        public static readonly DependencyProperty MinorIntervalVisibleProperty = DependencyProperty.Register(nameof(MinorIntervalVisible), typeof(bool), typeof(PpcGauge));

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(PpcGauge));

        public static readonly DependencyProperty NeedleEndProperty = DependencyProperty.Register(nameof(NeedleEnd), typeof(double), typeof(PpcGauge));

        public static readonly DependencyProperty NeedleFillProperty = DependencyProperty.Register(nameof(NeedleFill), typeof(SolidColorBrush), typeof(PpcGauge));

        public static readonly DependencyProperty NeedleVisibleProperty = DependencyProperty.Register(nameof(NeedleVisible), typeof(bool), typeof(PpcGauge));

        public static readonly DependencyProperty OffsetMajorLabelProperty = DependencyProperty.Register(nameof(OffsetMajorLabel), typeof(double), typeof(PpcGauge), new PropertyMetadata(-5.0));

        public static readonly DependencyProperty OffsetMarkerProperty = DependencyProperty.Register(nameof(OffsetMarker), typeof(double), typeof(PpcGauge), new PropertyMetadata(-5.0));

        public static readonly DependencyProperty OffsetRangeBarProperty = DependencyProperty.Register(nameof(OffsetRangeBar), typeof(double), typeof(PpcGauge), new PropertyMetadata(-5.0));

        public static readonly DependencyProperty OffsetRangeProperty = DependencyProperty.Register(nameof(OffsetRange), typeof(double), typeof(PpcGauge), new PropertyMetadata(-5.0));

        public static readonly DependencyProperty OffsetTickmarkProperty = DependencyProperty.Register(nameof(OffsetTickmark), typeof(double), typeof(PpcGauge), new PropertyMetadata(-5.0));

        public static readonly DependencyProperty RangeBarBackgroundFillProperty = DependencyProperty.Register(nameof(RangeBarBackgroundFill), typeof(SolidColorBrush), typeof(PpcGauge));

        public static readonly DependencyProperty RangeBarBackgroundVisibleProperty = DependencyProperty.Register(nameof(RangeBarBackgroundVisible), typeof(bool), typeof(PpcGauge));

        public static readonly DependencyProperty RangeBarFillProperty = DependencyProperty.Register(nameof(RangeBarFill), typeof(SolidColorBrush), typeof(PpcGauge));

        public static readonly DependencyProperty RangeBarThicknessProperty = DependencyProperty.Register(nameof(RangeBarThickness), typeof(double), typeof(PpcGauge));

        public static readonly DependencyProperty RangeBarVisibleProperty = DependencyProperty.Register(nameof(RangeBarVisible), typeof(bool), typeof(PpcGauge));

        public static readonly DependencyProperty RangeFillProperty = DependencyProperty.Register(nameof(RangeFill), typeof(SolidColorBrush), typeof(PpcGauge));

        public static readonly DependencyProperty RangeThicknessProperty = DependencyProperty.Register(nameof(RangeThickness), typeof(double), typeof(PpcGauge));

        public static readonly DependencyProperty RangeVisibleProperty = DependencyProperty.Register(nameof(RangeVisible), typeof(bool), typeof(PpcGauge));

        public static readonly DependencyProperty TickmarkHeightProperty = DependencyProperty.Register(nameof(TickmarkHeight), typeof(double), typeof(PpcGauge), new PropertyMetadata(1.0));

        public static readonly DependencyProperty TickmarkWidthProperty = DependencyProperty.Register(nameof(TickmarkWidth), typeof(double), typeof(PpcGauge), new PropertyMetadata(1.0));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(PpcGauge));

        #endregion

        #region Constructors

        public PpcGauge()
        {
            this.InitializeComponent();

            this.LayoutRoot.DataContext = this;

            this.Loaded += (s, e) => { this.OnAppeared(); };
            this.Unloaded += (s, e) => { this.Disappear(); };
        }

        #endregion

        #region Properties

        public double? GaugeEndAngle
        {
            get => (double?)this.GetValue(GaugeEndAngleProperty);
            set => this.SetValue(GaugeEndAngleProperty, value);
        }

        public ArcScaleLayoutMode GaugeLayoutMode
        {
            get => (ArcScaleLayoutMode)this.GetValue(GaugeLayoutModeProperty);
            set => this.SetValue(GaugeLayoutModeProperty, value);
        }

        public double? GaugeStartAngle
        {
            get => (double?)this.GetValue(GaugeStartAngleProperty);
            set => this.SetValue(GaugeStartAngleProperty, value);
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set => this.SetValue(LabelTextProperty, value);
        }

        public bool LabelVisible
        {
            get => (bool)this.GetValue(LabelVisibleProperty);
            set => this.SetValue(LabelVisibleProperty, value);
        }

        public int MajorIntervalCount
        {
            get => (int)this.GetValue(MajorIntervalCountProperty);
            set => this.SetValue(MajorIntervalCountProperty, value);
        }

        public bool MajorIntervalShowFirst
        {
            get => (bool)this.GetValue(MajorIntervalShowFirstProperty);
            set => this.SetValue(MajorIntervalShowFirstProperty, value);
        }

        public bool MajorIntervalShowLast
        {
            get => (bool)this.GetValue(MajorIntervalShowLastProperty);
            set => this.SetValue(MajorIntervalShowLastProperty, value);
        }

        public bool MajorIntervalVisible
        {
            get => (bool)this.GetValue(MajorIntervalVisibleProperty);
            set => this.SetValue(MajorIntervalVisibleProperty, value);
        }

        public SolidColorBrush MarkerFill
        {
            get => (SolidColorBrush)this.GetValue(MarkerFillProperty);
            set => this.SetValue(MarkerFillProperty, value);
        }

        public double MarkerHeight
        {
            get => (double)this.GetValue(MarkerHeightProperty);
            set => this.SetValue(MarkerHeightProperty, value);
        }

        public ArcScaleMarkerOrientation MarkerOrientation
        {
            get => (ArcScaleMarkerOrientation)this.GetValue(MarkerOrientationProperty);
            set => this.SetValue(MarkerOrientationProperty, value);
        }

        public bool MarkerVisible
        {
            get => (bool)this.GetValue(MarkerVisibleProperty);
            set => this.SetValue(MarkerVisibleProperty, value);
        }

        public double MarkerWidth
        {
            get => (double)this.GetValue(MarkerWidthProperty);
            set => this.SetValue(MarkerWidthProperty, value);
        }

        public double MaxValue
        {
            get => (double)this.GetValue(MaxValueProperty);
            set => this.SetValue(MaxValueProperty, value);
        }

        public int MinorIntervalCount
        {
            get => (int)this.GetValue(MinorIntervalCountProperty);
            set => this.SetValue(MinorIntervalCountProperty, value);
        }

        public bool MinorIntervalVisible
        {
            get => (bool)this.GetValue(MinorIntervalVisibleProperty);
            set => this.SetValue(MinorIntervalVisibleProperty, value);
        }

        public double MinValue
        {
            get => (double)this.GetValue(MinValueProperty);
            set => this.SetValue(MinValueProperty, value);
        }

        public double NeedleEnd
        {
            get => (double)this.GetValue(NeedleEndProperty);
            set => this.SetValue(NeedleEndProperty, value);
        }

        public SolidColorBrush NeedleFill
        {
            get => (SolidColorBrush)this.GetValue(NeedleFillProperty);
            set => this.SetValue(NeedleFillProperty, value);
        }

        public bool NeedleVisible
        {
            get => (bool)this.GetValue(NeedleVisibleProperty);
            set => this.SetValue(NeedleVisibleProperty, value);
        }

        public double OffsetMajorLabel
        {
            get => (double)this.GetValue(OffsetMajorLabelProperty);
            set => this.SetValue(OffsetMajorLabelProperty, value);
        }

        public double OffsetMarker
        {
            get => (double)this.GetValue(OffsetMarkerProperty);
            set => this.SetValue(OffsetMarkerProperty, value);
        }

        public double OffsetRange
        {
            get => (double)this.GetValue(OffsetRangeProperty);
            set => this.SetValue(OffsetRangeProperty, value);
        }

        public double OffsetRangeBar
        {
            get => (double)this.GetValue(OffsetRangeBarProperty);
            set => this.SetValue(RangeThicknessProperty, value);
        }

        public double OffsetTickmark
        {
            get => (double)this.GetValue(OffsetTickmarkProperty);
            set => this.SetValue(OffsetTickmarkProperty, value);
        }

        public SolidColorBrush RangeBarBackgroundFill
        {
            get => (SolidColorBrush)this.GetValue(RangeBarBackgroundFillProperty);
            set => this.SetValue(RangeBarBackgroundFillProperty, value);
        }

        public bool RangeBarBackgroundVisible
        {
            get => (bool)this.GetValue(RangeBarBackgroundVisibleProperty);
            set => this.SetValue(RangeBarBackgroundVisibleProperty, value);
        }

        public SolidColorBrush RangeBarFill
        {
            get => (SolidColorBrush)this.GetValue(RangeBarFillProperty);
            set => this.SetValue(RangeBarFillProperty, value);
        }

        public double RangeBarThickness
        {
            get => (double)this.GetValue(RangeBarThicknessProperty);
            set => this.SetValue(RangeBarThicknessProperty, value);
        }

        public bool RangeBarVisible
        {
            get => (bool)this.GetValue(RangeBarVisibleProperty);
            set => this.SetValue(RangeBarVisibleProperty, value);
        }

        public SolidColorBrush RangeFill
        {
            get => (SolidColorBrush)this.GetValue(RangeFillProperty);
            set => this.SetValue(RangeFillProperty, value);
        }

        public double RangeThickness
        {
            get => (double)this.GetValue(RangeThicknessProperty);
            set => this.SetValue(RangeThicknessProperty, value);
        }

        public bool RangeVisible
        {
            get => (bool)this.GetValue(RangeVisibleProperty);
            set => this.SetValue(RangeVisibleProperty, value);
        }

        public double TickmarkHeight
        {
            get => (double)this.GetValue(TickmarkHeightProperty);
            set => this.SetValue(TickmarkHeightProperty, value);
        }

        public double TickmarkWidth
        {
            get => (double)this.GetValue(TickmarkWidthProperty);
            set => this.SetValue(TickmarkWidthProperty, value);
        }

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        #endregion

        #region Methods

        protected void Disappear()
        {
        }

        protected void OnAppeared()
        {
        }

        #endregion
    }
}
