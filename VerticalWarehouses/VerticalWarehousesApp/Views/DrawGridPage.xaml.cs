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

        private const int square = 25;

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

        private List<Rect> rects;

        #endregion Fields

        #region Constructors

        public DrawGridPage()
        {
            this.InitializeComponent();
            this.rects = new List<Rect>();
            Rectangle rect = new Rectangle();
            //rect.Offset(new System.Drawing.Point(xOrigin, yOrigin));
            Canvas.SetLeft(rect, xOrigin);
            Canvas.SetTop(rect, yOrigin);
            rect.Width = 50;
            rect.Height = 50;
            rect.Fill = new SolidColorBrush(Colors.Red);
            rect.Opacity = 0.2f;
            this.canvas.Children.Add(rect);

            for (int i = 0; i < 10; i++)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = $"{square * i}";
                textBlock.Foreground = new SolidColorBrush(Colors.Blue);
                Canvas.SetLeft(textBlock, ((square * i) + offset_writing));
                Canvas.SetTop(textBlock, 0);
                this.canvas.Children.Add(textBlock);
                textBlock = new TextBlock();
                textBlock.Text = $"{square * i}";
                textBlock.Foreground = new SolidColorBrush(Colors.Blue);
                Canvas.SetLeft(textBlock, 0);
                Canvas.SetTop(textBlock, ((square * i) + offset_writing));
                this.canvas.Children.Add(textBlock);
            }
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

        private void DrawNewCompartment(InputCompartment ic)
        {
            var x = ic.PositionX + xOrigin;
            var y = ic.PositionY + yOrigin;
            var w = ic.Width;
            var h = ic.Height;
            Rect rectTemp = new Rect(x, y, w, h);
            bool intersect = this.IntersectNewRectangle(rectTemp);
            if (intersect)
            {
                Rectangle rect = new Rectangle();
                //rect.PointToScreen(new Point(0,0));
                Canvas.SetLeft(rect, ic.PositionX + xOrigin);
                Canvas.SetTop(rect, ic.PositionY + yOrigin);
                rect.Width = ic.Width;
                rect.Height = ic.Height;
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

        private void InputCompartment_Loaded(Object sender, RoutedEventArgs e)
        {
            Console.WriteLine("DrawGridPage::InputCompartment_Loaded");
            this.InputCompartmentControl.ButtonClick += new EventHandler(this.UserControl_ButtonClick);
        }

        private bool IntersectNewRectangle(Rect b)
        {
            foreach (var a in this.rects)
            {
                var rectangle1 = new System.Drawing.Rectangle((int)a.Top, (int)a.Left, (int)a.Width, (int)a.Height);
                var rectangle2 = new System.Drawing.Rectangle((int)b.Top, (int)b.Left, (int)b.Width, (int)b.Height);
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
