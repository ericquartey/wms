using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        //public readonly int LITTLEINTERVALSTEP = 20;
        public readonly int MAJORINTERVALSTEP = 100;

        //public readonly int MEDIUMINTERVALSTEP = 50;
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

        #endregion Properties

        #region Methods

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            double psuedoStartValue = 0;// StartValue;

            if (this.Orientation == enumOrientation.Horizontal)
            {
                for (int i = 0; i < this.ActualWidth / this.MajorIntervalHorizontal; i++)
                {
                    var ft = new FormattedText(
                        (psuedoStartValue * this.MAJORINTERVALSTEP).ToString(),
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
                    psuedoStartValue++;
                }
            }
            else
            {
                psuedoStartValue = 0;//;StartValue;
                for (int i = 0; i < this.ActualHeight / this.MajorIntervalVertical; i++)
                {
                    var ft = new FormattedText(
                        (psuedoStartValue * this.MAJORINTERVALSTEP).ToString(),//.ToString(),
                        System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                        new Typeface("Tahoma"), this.FONTSIZE, Brushes.Black);
                    var x = this.ActualWidth - ft.Height - this.OFFSET_TEXT;
                    var y = i * this.MajorIntervalVertical;
                    drawingContext.PushTransform(new RotateTransform(-90, x, y));
                    drawingContext.DrawText(ft, new Point(x, y));
                    drawingContext.Pop();
                    drawingContext.DrawLine(
                        new Pen(new SolidColorBrush(Colors.Black), 2),
                        new Point(this.MarkLength, i * this.MajorIntervalVertical),
                        new Point(0, i * this.MajorIntervalVertical));
                    drawingContext.DrawLine(
                        new Pen(new SolidColorBrush(Colors.Black), 1),
                        new Point(this.MiddleMarkLength, i * this.MajorIntervalVertical + (this.MajorIntervalVertical / 2)),
                        new Point(0, i * this.MajorIntervalVertical + (this.MajorIntervalVertical / 2)));
                    for (int j = 1; j < 10; j++)
                    {
                        var newPositionY = i * this.MajorIntervalVertical + ((this.MajorIntervalVertical * j) / 10);
                        if (newPositionY < this.ActualHeight)
                        {
                            if (j == 5)
                            {
                                continue;
                            }
                            drawingContext.DrawLine(
                                new Pen(new SolidColorBrush(Colors.Black), 1),
                                new Point(this.LittleMarkLength, newPositionY),
                                new Point(0, newPositionY));
                        }
                        else
                        {
                            break;
                        }
                    }

                    psuedoStartValue++;
                }
            }
        }

        #endregion Methods
    }
}
