using System;
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

        private const int LITTLE_INTERVALMARKS = 10;

        private const int MIDDLE_INTERVALMARKS = 2;

        private const int OFFSET_BORDER = 2;

        private const double WIDTH_MARK = 1;

        private readonly InfoRuler infoRuler;

        #endregion Fields

        #region Constructors

        public WmsRulerControl()
        {
            this.infoRuler = new InfoRuler();
            this.UseLayoutRounding = false;
            this.SnapsToDevicePixels = false;
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        #endregion Constructors

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

        #endregion Properties

        #region Methods

        public static double ConvertMillimetersToPixel(double value, double pixel, double mm)
        {
            return mm > 0 ? (pixel * value / mm) : value;
        }

        public void Redraw()
        {
            var size = new Size(this.ActualWidth, this.ActualHeight);
            Application.Current.MainWindow.Measure(size);
            this.Arrange(new Rect(this.DesiredSize));
        }

        public void SetHorizontal()
        {
            if (this.OriginX == 0)
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
                this.SetHorizontal();
                this.SetVertical();
                totalSteps = totalSteps + 1;
                this.DrawRuler(drawingContext, totalSteps);
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

        private bool CanBeShowed(int intervalMark, int factor)
        {
            var pixelSize = this.GetStepPixelSize(intervalMark);
            return pixelSize > (this.GetSizeOfPen() * factor);
        }

        private bool CanShowText(int maxTextLenght)
        {
            var pixelSize = this.GetStepPixelSize(1);
            var margin = this.GetSizeOfPen() * 2;
            var ft = new FormattedText(
                    (maxTextLenght * this.Step).ToString(CultureInfo.InvariantCulture),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(this.FontFamily.ToString()),
                                 this.FontSize,
                                 this.ForegroundText,
                                 VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return pixelSize > (ft.Width + margin);
        }

        private void DrawBase(DrawingContext drawingContext)
        {
            var mark = new Line();
            var offSet = this.GetHalfSizeOfPen();
            var halfOfHalf = offSet / 2;

            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                mark.XStart = offSet;
                mark.XEnd = this.ActualWidth;
                mark.YStart = this.ActualHeight - halfOfHalf;
                mark.YEnd = mark.YStart;
            }
            else
            {
                mark.XStart = this.ActualWidth - halfOfHalf;
                mark.XEnd = mark.XStart;
                mark.YStart = halfOfHalf;
                mark.YEnd = this.ActualHeight - halfOfHalf;
            }

            this.DrawLine(drawingContext, mark);
        }

        private void DrawEndMark(DrawingContext drawingContext)
        {
            var mark = new Line();
            var halfSize = this.GetHalfSizeOfPen();
            var halfOfHalf = halfSize / 2;
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                mark.XStart = this.ActualWidth;
                mark.YStart = halfOfHalf;
                mark.YEnd = this.ActualHeight - halfOfHalf;
                mark.XStart = (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left) ? mark.XStart + halfSize : this.ActualWidth - mark.XStart - halfSize;
                mark.XEnd = mark.XStart;
            }
            else
            {
                mark.YStart = -halfSize;
                mark.XStart = halfOfHalf;
                mark.XEnd = this.ActualWidth - halfOfHalf;
                mark.YStart = (this.InfoRuler.OriginVertical == OriginVertical.Top) ? mark.YStart + halfSize : mark.YStart;
                mark.YEnd = mark.YStart;
            }

            this.DrawLine(drawingContext, mark);
        }

        private void DrawLine(DrawingContext drawingContext, Line m)
        {
            if (m == null)
            {
                throw new ArgumentNullException(nameof(m));
            }

            var penSize = this.GetHalfSizeOfPen();
            var halfPenSize = this.GetHalfSizeOfPen();
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(m.XStart + halfPenSize);
            guidelines.GuidelinesX.Add(m.XEnd + halfPenSize);
            guidelines.GuidelinesY.Add(m.YStart + halfPenSize);
            guidelines.GuidelinesY.Add(m.YEnd + halfPenSize);
            drawingContext.PushGuidelineSet(guidelines);
            var penScaled = new Pen
            {
                DashCap = PenLineCap.Square,
                Brush = this.Foreground,
                Thickness = penSize,
                StartLineCap = PenLineCap.Square,
                EndLineCap = PenLineCap.Square
            };
            drawingContext.DrawLine(penScaled, new Point(m.XStart, m.YStart), new Point(m.XEnd, m.YEnd));
            drawingContext.Pop();
        }

        private void DrawLittleMark(DrawingContext drawingContext, int currentStep)
        {
            var halfSize = this.GetHalfSizeOfPen();
            var halfOfHalf = halfSize / 2;
            for (var j = 1; j < LITTLE_INTERVALMARKS; j++)
            {
                if (j == LITTLE_INTERVALMARKS / 2)
                {
                    continue;
                }

                var littleMark = new Line();
                if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
                {
                    var temp = (currentStep * this.Step) + ((this.Step * j) / LITTLE_INTERVALMARKS);
                    littleMark.XStart = ConvertMillimetersToPixel(temp, this.ActualWidth, this.DimensionWidth) + halfOfHalf;
                    littleMark.XEnd = littleMark.XStart;
                    littleMark.YStart = this.Height - this.LittleMarkLength + halfSize;
                    littleMark.YEnd = this.Height - halfSize;
                    if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                    {
                        if (littleMark.XStart >= this.ActualWidth)
                        {
                            break;
                        }
                    }
                    else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                    {
                        littleMark.XStart = this.ActualWidth - littleMark.XStart;
                        if (littleMark.XStart <= 0)
                        {
                            break;
                        }
                    }

                    littleMark.XEnd = littleMark.XStart;
                }
                else
                {
                    littleMark.XStart = this.ActualWidth - this.LittleMarkLength + halfSize;
                    littleMark.XEnd = this.ActualWidth - halfSize;
                    var temp = (currentStep * this.Step) + ((this.Step * j) / LITTLE_INTERVALMARKS);
                    littleMark.YStart = ConvertMillimetersToPixel(temp, this.ActualHeight, this.DimensionHeight) + halfOfHalf;
                    littleMark.YEnd = littleMark.YStart;
                    if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                    {
                        if (littleMark.YStart >= this.ActualHeight)
                        {
                            break;
                        }
                    }
                    else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                    {
                        littleMark.YStart = this.ActualHeight - littleMark.YStart;
                        if (littleMark.YStart <= 0)
                        {
                            break;
                        }
                    }

                    littleMark.YEnd = littleMark.YStart;
                }

                this.DrawLine(drawingContext, littleMark);
            }
        }

        private void DrawMark(DrawingContext drawingContext, int currentStep)
        {
            var mark = new Line();
            var halfSize = this.GetHalfSizeOfPen() / 2;
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                mark.XStart = ConvertMillimetersToPixel(currentStep * this.Step, this.ActualWidth, this.DimensionWidth);
                mark.YStart = halfSize;
                mark.YEnd = this.ActualHeight - halfSize;
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    mark.XStart += halfSize;
                }

                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    mark.XStart = this.ActualWidth - mark.XStart - halfSize;
                }

                mark.XEnd = mark.XStart;
            }
            else
            {
                mark.YStart = ConvertMillimetersToPixel(currentStep * this.Step, this.ActualHeight, this.DimensionHeight);

                mark.XStart = halfSize;
                mark.XEnd = this.ActualWidth - halfSize;
                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    mark.YStart += halfSize;
                }

                if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    mark.YStart = this.ActualHeight - mark.YStart - halfSize;
                }

                mark.YEnd = mark.YStart;
            }

            this.DrawLine(drawingContext, mark);
        }

        private void DrawMiddleMark(DrawingContext drawingContext, int currentStep)
        {
            var middleMark = new Line();
            var halfSize = this.GetHalfSizeOfPen();
            var halfOfHalfSize = halfSize / 2;
            var temp = (currentStep * this.Step) + (this.Step / 2);
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                middleMark.XStart = ConvertMillimetersToPixel(temp, this.ActualWidth, this.DimensionWidth);
                middleMark.YStart = this.Height - this.MiddleMarkLength + halfSize;
                middleMark.YEnd = this.Height - halfSize;

                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    middleMark.XStart += halfOfHalfSize;
                }
                else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    middleMark.XStart = this.ActualWidth - middleMark.XStart - halfSize;
                }

                middleMark.XEnd = middleMark.XStart;
            }
            else
            {
                middleMark.XStart = this.Width - this.MiddleMarkLength + halfSize;
                middleMark.XEnd = this.Width - halfSize;
                middleMark.YStart = ConvertMillimetersToPixel(temp, this.ActualHeight, this.DimensionHeight);
                middleMark.YEnd = middleMark.YStart + halfSize;

                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    middleMark.YStart += halfOfHalfSize;
                }
                else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    middleMark.YStart = this.ActualHeight - middleMark.YStart - halfOfHalfSize;
                }

                middleMark.YEnd = middleMark.YStart;
            }

            this.DrawLine(drawingContext, middleMark);
        }

        private void DrawRuler(DrawingContext drawingContext, int totalSteps)
        {
            var isBaseDrawVisible = false;
            var canShowText = this.CanShowText(totalSteps);
            var canShowMarks = this.CanBeShowed(1, 20);
            var canShowMiddleMarks = this.CanBeShowed(MIDDLE_INTERVALMARKS, 13);
            var canShowLittleMarks = this.CanBeShowed(LITTLE_INTERVALMARKS, 4);

            for (var currStep = 0; currStep < totalSteps; currStep++)
            {
                if (this.ShowInfo && canShowText)
                {
                    this.DrawText(drawingContext, currStep);
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

                if (this.ShowMiddleMark && canShowMiddleMarks &&
                    currStep < (totalSteps - 1))
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

            this.DrawEndMark(drawingContext);
        }

        private void DrawText(DrawingContext drawingContext, int currentStep)
        {
            double margin = this.GetSizeOfPen() * 2;
            var ft = new FormattedText(
                    (currentStep * this.Step).ToString(CultureInfo.InvariantCulture),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(this.FontFamily.ToString()),
                                 this.FontSize,
                                 this.ForegroundText,
                                 VisualTreeHelper.GetDpi(this).PixelsPerDip);
            double startFrom = 0;
            var toDraw = true;

            var position = new Position();
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                var temp = currentStep * this.Step;
                position.X = ConvertMillimetersToPixel(temp, this.ActualWidth, this.DimensionWidth);

                position.Y = this.ActualHeight / 20.0;
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    position.X += margin;
                    if (position.X + ft.Width >= this.ActualWidth)
                    {
                        toDraw = false;
                    }
                }
                else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    startFrom = this.ActualWidth;
                    position.X = startFrom - position.X - ft.Width;
                    position.X -= margin;

                    if (position.X <= 0)
                    {
                        toDraw = false;
                    }
                }
            }
            else
            {
                position.X = this.ActualWidth / 20;
                var temp = currentStep * this.Step;
                position.Y = ConvertMillimetersToPixel(temp, this.ActualHeight, this.DimensionHeight);
                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    position.Y = position.Y + margin + ft.Width;
                    if (position.Y + ft.Height >= this.ActualHeight)
                    {
                        toDraw = false;
                    }
                }
                else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    startFrom = this.ActualHeight;
                    position.Y = startFrom - position.Y;
                    position.Y -= OFFSET_BORDER;

                    if (position.Y - ft.Width <= 0)
                    {
                        toDraw = false;
                    }
                }

                if (toDraw)
                {
                    drawingContext.PushTransform(new RotateTransform(-90, position.X, position.Y));
                }
            }

            if (toDraw)
            {
                drawingContext.DrawText(ft, new Point(position.X, position.Y));
                if (this.InfoRuler.OrientationRuler == Orientation.Vertical)
                {
                    drawingContext.Pop();
                }
            }
        }

        private double GetHalfSizeOfPen()
        {
            return this.GetSizeOfPen() / 2.0;
        }

        private double GetSizeOfPen()
        {
            return WIDTH_MARK * VisualTreeHelper.GetDpi(this).PixelsPerDip;
        }

        private double GetStepPixelSize(int intervalMark)
        {
            var dimension = (this.InfoRuler.OrientationRuler == Orientation.Horizontal) ? this.DimensionWidth : this.DimensionHeight;
            var barSize = (this.InfoRuler.OrientationRuler == Orientation.Horizontal) ? this.ActualWidth : this.ActualHeight;
            var size = this.Step / intervalMark;
            return ConvertMillimetersToPixel(size, barSize, dimension);
        }

        private void SetVertical()
        {
            if (this.OriginY == 0)
            {
                this.InfoRuler.OriginVertical = OriginVertical.Top;
            }
            else
            {
                this.InfoRuler.OriginVertical = OriginVertical.Bottom;
            }
        }

        #endregion Methods
    }
}
