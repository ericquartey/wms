using System.Diagnostics;
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

        public static readonly DependencyProperty MajorIntervalHorizontalProperty =
                    DependencyProperty.Register("MajorIntervalHorizontalProperty", typeof(int), typeof(WmsRulerControl),
                    new UIPropertyMetadata(100));

        public static readonly DependencyProperty MajorIntervalVerticalProperty =
                    DependencyProperty.Register(nameof(MajorIntervalVertical), typeof(int), typeof(WmsRulerControl),
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

        private readonly int FONTSIZE = 10;
        private readonly int N_MARKS = 10;
        private readonly int OFFSET_TEXT = 1;
        private Position origin;

        #endregion Fields

        #region Constructors

        public WmsRulerControl()
        {
            this.InfoRuler = new InfoRuler();
        }

        #endregion Constructors

        #region Properties

        public InfoRuler InfoRuler { get; set; }

        public int LittleMarkLength
        {
            get { return (int)this.GetValue(LittleMarkLengthProperty); }
            set { this.SetValue(LittleMarkLengthProperty, value); }
        }

        public int MajorIntervalHorizontal
        {
            get
            {
                return (int)this.GetValue(MajorIntervalHorizontalProperty);
            }
            set { this.SetValue(MajorIntervalHorizontalProperty, value); }
        }

        public int MajorIntervalHorizontalPixel { get; set; }

        public int MajorIntervalVertical
        {
            get
            {
                return (int)this.GetValue(MajorIntervalVerticalProperty);
            }
            set { this.SetValue(MajorIntervalVerticalProperty, value); }
        }

        public int MajorIntervalVerticalPixel { get; set; }

        public int MarkLength
        {
            get { return (int)this.GetValue(MarkLengthProperty); }
            set { this.SetValue(MarkLengthProperty, value); }
        }

        public int MiddleMarkLength
        {
            get { return (int)this.GetValue(MiddleMarkLengthProperty); }
            set { this.SetValue(MiddleMarkLengthProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)this.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        public Position Origin
        {
            get => this.origin;
            set
            {
                this.origin = value;
                if (value.XPosition == 0)
                {
                    this.InfoRuler.OriginHorizontal = OriginHorizontal.Left;
                }
                else
                {
                    this.InfoRuler.OriginHorizontal = OriginHorizontal.Right;
                }
                if (value.YPosition == 0)
                {
                    this.InfoRuler.OriginVertical = OriginVertical.Top;
                }
                else
                {
                    this.InfoRuler.OriginVertical = OriginVertical.Bottom;
                }
            }
        }

        #endregion Properties

        #region Methods

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            double pseudoStartValue = 0;
            double ratio = 0;

            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                if (this.ActualWidth > 0 && this.MajorIntervalHorizontalPixel > 0)
                {
                    ratio = this.ActualWidth / this.MajorIntervalHorizontalPixel;
                    if (double.IsNegativeInfinity(ratio) || double.IsPositiveInfinity(ratio))
                    {
                        ratio = 0;
                    }
                }
            }
            if (this.InfoRuler.OrientationRuler == Orientation.Vertical)
            {
                if (this.ActualHeight > 0 && this.MajorIntervalVerticalPixel > 0)
                {
                    ratio = this.ActualHeight / this.MajorIntervalVerticalPixel;
                    if (double.IsNegativeInfinity(ratio) || double.IsPositiveInfinity(ratio))
                    {
                        ratio = 0;
                    }
                }
            }
            Debug.WriteLine($"RulerControl: OnRender() orientation: {this.InfoRuler.OrientationRuler}, ratio: {ratio}");
            Debug.WriteLine($"INFO: ActualWidth= {this.ActualWidth}  MajorIntervalHorizontalPixel= {this.MajorIntervalHorizontalPixel}");
            if (!double.IsPositiveInfinity(ratio) && !double.IsNegativeInfinity(ratio))
            {
                for (int i = 0; i < ratio; i++)
                {
                    this.DrawRuler(drawingContext, i, ref pseudoStartValue);
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

        private void DrawLittleMark(ref DrawingContext drawingContext, int i, int majorIntervalPixel)
        {
            int startFrom = 0;
            for (int j = 1; j < this.N_MARKS; j++)
            {
                var littleMark = new Line();
                if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
                {
                    littleMark.XStart = i * majorIntervalPixel + ((majorIntervalPixel * j) / this.N_MARKS);
                    littleMark.XEnd = littleMark.XStart;
                    littleMark.YStart = 0;
                    littleMark.YEnd = this.LittleMarkLength;
                    if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                    {
                        if (littleMark.XEnd >= this.ActualWidth)
                        {
                            break;
                        }
                    }
                    if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                    {
                        startFrom = (int)this.ActualWidth;
                        littleMark.XStart = startFrom - littleMark.XStart;
                        littleMark.XEnd = littleMark.XStart;
                        if (littleMark.XEnd <= 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    littleMark.XStart = 0;
                    littleMark.XEnd = this.LittleMarkLength;
                    littleMark.YStart = i * majorIntervalPixel + ((majorIntervalPixel * j) / this.N_MARKS);
                    littleMark.YEnd = littleMark.YStart;
                    if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                    {
                        if (littleMark.YEnd >= this.ActualHeight)
                        {
                            break;
                        }
                    }
                    if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                    {
                        startFrom = (int)this.ActualHeight;
                        littleMark.YStart = startFrom - littleMark.YStart;
                        littleMark.YEnd = littleMark.YStart;
                        if (littleMark.YEnd <= 0)
                        {
                            break;
                        }
                    }
                }

                if (j == this.N_MARKS / 2)
                {
                    continue;
                }
                drawingContext.DrawLine(
                                new Pen(new SolidColorBrush(Colors.Black), 1),
                                new Point(littleMark.XStart, littleMark.YStart),
                                new Point(littleMark.XEnd, littleMark.YEnd));
            }
        }

        private void DrawMark(ref DrawingContext drawingContext, int i, int majorIntervalPixel)
        {
            var mark = new Line();
            int startFrom = 0;

            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                //Ruler with origin Left
                mark.XStart = i * majorIntervalPixel;
                mark.XEnd = mark.XStart;
                mark.YStart = 0;
                mark.YEnd = this.MarkLength;

                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    startFrom = (int)this.ActualWidth;
                    mark.XStart = startFrom - mark.XStart;
                    mark.XEnd = mark.XStart;
                }
            }
            else
            {
                //Ruler with origin Top
                mark.YStart = i * majorIntervalPixel;
                mark.YEnd = mark.YStart;
                mark.XStart = 0;
                mark.XEnd = this.MarkLength;

                if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    startFrom = (int)this.ActualHeight;
                    mark.YStart = startFrom - mark.YStart;
                    mark.YEnd = mark.YStart;
                }
            }

            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Colors.Black), 2),
                new Point(mark.XStart, mark.YStart),
                new Point(mark.XEnd, mark.YEnd));
        }

        private void DrawMiddleMark(ref DrawingContext drawingContext, int i, int majorIntervalPixel)
        {
            var middleMark = new Line();
            bool toDraw = true;
            int startFrom = 0;
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                middleMark.XStart = i * majorIntervalPixel + (majorIntervalPixel / 2);
                middleMark.XEnd = middleMark.XStart;
                middleMark.YStart = 0;
                middleMark.YEnd = this.MiddleMarkLength;

                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    if (middleMark.XEnd >= this.ActualWidth)
                    {
                        toDraw = false;
                    }
                }
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    startFrom = (int)this.ActualWidth;
                    middleMark.XStart = startFrom - middleMark.XStart;
                    middleMark.XEnd = middleMark.XStart;
                    if (middleMark.XEnd <= 0)
                    {
                        toDraw = false;
                    }
                }
            }
            else
            {
                middleMark.XStart = 0;
                middleMark.XEnd = this.MiddleMarkLength;
                middleMark.YStart = i * majorIntervalPixel + (majorIntervalPixel / 2);
                middleMark.YEnd = middleMark.YStart;

                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    if (middleMark.YEnd >= this.ActualHeight)
                    {
                        toDraw = false;
                    }
                }
                if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    startFrom = (int)this.ActualHeight;
                    middleMark.YStart = startFrom - middleMark.YStart;
                    middleMark.YEnd = middleMark.YStart;
                    if (middleMark.YEnd <= 0)
                    {
                        toDraw = false;
                    }
                }
            }
            if (toDraw)
            {
                drawingContext.DrawLine(
                   new Pen(new SolidColorBrush(Colors.Black), 1),
                   new Point(middleMark.XStart, middleMark.YStart),
                   new Point(middleMark.XEnd, middleMark.YEnd));
            }
        }

        private void DrawRuler(DrawingContext drawingContext, int i, ref double pseudoStartValue)
        {
            double actualDimension = 0;
            int majorInterval = 0;
            int majorIntervalPixel = 0;

            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                actualDimension = this.ActualWidth;
                majorInterval = this.MajorIntervalHorizontal;
                majorIntervalPixel = this.MajorIntervalHorizontalPixel;
            }
            else
            {
                actualDimension = this.ActualHeight;
                majorInterval = this.MajorIntervalVertical;
                majorIntervalPixel = this.MajorIntervalVerticalPixel;
            }

            this.DrawText(ref drawingContext, i, majorInterval, majorIntervalPixel, pseudoStartValue);
            this.DrawMark(ref drawingContext, i, majorIntervalPixel);
            this.DrawMiddleMark(ref drawingContext, i, majorIntervalPixel);
            this.DrawLittleMark(ref drawingContext, i, majorIntervalPixel);

            pseudoStartValue++;
        }

        private void DrawText(ref DrawingContext drawingContext, int i, int majorInterval, int majorIntervalPixel, double pseudoStartValue)
        {
            var ft = new FormattedText(
                    (pseudoStartValue * majorInterval).ToString(),
                    CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface("Tahoma"), this.FONTSIZE, Brushes.Black);
            int startFrom = 0;
            bool toDraw = true;

            var position = new Position();
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                position.XPosition = i * majorIntervalPixel;
                position.YPosition = (int)(this.ActualHeight - ft.Height - this.OFFSET_TEXT);
                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    if (position.XPosition >= this.ActualWidth)
                    {
                        toDraw = false;
                    }
                }

                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                {
                    startFrom = (int)this.ActualWidth;
                    position.XPosition = startFrom - position.XPosition;
                    if (position.XPosition <= 0)
                    {
                        toDraw = false;
                    }
                }
            }
            else
            {
                position.XPosition = (int)(this.ActualWidth - ft.Height - this.OFFSET_TEXT);
                position.YPosition = i * majorIntervalPixel;
                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    if (position.YPosition >= this.ActualHeight)
                    {
                        toDraw = false;
                    }
                }
                if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    startFrom = (int)this.ActualHeight;
                    position.YPosition = startFrom - position.YPosition;
                    if (position.YPosition <= 0)
                    {
                        toDraw = false;
                    }
                }
                if (toDraw)
                {
                    drawingContext.PushTransform(new RotateTransform(-90, position.XPosition, position.YPosition));
                }
            }
            if (toDraw)
            {
                drawingContext.DrawText(ft, new Point(position.XPosition, position.YPosition));
                if (this.InfoRuler.OrientationRuler == Orientation.Vertical)
                {
                    drawingContext.Pop();
                }
            }
        }

        #endregion Methods
    }
}
