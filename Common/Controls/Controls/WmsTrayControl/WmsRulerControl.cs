using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public enum OriginHorizontal { Left, Right }

    public enum OriginVertical { Top, Bottom }

    public class InfoRuler
    {
        #region Properties

        public Orientation OrientationRuler { get; set; }
        public OriginHorizontal OriginHorizontal { get; set; }
        public OriginVertical OriginVertical { get; set; }

        #endregion Properties
    }

    public class WmsRulerControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty LittleMarkLengthProperty =
                    DependencyProperty.Register("LittleMarkLengthProperty", typeof(int), typeof(WmsRulerControl),
                    new UIPropertyMetadata(8));

        public static readonly DependencyProperty MajorIntervalProperty =
                    DependencyProperty.Register("MajorIntervalProperty", typeof(int), typeof(WmsRulerControl),
                    new UIPropertyMetadata(100));

        public static readonly DependencyProperty MarkLengthProperty =
                    DependencyProperty.Register("MarkLengthProperty", typeof(int), typeof(WmsRulerControl),
                    new UIPropertyMetadata(20));

        public static readonly DependencyProperty MiddleMarkLengthProperty =
                    DependencyProperty.Register("MiddleMarkLengthProperty", typeof(int), typeof(WmsRulerControl),
                    new UIPropertyMetadata(14));

        public static readonly DependencyProperty OrientationProperty =
                    DependencyProperty.Register("DisplayMode", typeof(Orientation), typeof(WmsRulerControl),
                    new FrameworkPropertyMetadata(OnOrientationChanged));

        private readonly int BORDER = 1;
        private readonly int FONTSIZE = 10;
        private readonly int N_MARKS = 10;
        private readonly int OFFSET_BORDER = 2;
        private readonly int OFFSET_TEXT = 1;
        private readonly double startFrom = 0;
        private readonly int WIDTH_MARK = 1;
        private readonly int WIDTH_MARK_MINOR = 1;
        private Position origin;

        #endregion Fields

        #region Constructors

        public WmsRulerControl()
        {
            this.InfoRuler = new InfoRuler();
        }

        #endregion Constructors

        #region Properties

        public double HeightMmForRatio { get; set; }
        public InfoRuler InfoRuler { get; set; }

        public int LittleMarkLength
        {
            get => (int)this.GetValue(LittleMarkLengthProperty);
            set => this.SetValue(LittleMarkLengthProperty, value);
        }

        public int MajorInterval
        {
            get => (int)this.GetValue(MajorIntervalProperty);
            set => this.SetValue(MajorIntervalProperty, value);
        }

        public int MajorIntervalPixel { get; set; }

        public int MarkLength
        {
            get => (int)this.GetValue(MarkLengthProperty);
            set => this.SetValue(MarkLengthProperty, value);
        }

        public int MiddleMarkLength
        {
            get => (int)this.GetValue(MiddleMarkLengthProperty);
            set => this.SetValue(MiddleMarkLengthProperty, value);
        }

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        public Position Origin
        {
            get => this.origin;
            set
            {
                this.origin = value;
                if (value.X == 0)
                {
                    this.InfoRuler.OriginHorizontal = OriginHorizontal.Left;
                }
                else
                {
                    this.InfoRuler.OriginHorizontal = OriginHorizontal.Right;
                }
                if (value.Y == 0)
                {
                    this.InfoRuler.OriginVertical = OriginVertical.Top;
                }
                else
                {
                    this.InfoRuler.OriginVertical = OriginVertical.Bottom;
                }
            }
        }

        public double WidthMmForConvert { get; set; }
        public double WidthPixelForConvert { get; set; }

        #endregion Properties

        #region Methods

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            double pseudoStartValue = 0;
            double ratio = 0;

            switch (this.InfoRuler.OrientationRuler)
            {
                case Orientation.Horizontal:
                {
                    this.MarkLength = (int)this.ActualHeight;

                    if (this.ActualWidth > 0 && this.MajorIntervalPixel > 0)
                    {
                        ratio = this.WidthMmForConvert / this.MajorInterval;
                        if (double.IsNegativeInfinity(ratio) || double.IsPositiveInfinity(ratio))
                        {
                            ratio = 0;
                        }
                    }

                    break;
                }
                case Orientation.Vertical:
                {
                    this.MarkLength = (int)this.ActualWidth;

                    if (this.ActualHeight > 0 && this.MajorIntervalPixel > 0)
                    {
                        ratio = this.HeightMmForRatio / this.MajorInterval;
                        if (double.IsNegativeInfinity(ratio) || double.IsPositiveInfinity(ratio))
                        {
                            ratio = 0;
                        }
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!double.IsPositiveInfinity(ratio) && !double.IsNegativeInfinity(ratio) && ratio != 0)
            {
                ratio = Math.Round(ratio) + 1;
                for (var i = 0; i < ratio; i++)
                {
                    this.DrawRuler(drawingContext, i, ref pseudoStartValue, (int)ratio);
                }
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl rulerControl)
            {
                rulerControl.InfoRuler.OrientationRuler = (Orientation)e.NewValue;
            }
        }

        private void DrawEndDraw(ref DrawingContext drawingContext, double actualDimension)
        {
            var mark = new Line();

            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                //Ruler with origin Left
                mark.XStart = actualDimension;
                mark.XEnd = mark.XStart;
                mark.YStart = 0;
                mark.YEnd = this.MarkLength;
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    mark.XStart += this.startFrom;
                }
                else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    mark.XStart = this.BORDER;
                }

                mark.XEnd = mark.XStart;
            }
            else
            {
                //Ruler with origin Top
                mark.YStart = actualDimension;
                mark.YEnd = mark.YStart;
                mark.XStart = 0;
                mark.XEnd = this.MarkLength;
                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    mark.YStart += this.startFrom;
                }
                else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    mark.YStart = this.BORDER;
                }

                mark.YEnd = mark.YStart;
            }

            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Colors.Black), this.WIDTH_MARK),
                new Point(Math.Floor(mark.XStart), Math.Floor(mark.YStart)),
                new Point(Math.Floor(mark.XEnd), Math.Floor(mark.YEnd)));
        }

        private void DrawLittleMark(ref DrawingContext drawingContext, int i)
        {
            for (int j = 1; j < this.N_MARKS; j++)
            {
                var littleMark = new Line();
                if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
                {
                    var temp = i * this.MajorInterval + ((this.MajorInterval * j) / this.N_MARKS);
                    littleMark.XStart = GraphicUtils.ConvertMillimetersToPixel(temp, this.WidthPixelForConvert, this.WidthMmForConvert);

                    littleMark.XEnd = littleMark.XStart;
                    littleMark.YStart = 0;
                    littleMark.YEnd = this.LittleMarkLength;
                    if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                    {
                        littleMark.XStart += this.startFrom;
                        if (littleMark.XStart >= this.ActualWidth)
                        {
                            break;
                        }
                    }
                    else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                    {
                        littleMark.XStart = (this.ActualWidth - this.startFrom) - littleMark.XStart;
                        if (littleMark.XStart <= 0)
                        {
                            break;
                        }
                    }

                    littleMark.XEnd = littleMark.XStart;
                }
                else
                {
                    littleMark.XStart = 0;
                    littleMark.XEnd = this.LittleMarkLength;
                    var temp = i * this.MajorInterval + ((this.MajorInterval * j) / this.N_MARKS);
                    littleMark.YStart = GraphicUtils.ConvertMillimetersToPixel(temp, this.WidthPixelForConvert, this.WidthMmForConvert);
                    littleMark.YEnd = littleMark.YStart;
                    if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                    {
                        littleMark.YStart += this.startFrom;
                        if (littleMark.YStart >= this.ActualHeight)
                        {
                            break;
                        }
                    }
                    else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                    {
                        littleMark.YStart = (this.ActualHeight - this.startFrom) - littleMark.YStart;
                        if (littleMark.YStart <= 0)
                        {
                            break;
                        }
                    }

                    littleMark.YEnd = littleMark.YStart;
                }

                if (j == this.N_MARKS / 2)
                {
                    continue;
                }
                drawingContext.DrawLine(
                                new Pen(new SolidColorBrush(Colors.Black), this.WIDTH_MARK_MINOR),
                                new Point(Math.Floor(littleMark.XStart), Math.Floor(littleMark.YStart)),
                                new Point(Math.Floor(littleMark.XEnd), Math.Floor(littleMark.YEnd)));
            }
        }

        private void DrawMark(ref DrawingContext drawingContext, int i)
        {
            var mark = new Line();

            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                ////Ruler with origin Left
                mark.XStart = GraphicUtils.ConvertMillimetersToPixel((i * this.MajorInterval), this.WidthPixelForConvert, this.WidthMmForConvert);
                mark.YStart = 0;
                mark.YEnd = this.MarkLength;
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    mark.XStart += this.startFrom;
                }
                else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    mark.XStart = (this.ActualWidth - this.startFrom) - mark.XStart;
                }

                mark.XEnd = mark.XStart;
            }
            //Vertical Ruler
            else
            {
                //Ruler with origin Top
                mark.YStart = GraphicUtils.ConvertMillimetersToPixel((i * this.MajorInterval), this.WidthPixelForConvert, this.WidthMmForConvert);

                mark.XStart = 0;
                mark.XEnd = this.MarkLength;
                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    mark.YStart += this.startFrom;
                }
                else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    mark.YStart = (this.ActualHeight - this.startFrom) - mark.YStart;
                }

                mark.YEnd = mark.YStart;
            }

            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Colors.Black), this.WIDTH_MARK),
                new Point(Math.Round(mark.XStart), Math.Round(mark.YStart)),
                new Point(Math.Round(mark.XEnd), Math.Round(mark.YEnd)));
        }

        private void DrawMiddleMark(ref DrawingContext drawingContext, int i)
        {
            var middleMark = new Line();
            bool toDraw = true;
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                var temp = (i * this.MajorInterval + (this.MajorInterval / 2)) + 1;
                middleMark.XStart = GraphicUtils.ConvertMillimetersToPixel(temp, this.WidthPixelForConvert, this.WidthMmForConvert);
                middleMark.YStart = 0;
                middleMark.YEnd = this.MiddleMarkLength;

                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    middleMark.XStart += this.startFrom;
                    if (middleMark.XStart >= this.ActualWidth)
                    {
                        toDraw = false;
                    }
                }
                else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    middleMark.XStart = (this.ActualWidth - this.startFrom) - middleMark.XStart;
                    if (middleMark.XStart <= 0)
                    {
                        toDraw = false;
                    }
                }

                middleMark.XEnd = middleMark.XStart;
            }
            else
            {
                middleMark.XStart = 0;
                middleMark.XEnd = this.MiddleMarkLength;
                var temp = i * this.MajorInterval + (this.MajorInterval / 2);
                middleMark.YStart = GraphicUtils.ConvertMillimetersToPixel(temp, this.WidthPixelForConvert, this.WidthMmForConvert);
                middleMark.YEnd = middleMark.YStart;

                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    middleMark.YStart += this.startFrom;
                    if (middleMark.YStart >= this.ActualHeight)
                    {
                        toDraw = false;
                    }
                }
                else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    middleMark.YStart = (this.ActualHeight - this.startFrom) - middleMark.YStart;
                    if (middleMark.YStart <= 0)
                    {
                        toDraw = false;
                    }
                }

                middleMark.YEnd = middleMark.YStart;
            }

            if (toDraw)
            {
                drawingContext.DrawLine(
                   new Pen(new SolidColorBrush(Colors.Black), this.WIDTH_MARK_MINOR),
                   new Point(Math.Floor(middleMark.XStart), Math.Floor(middleMark.YStart)),
                   new Point(Math.Floor(middleMark.XEnd), Math.Floor(middleMark.YEnd)));
            }
        }

        private void DrawRuler(DrawingContext drawingContext, int i, ref double pseudoStartValue, int count)
        {
            var actualDimension = this.InfoRuler.OrientationRuler == Orientation.Horizontal ? this.ActualWidth : this.ActualHeight;

            this.DrawText(ref drawingContext, i, this.MajorInterval, pseudoStartValue);
            this.DrawMark(ref drawingContext, i);
            this.DrawMiddleMark(ref drawingContext, i);
            this.DrawLittleMark(ref drawingContext, i);
            if (i == count - 1)
            {
                this.DrawEndDraw(ref drawingContext, actualDimension);
            }

            pseudoStartValue++;
        }

        private void DrawText(ref DrawingContext drawingContext, int i, int majorInterval, double pseudoStartValue)
        {
            var ft = new FormattedText(
                    (pseudoStartValue * majorInterval).ToString(CultureInfo.InvariantCulture),
                    CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface("Tahoma"), this.FONTSIZE, Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
            var startFrom = 0;
            var toDraw = true;

            var position = new Position();
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                var temp = i * majorInterval;
                position.X = (int)GraphicUtils.ConvertMillimetersToPixel(temp, this.WidthPixelForConvert, this.WidthMmForConvert);

                position.Y = (int)(this.ActualHeight - ft.Height - this.OFFSET_TEXT);
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    position.X += this.OFFSET_BORDER;
                    if (position.X + ft.Width >= this.ActualWidth)
                    {
                        toDraw = false;
                    }
                }
                else if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    startFrom = (int)this.ActualWidth;
                    position.X = startFrom - position.X - (int)ft.Width;
                    position.X -= this.OFFSET_BORDER;

                    if (position.X <= 0)
                    {
                        toDraw = false;
                    }
                }
            }
            // Vertical Ruler
            else
            {
                position.X = (int)(this.ActualWidth - ft.Height - this.OFFSET_TEXT);
                var temp = i * majorInterval;
                position.Y = (int)GraphicUtils.ConvertMillimetersToPixel(temp, this.WidthPixelForConvert, this.WidthMmForConvert);

                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    position.Y += 5 + (int)ft.Height;

                    if (position.Y + ft.Height >= this.ActualHeight)
                    {
                        toDraw = false;
                    }
                }
                else if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    startFrom = (int)this.ActualHeight;
                    position.Y = startFrom - position.Y;
                    position.Y -= this.OFFSET_BORDER;

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

        #endregion Methods
    }
}
