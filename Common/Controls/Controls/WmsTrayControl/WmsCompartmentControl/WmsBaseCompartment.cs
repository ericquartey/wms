using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.Controls
{
    public class Tray
    {
        #region Properties

        public double HeightMm { get; set; }

        public double WidthMm { get; set; }

        #endregion Properties
    }

    public class WmsBaseCompartment : INotifyPropertyChanged
    {
        #region Fields
        private int quantity;
        private int capacity;
        private string colorBorder;
        private string colorFill;
        private double height;
        private double left;
        private float rectangleBorderThickness;
        private string select;
        private double top;
        private Tray tray;
        private double width;
        private string article;


        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties
        public string Article
        {
            get { return this.article; }
            set
            {
                this.article = value;
                this.OnPropertyChanged(nameof(this.Article));
            }
        }
        public int Capacity
        {
            get { return this.capacity; }
            set
            {
                this.capacity = value;
                this.OnPropertyChanged(nameof(this.Capacity));
            }
        }

        public string ColorBorder
        {
            get { return this.colorBorder; }
            set
            {
                this.colorBorder = value;
                this.OnPropertyChanged(nameof(this.ColorBorder));
            }
        }

        public string ColorFill
        {
            get { return this.colorFill; }
            set
            {
                this.colorFill = value;
                this.OnPropertyChanged(nameof(this.ColorFill));
            }
        }

        public double Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
                this.OnPropertyChanged(nameof(this.Height));
            }
        }

        public double Left
        {
            get { return this.left; }
            set
            {
                this.left = value;
                this.OnPropertyChanged(nameof(this.Left));
            }
        }

        public double OriginHeight { get; set; }

        public double OriginWidth { get; set; }

        public float RectangleBorderThickness
        {
            get { return this.rectangleBorderThickness; }
            set
            {
                this.rectangleBorderThickness = value;
                this.OnPropertyChanged(nameof(this.RectangleBorderThickness));
            }
        }

        public int Quantity
        {
            get { return this.quantity; }
            set { this.quantity = value; }
        }

        public string Select
        {
            get { return this.select; }
            set
            {
                this.select = value;
                this.OnPropertyChanged(nameof(this.Select));
            }
        }

        public double Top
        {
            get
            {
                return this.top;
            }
            set
            {
                this.top = value;
                this.OnPropertyChanged(nameof(this.Top));
            }
        }

        public Tray Tray
        {
            get
            {
                return this.tray;
            }
            set
            {
                this.tray = value;
            }
        }

        public double Width
        {
            get { return this.width; }
            set
            {
                this.width = value;
                this.OnPropertyChanged(nameof(this.Width));
            }
        }

        #endregion Properties

        #region Methods

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion Methods
    }
}
