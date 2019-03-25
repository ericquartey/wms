using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.Common.Controls
{
    public class WmsRulerControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty DimensionHeightProperty =
                    DependencyProperty.Register(nameof(DimensionHeight), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty DimensionWidthProperty =
            DependencyProperty.Register(nameof(DimensionWidth), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty ForegroundTextProperty =
            DependencyProperty.Register(nameof(ForegroundText), typeof(Brush), typeof(WmsRulerControl), new UIPropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty HideAllMarkersProperty = DependencyProperty.Register(
                nameof(HideAllMarkers), typeof(bool), typeof(WmsRulerControl), new FrameworkPropertyMetadata(OnHideAllMarkersChanged));

        public static readonly DependencyProperty LittleMarkLengthProperty =
                            DependencyProperty.Register(nameof(LittleMarkLength), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(8.0));

        public static readonly DependencyProperty MiddleMarkLengthProperty =
                    DependencyProperty.Register(nameof(MiddleMarkLength), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(14.0));

        public static readonly DependencyProperty OrientationProperty =
                    DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(WmsRulerControl), new FrameworkPropertyMetadata(OnOrientationChanged));

        public static readonly DependencyProperty OriginHorizontalProperty =
                               DependencyProperty.Register(nameof(OriginHorizontal), typeof(OriginHorizontal), typeof(WmsRulerControl), new FrameworkPropertyMetadata(OriginHorizontal.Left));

        public static readonly DependencyProperty OriginVerticalProperty =
                               DependencyProperty.Register(nameof(OriginVertical), typeof(OriginVertical), typeof(WmsRulerControl), new FrameworkPropertyMetadata(OriginVertical.Bottom));

        public static readonly DependencyProperty ShowInfoProperty = DependencyProperty.Register(
                                    nameof(ShowInfo), typeof(bool), typeof(WmsRulerControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowLittleMarkProperty = DependencyProperty.Register(
       nameof(ShowLittleMark), typeof(bool), typeof(WmsRulerControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowMarkProperty = DependencyProperty.Register(
          nameof(ShowMark), typeof(bool), typeof(WmsRulerControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowMiddleMarkProperty = DependencyProperty.Register(
       nameof(ShowMiddleMark), typeof(bool), typeof(WmsRulerControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty StepProperty =
                    DependencyProperty.Register(nameof(Step), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(100.0, OnStepChanged));

        public static readonly DependencyProperty TrayHeightProperty = DependencyProperty.Register(nameof(TrayHeight), typeof(double), typeof(WmsRulerControl));

        public static readonly DependencyProperty TrayWidthProperty =
                               DependencyProperty.Register(nameof(TrayWidth), typeof(double), typeof(WmsRulerControl));

        private const string DEFAULTBACKGROUND = "CommonSecondaryMedium";

        private const int LITTLE_INTERVALMARKS = 10;

        private const int MIDDLE_INTERVALMARKS = 2;

        private const int OFFSET_BORDER = 2;

        private const double TEXT_OFFSET_FACTOR = 20;

        private const double WIDTH_MARK = 1;

        private Pen pen;

        private double penHalfSize;

        #endregion

        #region Constructors

        public WmsRulerControl()
        {
            this.UseLayoutRounding = false;
            this.SnapsToDevicePixels = false;
            var target = this;
            RenderOptions.SetEdgeMode(target, EdgeMode.Aliased);
        }

        #endregion

        #region Properties

        public double DimensionHeight
        {
            get => (double)this.GetValue(DimensionHeightProperty);
            set => this.SetValue(DimensionHeightProperty, value);
        }

        public double DimensionWidth
        {
            get => (double)this.GetValue(DimensionWidthProperty);
            set => this.SetValue(DimensionWidthProperty, value);
        }

        public Brush ForegroundText
        {
            get => (Brush)this.GetValue(ForegroundTextProperty);
            set => this.SetValue(ForegroundTextProperty, value);
        }

        public bool HideAllMarkers
        {
            get => (bool)this.GetValue(HideAllMarkersProperty);
            set => this.SetValue(HideAllMarkersProperty, value);
        }

        public double LittleMarkLength

        {
            get => (double)this.GetValue(LittleMarkLengthProperty);
            set => this.SetValue(LittleMarkLengthProperty, value);
        }

        public double MiddleMarkLength
        {
            get => (double)this.GetValue(MiddleMarkLengthProperty);
            set => this.SetValue(MiddleMarkLengthProperty, value);
        }

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        public OriginHorizontal OriginHorizontal
        {
            get => (OriginHorizontal)this.GetValue(OriginHorizontalProperty);
            set => this.SetValue(OriginHorizontalProperty, value);
        }

        public OriginVertical OriginVertical
        {
            get => (OriginVertical)this.GetValue(OriginVerticalProperty);
            set => this.SetValue(OriginVerticalProperty, value);
        }

        public bool ShowInfo
        {
            get => (bool)this.GetValue(ShowInfoProperty);
            set => this.SetValue(ShowInfoProperty, value);
        }

        public bool ShowLittleMark
        {
            get => (bool)this.GetValue(ShowLittleMarkProperty);
            set => this.SetValue(ShowLittleMarkProperty, value);
        }

        public bool ShowMark
        {
            get => (bool)this.GetValue(ShowMarkProperty);
            set => this.SetValue(ShowMarkProperty, value);
        }

        public bool ShowMiddleMark
        {
            get => (bool)this.GetValue(ShowMiddleMarkProperty);
            set => this.SetValue(ShowMiddleMarkProperty, value);
        }

        public double Step
        {
            get => (double)this.GetValue(StepProperty);
            set => this.SetValue(StepProperty, value);
        }

        public double TrayHeight
        {
            get => (double)this.GetValue(TrayHeightProperty);
            set => this.SetValue(TrayHeightProperty, value);
        }

        public double TrayWidth
        {
            get => (double)this.GetValue(TrayWidthProperty);
            set => this.SetValue(TrayWidthProperty, value);
        }

        #endregion

        #region Methods

        public static double ConvertMillimetersToPixel(double value, double pixel, double mm)
        {
            return mm > 0 ? (pixel * value / mm) : value;
        }

        public void Redraw()
        {
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var totalSteps = 0;

            switch (this.Orientation)
            {
                case Orientation.Horizontal:
                    {
                        if (this.ActualWidth > 0 && this.Step > 0)
                        {
                            totalSteps = (int)(this.DimensionWidth / this.Step);
                            if (double.IsNegativeInfinity(totalSteps) ||
                                double.IsPositiveInfinity(totalSteps))
                            {
                                totalSteps = 0;
                            }
                        }

                        break;
                    }

                case Orientation.Vertical:
                    {
                        if (this.ActualHeight > 0 && this.Step > 0)
                        {
                            totalSteps = (int)(this.DimensionHeight / this.Step);
                            if (double.IsNegativeInfinity(totalSteps) ||
                                double.IsPositiveInfinity(totalSteps))
                            {
                                totalSteps = 0;
                            }
                        }

                        break;
                    }
            }

            if (!double.IsPositiveInfinity(totalSteps) &&
                !double.IsNegativeInfinity(totalSteps) &&
                totalSteps != 0)
            {
                this.InitializePen();
                if (this.HideAllMarkers)
                {
                    this.ShowOnlyBaseMarkers(drawingContext);
                }
                else
                {
                    this.DrawMarkers(drawingContext, totalSteps);
                }
            }
        }

        private static void OnHideAllMarkersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl ruler)
            {
                ruler.Redraw();
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl ruler)
            {
                ruler.Redraw();
            }
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl rulerControl)
            {
                rulerControl.UpdateLayout();
            }
        }

        private bool CanBeShown(int intervalMark, int factor)
        {
            var pixelSize = this.GetStepPixelSize(intervalMark);
            return pixelSize > (this.GetSizeOfPen() * factor);
        }

        private bool CanShowText(int maxTextLenght, double fontSize)
        {
            var pixelSize = this.GetStepPixelSize(1);
            var margin = this.GetSizeOfPen() * 2;
            var ft = new FormattedText(
                    (maxTextLenght * this.Step).ToString(CultureInfo.InvariantCulture),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(this.FontFamily.ToString()),
                                 fontSize,
                                 this.ForegroundText,
                                 VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return pixelSize > (ft.Width + margin);
        }

        private void DrawBase(DrawingContext drawingContext)
        {
            var pointStart = new Point(0, 0);
            var pointEnd = new Point(0, 0);
            var sizeOfPen = this.GetSizeOfPen();

            if (this.Orientation == Orientation.Horizontal)
            {
                pointStart.Y = this.ActualHeight - sizeOfPen;
                pointEnd.Y = pointStart.Y;
                pointStart.X = this.HideAllMarkers ? -1 : 0;
                pointEnd.X = this.TrayWidth - sizeOfPen;
                if (this.HideAllMarkers)
                {
                    pointEnd.X++;
                }
            }
            else
            {
                pointStart.X = this.ActualWidth - sizeOfPen;
                pointEnd.X = pointStart.X;
                pointStart.Y = this.HideAllMarkers ? -1 : 0;
                pointEnd.Y = this.TrayHeight - 1;
                if (this.HideAllMarkers)
                {
                    pointEnd.Y++;
                }
            }

            this.DrawSnappedLinesBetweenPoints(drawingContext, pointStart, pointEnd);
        }

        private void DrawLittleMark(DrawingContext drawingContext, int currentStep)
        {
            var littleMarkStart = new Point(0, 0);
            var littleMarkEnd = new Point(0, 0);
            double stepPixel = 0;
            if (this.Orientation == Orientation.Horizontal)
            {
                stepPixel = ConvertMillimetersToPixel(this.Step, this.TrayWidth, this.DimensionWidth);
            }
            else
            {
                stepPixel = ConvertMillimetersToPixel(this.Step, this.TrayHeight, this.DimensionHeight);
            }

            var basePixelStart = stepPixel * currentStep;
            var littlePixelStep = stepPixel / LITTLE_INTERVALMARKS;
            for (var j = 1; j < LITTLE_INTERVALMARKS; j++)
            {
                if (j == LITTLE_INTERVALMARKS / 2)
                {
                    continue;
                }
                if (this.Orientation == Orientation.Horizontal)
                {
                    littleMarkStart.X = basePixelStart + (littlePixelStep * j);
                    littleMarkStart.Y = this.Height - this.LittleMarkLength + this.penHalfSize;
                    littleMarkEnd.Y = this.Height - this.GetSizeOfPen();
                    if (this.OriginHorizontal == OriginHorizontal.Right)
                    {
                        littleMarkStart.X = this.ActualWidth - littleMarkStart.X;
                    }

                    littleMarkEnd.X = littleMarkStart.X;
                }
                else
                {
                    var pixelPosition = basePixelStart + (littlePixelStep * j);
                    littleMarkStart.Y = (this.OriginVertical == OriginVertical.Top) ? pixelPosition : this.ActualHeight - pixelPosition - 1;
                    littleMarkStart.X = this.Width - this.LittleMarkLength + this.GetSizeOfPen();
                    littleMarkEnd.X = this.Width - this.penHalfSize - this.GetSizeOfPen();
                    littleMarkEnd.Y = littleMarkStart.Y;
                }

                if (littleMarkEnd.X < 0 ||
                    littleMarkEnd.X > this.ActualWidth)
                {
                    return;
                }

                if (littleMarkEnd.Y < 0 ||
                    littleMarkEnd.Y > this.ActualHeight)
                {
                    return;
                }

                this.DrawSnappedLinesBetweenPoints(drawingContext, littleMarkStart, littleMarkEnd);
            }
        }

        private void DrawMark(DrawingContext drawingContext, int currentStep)
        {
            var littleMarkStart = new Point(0, 0);
            var littleMarkEnd = new Point(0, 0);
            var sizeOfPen = this.GetSizeOfPen();
            if (this.Orientation == Orientation.Horizontal)
            {
                littleMarkStart.X = ConvertMillimetersToPixel(this.Step * currentStep, this.TrayWidth, this.DimensionWidth);
                littleMarkStart.Y = this.penHalfSize;
                littleMarkEnd.Y = this.ActualHeight - sizeOfPen;
                if (this.OriginHorizontal == OriginHorizontal.Right)
                {
                    littleMarkStart.X = this.ActualWidth - littleMarkStart.X;
                }

                littleMarkEnd.X = littleMarkStart.X;
            }
            else
            {
                var offSet = ConvertMillimetersToPixel(this.Step * currentStep, this.TrayHeight, this.DimensionHeight);
                littleMarkStart.X = this.penHalfSize;
                littleMarkEnd.X = this.ActualWidth - sizeOfPen;
                littleMarkStart.Y = (this.OriginVertical == OriginVertical.Top) ? offSet : this.ActualHeight - offSet - sizeOfPen;
                littleMarkEnd.Y = littleMarkStart.Y;
            }

            this.DrawSnappedLinesBetweenPoints(drawingContext, littleMarkStart, littleMarkEnd);
        }

        private void DrawMarkers(DrawingContext drawingContext, int totalSteps)
        {
            var isBaseDrawVisible = false;
            totalSteps = totalSteps + 1;
            var currentFontSize = this.GetFontSize();
            var canShowText = this.CanShowText(totalSteps, currentFontSize);
            var canShowMarks = this.CanBeShown(1, 15);
            var canShowMiddleMarks = this.CanBeShown(MIDDLE_INTERVALMARKS, 13);
            var canShowLittleMarks = this.CanBeShown(LITTLE_INTERVALMARKS, 4);

            for (var currStep = 0; currStep < totalSteps; currStep++)
            {
                if (this.ShowInfo && canShowText)
                {
                    this.DrawText(drawingContext, currStep, currentFontSize);
                }

                if (this.ShowMark)
                {
                    if (currStep == 0 ||
                        canShowMarks)
                    {
                        this.DrawMark(drawingContext, currStep);
                    }

                    isBaseDrawVisible = true;
                }

                if (this.ShowMiddleMark && canShowMiddleMarks)
                {
                    this.DrawMiddleMark(drawingContext, currStep);
                    isBaseDrawVisible = true;
                }

                if (this.ShowLittleMark && canShowLittleMarks)
                {
                    this.DrawLittleMark(drawingContext, currStep);
                    isBaseDrawVisible = true;
                }
            }

            if (isBaseDrawVisible)
            {
                this.DrawBase(drawingContext);
            }
        }

        private void DrawMiddleMark(DrawingContext drawingContext, int currentStep)
        {
            var littleMarkStart = new Point(0, 0);
            var littleMarkEnd = new Point(0, 0);
            var sizeOfPen = this.GetSizeOfPen();
            if (this.Orientation == Orientation.Horizontal)
            {
                var pixelStep = ConvertMillimetersToPixel(this.Step, this.TrayWidth, this.DimensionWidth);
                littleMarkStart.X = (pixelStep * currentStep) + (pixelStep / 2);
                littleMarkStart.Y = this.Height - this.MiddleMarkLength + this.penHalfSize;
                littleMarkEnd.Y = this.ActualHeight - sizeOfPen;
                if (this.OriginHorizontal == OriginHorizontal.Right)
                {
                    littleMarkStart.X = this.ActualWidth - littleMarkStart.X;
                }

                littleMarkEnd.X = littleMarkStart.X;
                if (littleMarkEnd.X < 0)
                {
                    return;
                }

                if (littleMarkEnd.X > this.ActualWidth)
                {
                    return;
                }
            }
            else
            {
                var pixelStep = ConvertMillimetersToPixel(this.Step, this.TrayHeight, this.DimensionHeight);
                var pixelPosition = (pixelStep * currentStep) + (pixelStep / 2);
                littleMarkStart.Y = (this.OriginVertical == OriginVertical.Top) ? pixelPosition : this.ActualHeight - (pixelPosition + 1);
                littleMarkStart.X = this.Width - this.MiddleMarkLength + this.penHalfSize;
                littleMarkEnd.X = this.ActualWidth - sizeOfPen;
                littleMarkEnd.Y = littleMarkStart.Y;
                if (littleMarkEnd.Y < 0)
                {
                    return;
                }

                if (littleMarkEnd.Y > this.ActualHeight)
                {
                    return;
                }
            }

            this.DrawSnappedLinesBetweenPoints(drawingContext, littleMarkStart, littleMarkEnd);
        }

        private void DrawSnappedLinesBetweenPoints(DrawingContext dc, Point pointStart, Point pointEnd)
        {
            var guidelineSet = new GuidelineSet();
            guidelineSet.GuidelinesX.Add(pointStart.X);
            guidelineSet.GuidelinesY.Add(pointStart.Y);
            guidelineSet.GuidelinesX.Add(pointEnd.X);
            guidelineSet.GuidelinesY.Add(pointEnd.Y);
            dc.PushGuidelineSet(guidelineSet);
            var points = new Point[2];
            points[0] = new Point(pointStart.X + this.penHalfSize, pointStart.Y + this.penHalfSize);
            points[1] = new Point(pointEnd.X + this.penHalfSize, pointEnd.Y + this.penHalfSize);
            dc.DrawLine(this.pen, points[0], points[1]);
            dc.Pop();
        }

        private void DrawText(DrawingContext drawingContext, int currentStep, double fontSize)
        {
            var margin = this.GetSizeOfPen() * 2;
            var ft = new FormattedText(
                    (currentStep * this.Step).ToString(CultureInfo.InvariantCulture),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(this.FontFamily.ToString()),
                                 fontSize,
                                 this.ForegroundText,
                                 VisualTreeHelper.GetDpi(this).PixelsPerDip);
            double startFrom = 0;

            var position = new Point(0, 0);
            if (this.Orientation == Orientation.Horizontal)
            {
                var temp = currentStep * this.Step;
                position.X = ConvertMillimetersToPixel(temp, this.ActualWidth, this.DimensionWidth);

                position.Y = this.ActualHeight / TEXT_OFFSET_FACTOR;
                if (this.OriginHorizontal == OriginHorizontal.Left)
                {
                    position.X += margin;
                }
                else if (this.OriginHorizontal == OriginHorizontal.Right)
                {
                    startFrom = this.ActualWidth;
                    position.X = startFrom - position.X - ft.Width;
                    position.X -= margin;
                }

                if (position.X + ft.Width > this.ActualWidth)
                {
                    return;
                }
            }
            else
            {
                position.X = this.ActualWidth / TEXT_OFFSET_FACTOR;
                var temp = currentStep * this.Step;
                position.Y = ConvertMillimetersToPixel(temp, this.ActualHeight, this.DimensionHeight);
                if (this.OriginVertical == OriginVertical.Top)
                {
                    position.Y = position.Y + margin + ft.Width;
                    if (position.Y + ft.Width > this.ActualHeight)
                    {
                        return;
                    }
                }
                else if (this.OriginVertical == OriginVertical.Bottom)
                {
                    startFrom = this.ActualHeight;
                    position.Y = startFrom - position.Y;
                    position.Y -= OFFSET_BORDER;
                    if (position.Y - ft.Width < 0)
                    {
                        return;
                    }
                }

                drawingContext.PushTransform(new RotateTransform(-90, position.X, position.Y));
            }

            drawingContext.DrawText(ft, new Point(position.X, position.Y));
            if (this.Orientation == Orientation.Vertical)
            {
                drawingContext.Pop();
            }
        }

        private double GetFontSize()
        {
            var max = (this.DimensionHeight > this.DimensionWidth) ? this.DimensionHeight : this.DimensionWidth;
            var maxText = (int)(max / this.Step);
            var size = this.FontSize;
            while (size > 5 && this.CanShowText(maxText, size) == false)
            {
                size--;
            }

            return size;
        }

        private double GetHalfSizeOfPen()
        {
            return this.GetSizeOfPen() / 2.0;
        }

        private double GetSizeOfPen()
        {
            var ma = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            return WIDTH_MARK / ma.M11;
        }

        private double GetStepPixelSize(int intervalMark)
        {
            var dimension = (this.Orientation == Orientation.Horizontal) ? this.DimensionWidth : this.DimensionHeight;
            var barSize = (this.Orientation == Orientation.Horizontal) ? this.ActualWidth : this.ActualHeight;
            var size = this.Step / intervalMark;
            return ConvertMillimetersToPixel(size, barSize, dimension);
        }

        private void InitializePen()
        {
            this.penHalfSize = this.GetHalfSizeOfPen();
            this.pen = new Pen
            {
                DashCap = PenLineCap.Square,
                Brush = this.Foreground,
                Thickness = this.GetSizeOfPen(),
                StartLineCap = PenLineCap.Square,
                EndLineCap = PenLineCap.Square
            };
        }

        private void ShowOnlyBaseMarkers(DrawingContext drawingContext)
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                this.Height = 1;
            }
            else
            {
                this.Width = 1;
            }

            this.pen.Brush = Application.Current.Resources[DEFAULTBACKGROUND] as Brush;
            this.DrawBase(drawingContext);
        }

        #endregion
    }
}
