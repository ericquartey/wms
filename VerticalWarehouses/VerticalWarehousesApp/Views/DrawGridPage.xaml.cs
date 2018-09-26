using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.VerticalWarehousesApp.Models;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    /// <summary>
    /// Interaction logic for DrawGrid.xaml
    /// </summary>
    public partial class DrawGridPage : Page
    {
        #region Fields

        private const double offset_writing = 7.5f;

        private const int passo = 150;

        private const int xOrigin = 20;

        private const int yOrigin = 15;

        private String[] colors = {"#000000",
"#00FF00",
"#0000FF",
"#FF0000",
"#01FFFE",
"#FFA6FE",
"#FFDB66",
"#006401",
"#010067",
"#95003A",
"#007DB5",
"#FF00F6",
"#FFEEE8",
"#774D00",
"#90FB92",
"#0076FF",
"#D5FF00",
"#FF937E",
"#6A826C",
"#FF029D",
"#FE8900",
"#7A4782",
"#7E2DD2",
"#85A900",
"#FF0056",
"#A42400",
"#00AE7E",
"#683D3B",
"#BDC6FF",
"#263400",
"#BDD393",
"#00B917",
"#9E008E",
"#001544",
"#C28C9F",
"#FF74A3",
"#01D0FF",
"#004754",
"#E56FFE",
"#788231",
"#0E4CA1",
"#91D0CB",
"#BE9970",
"#968AE8",
"#BB8800",
"#43002C",
"#DEFF74",
"#00FFC6",
"#FFE502",
"#620E00",
"#008F9C",
"#98FF52",
"#7544B1",
"#B500FF",
"#00FF78",
"#FF6E41",
"#005F39",
"#6B6882",
"#5FAD4E",
"#A75740",
"#A5FFD2",
"#FFB167",
"#009BFF",
"#E85EBE"};

        private int conversionePassoPixel;
        private string drawerSelected;

        private int HDrawerSelectedMM;
        private int HDrawerSelectedPixel;
        private int NDrawerSelected;
        private float PDrawerPixelMM;
        private List<Rect> rects;
        private int WDrawerSelectedMM;
        private int WDrawerSelectedPixel;
        private MainWindow window;

        #endregion Fields

        #region Constructors

        public DrawGridPage(MainWindow window, string drawerSelected)
        {
            this.window = window;
            this.InitializeComponent();

            //this.window.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //this.window.Arrange(new Rect(0, 0, this.window.DesiredSize.Width, this.window.DesiredSize.Height));

            this.drawerSelected = drawerSelected;
            var temp = this.drawerSelected.Split(',');
            var WxH = temp[1].Trim().Split('x');
            this.WDrawerSelectedMM = int.Parse(WxH[0].Trim());
            this.HDrawerSelectedMM = int.Parse(WxH[1].Trim());

            this.rects = new List<Rect>();

            this.InfoCassetto.Text = this.drawerSelected;

            Loaded += this.DrawGridPage_Loaded;
            //this.CreateRectangleDrawer();
            //this.DrawNumberMetric();
        }

        #endregion Constructors

        #region Methods

        public Color GetRandomColor()
        {
            //Random randonGen = new Random();
            //Color randomColor =
            //    Color.FromArgb(
            //    (byte)randonGen.Next(0, 200),
            //    (byte)randonGen.Next(0, 200),
            //    (byte)randonGen.Next(0, 200),
            //    (byte)randonGen.Next(0, 200));

            //return randomColor;

            Random r = new Random();
            var rInt = r.Next(0, this.colors.Length);
            ColorConverter converter = new ColorConverter();
            Color newColor = (Color)converter.ConvertFromInvariantString((this.colors[rInt]));
            return newColor;
        }

        protected void UserControl_ButtonClick(object sender, EventArgs e)
        {
            //handle the event
            Console.WriteLine("Btn Clik!!");

            var InputCompartment = new InputCompartment()
            {
                PositionX = this.InputCompartmentControl.GetPositionX(),
                PositionY = this.InputCompartmentControl.GetPositionY(),
                Width = this.InputCompartmentControl.GetWidth(),
                Height = this.InputCompartmentControl.GetHeight()
            };
            this.DrawNewCompartment(InputCompartment);
        }

        private void CreateRectangleDrawer()
        {
            //< Rectangle
            //    Canvas.Left = "20"
            //    Canvas.Top = "15"
            //    Width = "751"
            //    Height = "251"
            //    Stroke = "#FFA21919" />
            var rectangle = new Rectangle();
            Canvas.SetLeft(rectangle, xOrigin);
            Canvas.SetTop(rectangle, yOrigin);

            // TODO: calculate W & H window
            var widthCanvas = this.canvas.ActualWidth;
            var heightCanvas = this.canvas.ActualHeight;
            this.WDrawerSelectedPixel = (int)widthCanvas - 50;

            this.conversionePassoPixel = (this.WDrawerSelectedPixel * passo) / this.WDrawerSelectedMM;
            this.HDrawerSelectedPixel = (this.WDrawerSelectedPixel * this.HDrawerSelectedMM) / this.WDrawerSelectedMM;

            //this.WDrawerSelectedPixel = this.HDrawerSelectedMM * this.conversionePassoPixel;
            // 750=W
            // 750 : WD = x : HD -> x = (750 * HD) / WD
            this.PDrawerPixelMM = this.WDrawerSelectedPixel / this.WDrawerSelectedMM;

            rectangle.Width = this.WDrawerSelectedPixel;
            rectangle.Height = this.HDrawerSelectedPixel;
            rectangle.Stroke = new SolidColorBrush(Colors.Red);
            this.canvas.Children.Add(rectangle);
        }

        private void DrawGridPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.CreateRectangleDrawer();
            this.DrawNumberMetric();
            this.DrawTestRectangle();
        }

        private void DrawNewCompartment(InputCompartment ic)
        {
            var x = ((this.WDrawerSelectedPixel * (ic.PositionX)) / this.WDrawerSelectedMM) + xOrigin;
            var y = ((this.WDrawerSelectedPixel * (ic.PositionY)) / this.WDrawerSelectedMM) + yOrigin;
            var w = (this.WDrawerSelectedPixel * ic.Width) / this.WDrawerSelectedMM;
            var h = (this.WDrawerSelectedPixel * ic.Height) / this.WDrawerSelectedMM;
            Rect rectTemp = new Rect(x, y, w, h);
            bool intersect = this.IntersectNewRectangle(rectTemp);
            if (intersect)
            {
                Rectangle rect = new Rectangle();
                //rect.PointToScreen(new Point(0,0));
                Canvas.SetLeft(rect, x);// ic.PositionX + xOrigin);
                Canvas.SetTop(rect, y);//ic.PositionY + yOrigin);
                rect.Width = w;// ic.Width;
                rect.Height = h;// ic.Height;
                rect.Fill = new SolidColorBrush(this.GetRandomColor());
                rect.Opacity = 0.2f;
                this.canvas.Children.Add(rect);
                this.rects.Add(rectTemp);
            }
            else
            {
                MessageBox.Show("Errore: è presente un altro scompartimento", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DrawNumberMetric()
        {
            //W pixel / 5(passo)

            var nMisureWidth = this.WDrawerSelectedPixel / this.conversionePassoPixel;
            var nMisureHeight = this.HDrawerSelectedPixel / this.conversionePassoPixel;

            nMisureWidth += 2;
            nMisureHeight += 2;

            var passoPixel = this.conversionePassoPixel * passo;

            this.griglia.Viewport = new Rect(xOrigin, yOrigin, this.conversionePassoPixel, this.conversionePassoPixel);

            for (int i = 0; i < nMisureWidth; i++)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = $"{passo * i}";
                textBlock.Foreground = new SolidColorBrush(Colors.Blue);
                Canvas.SetLeft(textBlock, ((/*square*/this.conversionePassoPixel * i) + offset_writing));
                Canvas.SetTop(textBlock, 0);
                this.canvas.Children.Add(textBlock);
                textBlock = new TextBlock();
                textBlock.Text = $"{passo * i}";
                if (i <= nMisureHeight)
                {
                    textBlock.Foreground = new SolidColorBrush(Colors.Blue);
                    Canvas.SetLeft(textBlock, 0);
                    Canvas.SetTop(textBlock, ((/*square*/this.conversionePassoPixel * i) + offset_writing));
                    this.canvas.Children.Add(textBlock);
                }
            }
        }

        private void DrawTestRectangle()
        {
            Rectangle rect = new Rectangle();
            //rect.Offset(new System.Drawing.Point(xOrigin, yOrigin));
            Canvas.SetLeft(rect, xOrigin);
            Canvas.SetTop(rect, yOrigin);
            rect.Width = this.conversionePassoPixel;
            rect.Height = this.conversionePassoPixel;
            rect.Fill = new SolidColorBrush(Colors.Red);
            rect.Opacity = 0.2f;
            this.canvas.Children.Add(rect);
        }

        private void InputCompartment_Loaded(Object sender, RoutedEventArgs e)
        {
            Console.WriteLine("DrawGridPage::InputCompartment_Loaded");
            this.InputCompartmentControl.ButtonClick += new EventHandler(this.UserControl_ButtonClick);
        }

        private bool IntersectNewRectangle(Rect b)
        {
            foreach (var a in this.rects)
            {
                System.Drawing.Rectangle rectangle1, rectangle2;
                if (a.Left < b.Left)
                {
                    rectangle1 = new System.Drawing.Rectangle((int)a.Top, (int)a.Left, (int)a.Width, (int)a.Height);
                    rectangle2 = new System.Drawing.Rectangle((int)b.Top, (int)b.Left, (int)b.Width, (int)b.Height);
                }
                else
                {
                    rectangle2 = new System.Drawing.Rectangle((int)a.Top, (int)a.Left, (int)a.Width, (int)a.Height);
                    rectangle1 = new System.Drawing.Rectangle((int)b.Top, (int)b.Left, (int)b.Width, (int)b.Height);
                }

                //bool intersect = a.IntersectsWith(b);
                //if(intersect)
                //{
                //    return false;
                //}
                System.Drawing.Rectangle temp = System.Drawing.Rectangle.Intersect(rectangle1, rectangle2);
                if (temp.Width > 0 && temp.Height > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        #endregion Methods
    }
}
