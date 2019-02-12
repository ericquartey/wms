using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class WmsRulerControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CurrentFontSizeProperty =
                    DependencyProperty.Register(nameof(CurrentFontSize), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(OnCurrentFontSizeChanged));

        public static readonly DependencyProperty DimensionHeightProperty =
                    DependencyProperty.Register(nameof(DimensionHeight), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty DimensionWidthProperty =
            DependencyProperty.Register(nameof(DimensionWidth), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty ForegroundTextProperty =
            DependencyProperty.Register(nameof(ForegroundText), typeof(Brush), typeof(WmsRulerControl), new UIPropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty LittleMarkLengthProperty =
                    DependencyProperty.Register(nameof(LittleMarkLength), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(8.0));

        public static readonly DependencyProperty MiddleMarkLengthProperty =
                    DependencyProperty.Register(nameof(MiddleMarkLength), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(14.0));

        public static readonly DependencyProperty OrientationProperty =
                    DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(WmsRulerControl), new FrameworkPropertyMetadata(OnOrientationChanged));

        public static readonly DependencyProperty OriginXProperty =
            DependencyProperty.Register(nameof(OriginX), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(0.0, OnOriginXChanged));

        public static readonly DependencyProperty OriginYProperty =
            DependencyProperty.Register(nameof(OriginY), typeof(double), typeof(WmsRulerControl), new UIPropertyMetadata(0.0, OnOriginYChanged));

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

        private const int LITTLE_INTERVALMARKS = 10;

        private const int MIDDLE_INTERVALMARKS = 2;

        private const int OFFSET_BORDER = 2;

        private const double TEXT_OFFSET_FACTOR = 20;

        private const double WIDTH_MARK = 1;

        private readonly InfoRuler infoRuler;

        private Pen pen;

        private double penHalfSize;

        #endregion

        #region Constructors

        public WmsRulerControl()
        {
            this.infoRuler = new InfoRuler();
            this.UseLayoutRounding = false;
            this.SnapsToDevicePixels = false;
            var target = this;
            RenderOptions.SetEdgeMode(target, EdgeMode.Aliased);
        }

        #endregion

        #region Properties

        public double CurrentFontSize
        {
            get => (double)this.GetValue(CurrentFontSizeProperty);
            set => this.SetValue(CurrentFontSizeProperty, value);
        }

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

        public InfoRuler InfoRuler => this.infoRuler;

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

        public double OriginX
        {
            get => (double)this.GetValue(OriginXProperty);
            set => this.SetValue(OriginXProperty, value);
        }

        public double OriginY
        {
            get => (double)this.GetValue(OriginYProperty);
            set => this.SetValue(OriginYProperty, value);
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

        public void SetHorizontal()
        {
            if (this.OriginX.Equals(0))
            {
                this.InfoRuler.OriginHorizontal = OriginHorizontal.Left;
            }
            else
            {
                this.InfoRuler.OriginHorizontal = OriginHorizontal.Right;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var totalSteps = 0;

            switch (this.InfoRuler.OrientationRuler)
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
                this.SetHorizontal();
                this.SetVertical();
                this.DrawMarkers(drawingContext, totalSteps);
            }
        }

        private static void OnCurrentFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl ruler &&
                e.NewValue is double currentFontSize &&
                currentFontSize.Equals(ruler.FontSize) == false)
            {
                ruler.Redraw();
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl rulerControl)
            {
                rulerControl.InfoRuler.OrientationRuler = (Orientation)e.NewValue;
            }
        }

        private static void OnOriginXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl ruler && e.NewValue is double v)
            {
                ruler.UpdateLayout();
            }
        }

        private static void OnOriginYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl ruler && e.NewValue is double v)
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
            var mark = new Line();
            var sizeOfPen = this.GetSizeOfPen();

            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                mark.YStart = this.ActualHeight - sizeOfPen;
                mark.YEnd = mark.YStart;
                mark.XStart = 0;
                mark.XEnd = this.TrayWidth - sizeOfPen;
            }
            else
            {
                mark.XStart = this.ActualWidth - sizeOfPen;
                mark.XEnd = mark.XStart;
                mark.YStart = 0;
                mark.YEnd = this.TrayHeight - sizeOfPen;
            }

            this.DrawSnappedLinesBetweenPoints(drawingContext, mark);
        }

        private void DrawLittleMark(DrawingContext drawingContext, int currentStep)
        {
            double stepPixel = 0;
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
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

                var littleMark = new Line();
                if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
                {
                    littleMark.XStart = basePixelStart + (littlePixelStep * j);
                    littleMark.YStart = this.ActualHeight - this.LittleMarkLength + this.penHalfSize;
                    littleMark.YEnd = this.ActualHeight - this.penHalfSize;
                    if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                    {
                        littleMark.XStart = this.ActualWidth - littleMark.XStart;
                    }

                    littleMark.XEnd = littleMark.XStart;
                    if (littleMark.XEnd > this.ActualWidth)
                    {
                        return;
                    }
                }
                else
                {
                    var pixelPosition = basePixelStart + (littlePixelStep * j);
                    littleMark.YStart = (this.InfoRuler.OriginVertical == OriginVertical.Top) ? pixelPosition : this.ActualHeight - pixelPosition - 1;
                    littleMark.XStart = this.ActualWidth - this.LittleMarkLength + this.penHalfSize;
                    littleMark.XEnd = this.ActualWidth - this.penHalfSize - this.GetSizeOfPen();
                    littleMark.YEnd = littleMark.YStart;
                    if (littleMark.YEnd > this.ActualHeight)
                    {
                        return;
                    }
                }

                this.DrawSnappedLinesBetweenPoints(drawingContext, littleMark);
            }
        }

        private void DrawMark(DrawingContext drawingContext, int currentStep)
        {
            var mark = new Line();
            var sizeOfPen = this.GetSizeOfPen();
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                mark.XStart = ConvertMillimetersToPixel(this.Step * currentStep, this.TrayWidth, this.DimensionWidth);
                mark.YStart = this.penHalfSize;
                mark.YEnd = this.ActualHeight - this.penHalfSize;
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    mark.XStart = this.ActualWidth - mark.XStart;
                }

                mark.XEnd = mark.XStart;
            }
            else
            {
                var offSet = ConvertMillimetersToPixel(this.Step * currentStep, this.TrayHeight, this.DimensionHeight);
                mark.XStart = this.penHalfSize;
                mark.XEnd = this.ActualWidth - this.penHalfSize;
                mark.YStart = (this.InfoRuler.OriginVertical == OriginVertical.Top) ? offSet - 0 : this.ActualHeight - offSet - sizeOfPen;
                mark.YEnd = mark.YStart;
            }

            this.DrawSnappedLinesBetweenPoints(drawingContext, mark);
        }

        private void DrawMarkers(DrawingContext drawingContext, int totalSteps)
        {
            var isBaseDrawVisible = false;
            totalSteps = totalSteps + 1;
            this.CurrentFontSize = this.GetFontSize(totalSteps);
            var canShowText = this.CanShowText(totalSteps, this.CurrentFontSize);
            var canShowMarks = this.CanBeShown(1, 15);
            var canShowMiddleMarks = this.CanBeShown(MIDDLE_INTERVALMARKS, 13);
            var canShowLittleMarks = this.CanBeShown(LITTLE_INTERVALMARKS, 4);

            for (var currStep = 0; currStep < totalSteps; currStep++)
            {
                if (this.ShowInfo && canShowText)
                {
                    this.DrawText(drawingContext, currStep, this.CurrentFontSize);
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
            var middleMark = new Line();
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                var pixelStep = ConvertMillimetersToPixel(this.Step, this.TrayWidth, this.DimensionWidth);
                middleMark.XStart = (pixelStep * currentStep) + (pixelStep / 2);
                middleMark.YStart = this.Height - this.MiddleMarkLength + this.penHalfSize;
                middleMark.YEnd = this.Height - this.penHalfSize;
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    middleMark.XStart = this.ActualWidth - middleMark.XStart;
                }

                middleMark.XEnd = middleMark.XStart;
            }
            else
            {
                var pixelStep = ConvertMillimetersToPixel(this.Step, this.TrayHeight, this.DimensionHeight);
                var pixelPosition = (pixelStep * currentStep) + (pixelStep / 2);
                middleMark.YStart = (this.InfoRuler.OriginVertical == OriginVertical.Top) ? pixelPosition : this.ActualHeight - (pixelPosition + 1);
                middleMark.XStart = this.Width - this.MiddleMarkLength + this.penHalfSize;
                middleMark.XEnd = this.Width - this.penHalfSize;
                middleMark.YEnd = middleMark.YStart;
            }

            this.DrawSnappedLinesBetweenPoints(drawingContext, middleMark);
        }

        private void DrawSnappedLinesBetweenPoints(DrawingContext dc, Line mark)
        {
            var guidelineSet = new GuidelineSet();
            guidelineSet.GuidelinesX.Add(mark.XStart);
            guidelineSet.GuidelinesY.Add(mark.YStart);
            guidelineSet.GuidelinesX.Add(mark.XEnd);
            guidelineSet.GuidelinesY.Add(mark.YEnd);
            dc.PushGuidelineSet(guidelineSet);
            var points = new Point[2];
            points[0] = new Point(mark.XStart + this.penHalfSize, mark.YStart + this.penHalfSize);
            points[1] = new Point(mark.XEnd + this.penHalfSize, mark.YEnd + this.penHalfSize);
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

            var position = new Position();
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                var temp = currentStep * this.Step;
                position.X = ConvertMillimetersToPixel(temp, this.ActualWidth, this.DimensionWidth);

                position.Y = this.ActualHeight / TEXT_OFFSET_FACTOR;
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    position.X += margin;
                }
                else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
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
                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    position.Y = position.Y + margin + ft.Width;
                }
                else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    startFrom = this.ActualHeight;
                    position.Y = startFrom - position.Y;
                    position.Y -= OFFSET_BORDER;
                }

                if (position.Y + ft.Width > this.ActualHeight)
                {
                    return;
                }

                drawingContext.PushTransform(new RotateTransform(-90, position.X, position.Y));
            }

            drawingContext.DrawText(ft, new Point(position.X, position.Y));
            if (this.InfoRuler.OrientationRuler == Orientation.Vertical)
            {
                drawingContext.Pop();
            }
        }

        private double GetFontSize(int totText)
        {
            var size = (this.CurrentFontSize > 0 && this.CurrentFontSize < this.FontSize) ? this.CurrentFontSize : this.FontSize;
            while (size > 5 && this.CanShowText(totText, size) == false)
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
            var dimension = (this.InfoRuler.OrientationRuler == Orientation.Horizontal) ? this.DimensionWidth : this.DimensionHeight;
            var barSize = (this.InfoRuler.OrientationRuler == Orientation.Horizontal) ? this.ActualWidth : this.ActualHeight;
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

        private void SetVertical()
        {
            if (this.OriginY.Equals(0))
            {
                this.InfoRuler.OriginVertical = OriginVertical.Top;
            }
            else
            {
                this.InfoRuler.OriginVertical = OriginVertical.Bottom;
            }
        }

        #endregion
    }
}
