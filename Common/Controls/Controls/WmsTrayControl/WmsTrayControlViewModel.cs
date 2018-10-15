using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    public class WmsTrayControlViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<WmsBaseCompartment> items;
        private int left;
        private SolidColorBrush penBrush;
        private int penThickness;
        private int top;
        private double trayHeight;
        private double trayWidth;

        private int xxx = 800;

        #endregion Fields

        #region Constructors

        public WmsTrayControlViewModel()
        {
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public ObservableCollection<WmsBaseCompartment> Items { get { return this.items; } set { this.items = value; } }

        public int Left
        {
            get { return this.left; }
            set
            {
                this.left = value;
                this.NotifyPropertyChanged(nameof(this.Left));
            }
        }

        public LoadingUnitDetails LoadingUnitProperty { get; set; }

        public SolidColorBrush PenBrush
        {
            get { return this.penBrush; }
            set
            {
                this.penBrush = value;
                this.NotifyPropertyChanged(nameof(this.PenBrush));
            }
        }

        public int PenThickness
        {
            get { return this.penThickness; }
            set
            {
                this.penThickness = value;
                this.NotifyPropertyChanged(nameof(this.PenThickness));
            }
        }

        public int Top
        {
            get { return this.top; }
            set { this.top = value; }
        }

        public double TrayHeight
        {
            get { return this.trayHeight; }
            set
            {
                this.trayHeight = value;
                this.NotifyPropertyChanged(nameof(this.TrayHeight));
            }
        }

        public double TrayWidth
        {
            get { return this.trayWidth; }
            set
            {
                this.trayWidth = value;
                this.NotifyPropertyChanged(nameof(this.TrayWidth));
            }
        }

        #endregion Properties

        #region Methods

        public void Resize(double widthTrayPixel, double heightTrayPixel)
        {
            this.TrayHeight = xxx;// this.LoadingUnitProperty.Length;
            this.TrayWidth = xxx;//this.LoadingUnitProperty.Width;

            //double ratioW = widthTrayPixel / this.LoadingUnitProperty.Width;
            //double ratioH = heightTrayPixel / this.LoadingUnitProperty.Length;

            double ratio = this.LoadingUnitProperty.Width / this.LoadingUnitProperty.Length;

            //PIXEL NEW
            this.TrayWidth = widthTrayPixel;//this.LoadingUnitProperty.Width;// * ratio;
            this.TrayHeight = ConvertMillimetersToPixel(this.LoadingUnitProperty.Length, this.TrayWidth, this.LoadingUnitProperty.Width);
            //RATIO PIXEL NEW
            double ratioW = this.TrayWidth / this.TrayHeight; //widthTrayPixel / this.LoadingUnitProperty.Width;
            double ratioH = this.TrayHeight / this.TrayWidth; //heightTrayPixel / this.LoadingUnitProperty.Length;

            //(this.LoadingUnitProperty.Length * this.TrayWidth) / widthTrayPixel;

            //this.LoadingUnitProperty.Width = ConvertMillimetersToPixel(this.LoadingUnitProperty.Width, widthTrayPixel, this.LoadingUnitProperty.Width);

            this.PenBrush = new SolidColorBrush(Colors.Black);
            this.PenThickness = 10;

            if (this.items != null)
            {
                foreach (var i in this.items)
                {
                    //i.Width = i.OriginWidth * ratioW;
                    //i.Height = i.OriginHeight * ratioH;
                    //i.Top = i.OriginTop * ratioH;
                    //i.Left = i.OriginLeft * ratioW;

                    i.Width = //(this.TrayHeight * i.OriginWidth) / this.TrayWidth;
                        ConvertMillimetersToPixel(i.OriginWidth, this.TrayWidth, this.LoadingUnitProperty.Width);
                    i.Height = //(this.TrayHeight * i.OriginHeight) / this.TrayWidth;
                        ConvertMillimetersToPixel(i.OriginHeight, this.TrayWidth, this.LoadingUnitProperty.Width);
                    i.Top = //(this.TrayHeight * i.OriginTop) / this.TrayWidth;
                        ConvertMillimetersToPixel(i.OriginTop, this.TrayWidth, this.LoadingUnitProperty.Width);
                    i.Left = //(this.TrayHeight * i.OriginLeft) / this.TrayWidth;
                        ConvertMillimetersToPixel(i.OriginLeft, this.TrayWidth, this.LoadingUnitProperty.Width);
                }
            }
        }

        public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        {
            this.items = new ObservableCollection<WmsBaseCompartment>();
            this.LoadingUnitProperty = loadingUnitDetails;

            ///

            this.TrayHeight = this.LoadingUnitProperty.Length;
            this.TrayWidth = this.LoadingUnitProperty.Width;

            this.Top = 0;
            this.Left = 0;

            this.PenBrush = new SolidColorBrush(Colors.Black);
            this.PenThickness = 10;
            ////

            loadingUnitDetails.AddedCompartmentEvent -= this.LoadingUnitDetails_AddedCompartmentEvent;
            loadingUnitDetails.AddedCompartmentEvent += this.LoadingUnitDetails_AddedCompartmentEvent;
            this.TransformDataInput();
            this.NotifyPropertyChanged(nameof(this.Items));
        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static double ConvertMillimetersToPixel(double value, double pixel, double mm, int offsetMM = 0)
        {
            if (mm > 0)
            {
                return (pixel * value) / mm + offsetMM;
            }
            return value;
        }

        private void CalculateDimensionPixelOfTray(double widthPixel, double heightPixel, double widthMm, double heightMm)
        {
        }

        private void LoadingUnitDetails_AddedCompartmentEvent(Object sender, EventArgs e)
        {
            this.TransformDataInput();
            this.NotifyPropertyChanged(nameof(this.Items));
        }

        private void TransformDataInput(float ratio = 1)
        {
            foreach (var compartment in this.LoadingUnitProperty.Compartments)
            {
                this.items.Add(new WmsCompartmentViewModel
                {
                    Tray = new Tray { WidthMm = this.LoadingUnitProperty.Width, HeightMm = this.LoadingUnitProperty.Length },
                    OriginHeight = (int)compartment.Height,
                    OriginWidth = (int)compartment.Width,
                    OriginLeft = (int)compartment.XPosition,
                    OriginTop = (int)compartment.YPosition,
                    Width = (int)(compartment.Width * ratio),
                    Height = (int)(compartment.Height * ratio),
                    Left = (int)(compartment.XPosition * ratio),
                    Top = (int)(compartment.YPosition * ratio),
                    ColorFill = Colors.Red.ToString(),
                    Selected = Colors.RoyalBlue.ToString()
                });
            }
        }

        #endregion Methods
    }
}
