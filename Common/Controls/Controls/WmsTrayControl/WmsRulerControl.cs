using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.Common.Controls
{
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
                                            DependencyProperty.Register("DisplayMode", typeof(enumOrientation), typeof(WmsRulerControl),
            new UIPropertyMetadata(enumOrientation.Horizontal));

        private readonly int fontSize = 10;

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

        public int MajorInterval
        {
            get { return (int)base.GetValue(MajorIntervalProperty); }
            set { this.SetValue(MajorIntervalProperty, value); }
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
                for (int i = 0; i < this.ActualWidth / this.MajorInterval; i++)
                {
                    var ft = new FormattedText(
                        (psuedoStartValue * this.MajorInterval).ToString(),
                        System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), fontSize, Brushes.Black);
                    drawingContext.DrawText(ft, new Point(i * this.MajorInterval, 0));
                    drawingContext.DrawLine(
                        new Pen(new SolidColorBrush(Colors.Black), 2),
                        new Point(i * this.MajorInterval, this.MarkLength),
                        new Point(i * this.MajorInterval, 0));
                    drawingContext.DrawLine(
                        new Pen(new SolidColorBrush(Colors.Black), 1),
                        new Point(i * this.MajorInterval + (this.MajorInterval / 2), this.MiddleMarkLength),
                        new Point(i * this.MajorInterval + (this.MajorInterval / 2), 0));
                    for (int j = 1; j < 10; j++)
                    {
                        if (j == 5)
                        {
                            continue;
                        }
                        drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1),
                        new Point(i * this.MajorInterval + (((this.MajorInterval * j) / 10)), this.LittleMarkLength),
                        new Point(i * this.MajorInterval + (((this.MajorInterval * j) / 10)), 0));
                    }
                    psuedoStartValue++;
                }
            }
            else
            {
                psuedoStartValue = 0;//;StartValue;
                for (int i = 0; i < this.ActualHeight / this.MajorInterval; i++)
                {
                    var ft = new FormattedText(
                        (psuedoStartValue * this.MajorInterval).ToString(),//.ToString(),
                        System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), fontSize, Brushes.Black);
                    drawingContext.DrawText(ft, new Point(0, i * this.MajorInterval));
                    drawingContext.DrawLine(
                        new Pen(new SolidColorBrush(Colors.Black), 2),
                        new Point(this.MarkLength, i * this.MajorInterval),
                        new Point(0, i * this.MajorInterval));
                    drawingContext.DrawLine(
                        new Pen(new SolidColorBrush(Colors.Black), 1),
                        new Point(this.MiddleMarkLength, i * this.MajorInterval + (this.MajorInterval / 2)),
                        new Point(0, i * this.MajorInterval + (this.MajorInterval / 2)));
                    for (int j = 1; j < 10; j++)
                    {
                        if (j == 5)
                        {
                            continue;
                        }
                        drawingContext.DrawLine(
                            new Pen(new SolidColorBrush(Colors.Black), 1),
                            new Point(this.LittleMarkLength, i * this.MajorInterval + ((this.MajorInterval * j) / 10)),
                            new Point(0, i * this.MajorInterval + ((this.MajorInterval * j) / 10)));
                    }
                    //drawingContext.DrawLine(
                    //    new Pen(new SolidColorBrush(Colors.Black), 2),
                    //    new Point(this.MarkLength, i * this.MajorInterval),
                    //    new Point(0, i * this.MajorInterval)
                    //    );
                    //drawingContext.DrawLine(
                    //    new Pen(new SolidColorBrush(Colors.Black), 1),
                    //    new Point(this.MarkLength, i * (this.MajorInterval)),
                    //    new Point(0, i * this.MajorInterval)
                    //    );
                    //drawingContext.DrawLine(
                    //    new Pen(new SolidColorBrush(Colors.Black), 1),
                    //    new Point(this.MiddleMarkLength, i * (this.MajorInterval + (this.MajorInterval / 2))),
                    //    new Point(0, i * (this.MajorInterval + (this.MajorInterval / 2)))
                    //    );
                    //for (int j = 1; j < 10; j++)
                    //{
                    //    if (j == 5)
                    //    {
                    //        continue;
                    //    }
                    //    drawingContext.DrawLine(
                    //        new Pen(new SolidColorBrush(Colors.Black), 1),
                    //        new Point(this.LittleMarkLength, i * (this.MajorInterval + (this.MajorInterval * j / 10))),
                    //        new Point(0, i * (this.MajorInterval + (this.MajorInterval * j / 10)))
                    //        );
                    //}
                    psuedoStartValue++;
                }
            }
        }

        #endregion Methods
    }
}
