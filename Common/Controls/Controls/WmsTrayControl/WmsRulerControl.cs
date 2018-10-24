using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public enum Orientation { Horizontal, Vertical }

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

        //Lunghezza trattini piccoli
        public static readonly DependencyProperty LittleMarkLengthProperty =
                            DependencyProperty.Register("LittleMarkLengthProperty", typeof(int), typeof(WmsRulerControl),
            new UIPropertyMetadata(8));

        public static readonly DependencyProperty MajorIntervalHorizontalProperty =
                            DependencyProperty.Register("MajorIntervalHorizontalProperty", typeof(int), typeof(WmsRulerControl),
                    new UIPropertyMetadata(100));

        //Intervallo Grandi (100)
        public static readonly DependencyProperty MajorIntervalVerticalProperty =
                    DependencyProperty.Register("MajorIntervalVerticalProperty", typeof(int), typeof(WmsRulerControl),
            new UIPropertyMetadata(100));

        //Lunghezza trattini Grandi
        public static readonly DependencyProperty MarkLengthProperty =
            DependencyProperty.Register("MarkLengthProperty", typeof(int), typeof(WmsRulerControl),
            new UIPropertyMetadata(20));

        //Lunghezza trattini Medi
        public static readonly DependencyProperty MiddleMarkLengthProperty =
            DependencyProperty.Register("MiddleMarkLengthProperty", typeof(int), typeof(WmsRulerControl),
            new UIPropertyMetadata(14));

        public static readonly DependencyProperty OrientationProperty =
                                            DependencyProperty.Register("DisplayMode", typeof(Orientation), typeof(WmsRulerControl),
                                                 //,new UIPropertyMetadata(Orientation.Horizontal)
                                                 new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOrientationChanged)));

        public readonly int MAJORINTERVALSTEP = 100;

        private readonly int FONTSIZE = 10;

        private readonly int OFFSET_TEXT = 1;

        private Position origin;

        #endregion Fields

        #region Constructors

        public WmsRulerControl()
        {
            //this.InitializeComponent();
            this.InfoRuler = new InfoRuler();
        }

        #endregion Constructors

        #region Properties

        public InfoRuler InfoRuler { get; set; }

        public int LittleMarkLength
        {
            get { return (int)base.GetValue(LittleMarkLengthProperty); }
            set { this.SetValue(LittleMarkLengthProperty, value); }
        }

        //public enum OriginVertical { Top, Bottom }
        public int MajorIntervalHorizontal
        {
            get { return (int)base.GetValue(MajorIntervalHorizontalProperty); }
            set { this.SetValue(MajorIntervalHorizontalProperty, value); }
        }

        //public enum enumOrientation { Horizontal, Vertical }
        public int MajorIntervalVertical
        {
            get { return (int)base.GetValue(MajorIntervalVerticalProperty); }
            set
            {
                this.SetValue(MajorIntervalVerticalProperty, value);
            }
        }

        public int MarkLength
        {
            get { return (int)base.GetValue(MarkLengthProperty); }
            set { this.SetValue(MarkLengthProperty, value); }
        }

        public int MiddleMarkLength
        {
            get { return (int)base.GetValue(MiddleMarkLengthProperty); }
            set { this.SetValue(MiddleMarkLengthProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)base.GetValue(OrientationProperty); }
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
            double pseudoStartValue = 0;// StartValue;
            double ratio = 0;
            //if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            //{
            //    for (int i = 0; i < this.ActualWidth / this.MajorIntervalHorizontal; i++)
            //    {
            //        this.DrawHorizontalRuler(drawingContext, i, ref pseudoStartValue);
            //    }
            //}
            //else
            //{
            //    pseudoStartValue = 0;//;StartValue;
            //    if (this.Origin != null)
            //    {
            //OriginVertical originStart = OriginVertical.Top;
            //if (this.Origin.XPosition == 0 && this.Origin.YPosition == 0)
            //{
            //    originStart = OriginVertical.Top;
            //    for (int i = 0; i < this.ActualHeight / this.MajorIntervalVertical; i++)
            //    {
            //        this.DrawVerticalRuler(drawingContext, i, ref pseudoStartValue, originStart);
            //    }
            //}
            //if (this.Origin.XPosition == 0 && this.Origin.YPosition != 0)
            //{
            //    pseudoStartValue = 0;

            //    //originStart = OriginVertical.Bottom;
            //    //for (int i = (int)this.ActualHeight / this.MajorIntervalVertical; i >= 0; i--)
            //    //{
            //    //    this.DrawVerticalRuler(drawingContext, i, ref pseudoStartValue, originStart);
            //    //}
            //}
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                ratio = this.ActualWidth / this.MajorIntervalHorizontal;
            }
            if (this.InfoRuler.OrientationRuler == Orientation.Vertical)
            {
                ratio = this.ActualHeight / this.MajorIntervalVertical;
            }

            for (int i = 0; i < ratio; i++)
            {
                this.DrawVerticalRuler(drawingContext, i, ref pseudoStartValue);
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsRulerControl rulerControl)
            {
                rulerControl.InfoRuler.OrientationRuler = (Orientation)e.NewValue;
            }
        }

        private void DrawHorizontalRuler(DrawingContext drawingContext, int i, ref double pseudoStartValue)
        {
            var ft = new FormattedText(
                        (pseudoStartValue * this.MAJORINTERVALSTEP).ToString(),
                        System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                        new Typeface("Tahoma"), this.FONTSIZE, Brushes.Black);
            drawingContext.DrawText(ft, new Point(i * this.MajorIntervalHorizontal - ft.Width - this.OFFSET_TEXT, this.ActualHeight - ft.Height));
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Colors.Black), 2),
                new Point(i * this.MajorIntervalHorizontal, this.MarkLength),
                new Point(i * this.MajorIntervalHorizontal, 0));
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Colors.Black), 1),
                new Point(i * this.MajorIntervalHorizontal + (this.MajorIntervalHorizontal / 2), this.MiddleMarkLength),
                new Point(i * this.MajorIntervalHorizontal + (this.MajorIntervalHorizontal / 2), 0));
            for (int j = 1; j < 10; j++)
            {
                var newPositionX = i * this.MajorIntervalHorizontal + (((this.MajorIntervalHorizontal * j) / 10));
                if (newPositionX < this.ActualWidth)
                {
                    if (j == 5)
                    {
                        continue;
                    }
                    drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1),
                    new Point(newPositionX, this.LittleMarkLength),
                    new Point(newPositionX, 0));
                }
                else
                {
                    break;
                }
            }
            pseudoStartValue++;
        }

        private void DrawVerticalRuler(DrawingContext drawingContext, int i, ref double psuedoStartValue)
        {
            int xStart = 0, xStartMiddle = 0, xStartLittle = 0;
            int startFrom = 0, xFinal = 0, xFinalMiddle = 0, xFinalLittle = 0, xText = 0, yText = 0;
            int yStart = 0, yStartMiddle = 0, yStartLittle = 0;
            int yFinal = 0, yFinalMiddle = 0, yFinalLittle = 0;
            int majorInterval = 0;

            var ft = new FormattedText(
                        (psuedoStartValue * this.MAJORINTERVALSTEP).ToString(),//.ToString(),
                        System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                        new Typeface("Tahoma"), this.FONTSIZE, Brushes.Black);
            if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
            {
                majorInterval = this.MajorIntervalHorizontal;

                if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                {
                    ///////////////////////////
                    startFrom = 0;
                    xStart = i * majorInterval;
                    xFinal = xStart;
                    yStart = 0;
                    yFinal = this.MarkLength;
                    xStartMiddle = i * majorInterval + (majorInterval / 2);
                    xFinalMiddle = xStartMiddle;
                    yStartMiddle = 0;
                    yFinalMiddle = this.MiddleMarkLength;
                    xText = i * majorInterval;
                    yText = (int)(this.ActualHeight - ft.Height - this.OFFSET_TEXT);
                }
                else
                {
                    startFrom = (int)this.ActualWidth;
                    xStart = startFrom - (i * majorInterval);
                    xFinal = xStart;
                    yStart = 0;
                    yFinal = this.MarkLength;
                    xStartMiddle = startFrom - (i * majorInterval + (majorInterval / 2));
                    xFinalMiddle = xStartMiddle;
                    yStartMiddle = 0;
                    yFinalMiddle = this.MiddleMarkLength;
                    xText = startFrom - (i * majorInterval);
                    yText = (int)(this.ActualHeight - ft.Height + this.OFFSET_TEXT);
                }
            }
            else
            {
                //VERTICAL
                majorInterval = this.MajorIntervalVertical;

                if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                {
                    startFrom = 0;
                    xStart = 0;
                    xFinal = this.MarkLength;
                    yStart = i * majorInterval;
                    yFinal = yStart;
                    xStartMiddle = 0;
                    xFinalMiddle = this.MiddleMarkLength;
                    yStartMiddle = i * majorInterval + (majorInterval / 2);
                    yFinalMiddle = yStartMiddle;
                    xText = (int)(this.ActualWidth - ft.Height - this.OFFSET_TEXT);
                    yText = i * majorInterval;
                }
                if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                {
                    startFrom = (int)this.ActualHeight;
                    xStart = 0;
                    xFinal = this.MarkLength;
                    yStart = startFrom - (i * majorInterval);
                    yFinal = yStart;
                    xStartMiddle = 0;
                    xFinalMiddle = this.MiddleMarkLength;
                    yStartMiddle = startFrom - (i * majorInterval + (majorInterval / 2));
                    yFinalMiddle = yStartMiddle;
                    xText = (int)(this.ActualWidth - ft.Height + this.OFFSET_TEXT);
                    yText = startFrom - (i * majorInterval);
                }
                drawingContext.PushTransform(new RotateTransform(-90, xText, yText));
            }

            drawingContext.DrawText(ft, new Point(xText, yText));
            if (this.InfoRuler.OrientationRuler == Orientation.Vertical)
            {
                drawingContext.Pop();
            }
            drawingContext.DrawLine(
            new Pen(new SolidColorBrush(Colors.Black), 2),
            new Point(xStart, yStart),
            new Point(xFinal, yFinal));
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Colors.Black), 1),
                new Point(xStartMiddle, yStartMiddle),
                new Point(xFinalMiddle, yFinalMiddle));
            for (int j = 1; j < 10; j++)
            {
                if (this.InfoRuler.OrientationRuler == Orientation.Horizontal)
                {
                    if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Left)
                    {
                        startFrom = 0;
                        xStartLittle = i * majorInterval + ((majorInterval * j) / 10);
                        xFinalLittle = xStartLittle;
                        yStartLittle = 0;
                        yFinalLittle = this.LittleMarkLength;
                        if (xFinalLittle >= this.ActualWidth)
                        {
                            break;
                        }
                    }
                    if (this.InfoRuler.OriginHorizontal == OriginHorizontal.Right)
                    {
                        startFrom = (int)this.ActualWidth;
                        xStartLittle = startFrom - (i * majorInterval + ((majorInterval * j) / 10));
                        xFinalLittle = xStartLittle;
                        yStartLittle = 0;
                        yFinalLittle = this.LittleMarkLength;
                        if (xFinalLittle <= 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (this.InfoRuler.OriginVertical == OriginVertical.Top)
                    {
                        startFrom = 0;
                        xStartLittle = 0;
                        xFinalLittle = this.LittleMarkLength;
                        yStartLittle = i * majorInterval + ((majorInterval * j) / 10);
                        yFinalLittle = yStartLittle;
                        if (yFinalLittle >= this.ActualHeight)
                        {
                            break;
                        }
                    }
                    if (this.InfoRuler.OriginVertical == OriginVertical.Bottom)
                    {
                        startFrom = (int)this.ActualHeight;
                        xStartLittle = 0;
                        xFinalLittle = this.LittleMarkLength;
                        yStartLittle = startFrom - (i * majorInterval + ((majorInterval * j) / 10));
                        yFinalLittle = yStartLittle;
                        if (yFinalLittle <= 0)
                        {
                            break;
                        }
                    }
                }

                if (j == 5)
                {
                    continue;
                }
                drawingContext.DrawLine(
                    new Pen(new SolidColorBrush(Colors.Black), 1),
                    new Point(xStartLittle, yStartLittle),
                    new Point(xFinalLittle, yFinalLittle));
            }

            psuedoStartValue++;
        }

        #endregion Methods
    }
}
