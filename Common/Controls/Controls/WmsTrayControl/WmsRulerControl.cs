using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
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
                                            DependencyProperty.Register("DisplayMode", typeof(enumOrientation), typeof(WmsRulerControl),
            new UIPropertyMetadata(enumOrientation.Horizontal));

        public readonly int MAJORINTERVALSTEP = 100;
        private readonly int FONTSIZE = 10;
        private readonly int OFFSET_TEXT = 1;

        #endregion Fields

        #region Constructors

        public WmsRulerControl()
        {
            //this.InitializeComponent();
        }

        #endregion Constructors

        #region Enums

        public enum enumOrientation { Horizontal, Vertical }

        public enum OriginVertical { Top, Bottom }

        #endregion Enums

        #region Properties

        public int LittleMarkLength
        {
            get { return (int)base.GetValue(LittleMarkLengthProperty); }
            set { this.SetValue(LittleMarkLengthProperty, value); }
        }

        public int MajorIntervalHorizontal
        {
            get { return (int)base.GetValue(MajorIntervalHorizontalProperty); }
            set { this.SetValue(MajorIntervalHorizontalProperty, value); }
        }

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

        public enumOrientation Orientation
        {
            get { return (enumOrientation)base.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        public Position Origin { get; set; }

        #endregion Properties

        #region Methods

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            double pseudoStartValue = 0;// StartValue;

            if (this.Orientation == enumOrientation.Horizontal)
            {
                for (int i = 0; i < this.ActualWidth / this.MajorIntervalHorizontal; i++)
                {
                    this.DrawHorizontalRuler(drawingContext, i, ref pseudoStartValue);
                }
            }
            else
            {
                pseudoStartValue = 0;//;StartValue;
                if (Origin != null)
                {
                    OriginVertical originStart = OriginVertical.Top;
                    if (Origin.XPosition == 0 && Origin.YPosition == 0)
                    {
                        originStart = OriginVertical.Top;
                        for (int i = 0; i < this.ActualHeight / this.MajorIntervalVertical; i++)
                        {
                            this.DrawVerticalRuler(drawingContext, i, ref pseudoStartValue, originStart);
                        }
                    }
                    if (Origin.XPosition == 0 && Origin.YPosition != 0)
                    {
                        pseudoStartValue = 0;

                        originStart = OriginVertical.Bottom;
                        //for (int i = (int)this.ActualHeight / this.MajorIntervalVertical; i >= 0; i--)
                        //{
                        //    this.DrawVerticalRuler(drawingContext, i, ref pseudoStartValue, originStart);
                        //}
                    }
                    pseudoStartValue = 0;
                    for (int i = 0; i < this.ActualHeight / this.MajorIntervalVertical; i++)
                    {
                        this.DrawVerticalRuler(drawingContext, i, ref pseudoStartValue, originStart);
                    }
                }
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

        private void DrawVerticalRuler(DrawingContext drawingContext, int i, ref double psuedoStartValue, OriginVertical originVertical)
        {
            int startFrom = 0, xFinal = 0, xFinalMiddle = 0, xFinalLittle = 0, xText = 0, yText = 0;

            var ft = new FormattedText(
                        (psuedoStartValue * this.MAJORINTERVALSTEP).ToString(),//.ToString(),
                        System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                        new Typeface("Tahoma"), this.FONTSIZE, Brushes.Black);

            if (originVertical == OriginVertical.Top)
            {
                startFrom = 0;
                xFinal = i * this.MajorIntervalVertical;
                xFinalMiddle = i * this.MajorIntervalVertical + (this.MajorIntervalVertical / 2);
                xText = (int)(this.ActualWidth - ft.Height - this.OFFSET_TEXT);
                yText = i * this.MajorIntervalVertical;
            }
            if (originVertical == OriginVertical.Bottom)
            {
                startFrom = (int)this.ActualHeight;
                xFinal = startFrom - (i * this.MajorIntervalVertical);
                xFinalMiddle = startFrom - (i * this.MajorIntervalVertical + (this.MajorIntervalVertical / 2));
                xText = (int)(this.ActualWidth - ft.Height + this.OFFSET_TEXT);
                yText = startFrom - (i * this.MajorIntervalVertical);
            }

            drawingContext.PushTransform(new RotateTransform(-90, xText, yText));
            drawingContext.DrawText(ft, new Point(xText, yText));
            drawingContext.Pop();
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Colors.Black), 2),
                new Point(this.MarkLength, xFinal),
                new Point(0, xFinal));
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Colors.Black), 1),
                new Point(this.MiddleMarkLength, xFinalMiddle),
                new Point(0, xFinalMiddle));
            for (int j = 1; j < 10; j++)
            {
                if (originVertical == OriginVertical.Top)
                {
                    startFrom = 0;
                    xFinalLittle = i * this.MajorIntervalVertical + ((this.MajorIntervalVertical * j) / 10);
                    if (xFinalLittle >= this.ActualHeight)
                    {
                        break;
                    }
                }
                if (originVertical == OriginVertical.Bottom)
                {
                    startFrom = (int)this.ActualHeight;
                    xFinalLittle = startFrom - (i * this.MajorIntervalVertical + ((this.MajorIntervalVertical * j) / 10));
                    if (xFinalLittle <= 0)
                    {
                        break;
                    }
                }

                if (j == 5)
                {
                    continue;
                }
                drawingContext.DrawLine(
                    new Pen(new SolidColorBrush(Colors.Black), 1),
                    new Point(this.LittleMarkLength, xFinalLittle),
                    new Point(0, xFinalLittle));
            }

            psuedoStartValue++;
        }

        #endregion Methods
    }
}
