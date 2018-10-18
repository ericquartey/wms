using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.Common.BusinessModels;

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

        private string article;

        private int capacity;

        private string colorBorder;

        private string colorFill;

        private double height;

        private bool isSelect;

        private bool isSelected;

        private double left;

        private int quantity;

        private float rectangleBorderThickness;

        private string selected;

        private double top;

        private double width;

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

        public CompartmentDetails CompartmentDetails { get; set; }

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

        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.isSelected = value; this.OnPropertyChanged(nameof(this.IsSelected)); }
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

        public int Quantity
        {
            get { return this.quantity; }
            set { this.quantity = value; }
        }

        //public double OriginWidth { get; set; }
        public float RectangleBorderThickness
        {
            get { return this.rectangleBorderThickness; }
            set
            {
                this.rectangleBorderThickness = value;
                this.OnPropertyChanged(nameof(this.RectangleBorderThickness));
            }
        }

        //public double OriginTop { get; set; }
        public string Selected
        {
            get { return this.selected; }
            set
            {
                this.selected = value;
                this.OnPropertyChanged(nameof(this.Selected));
            }
        }

        //public double OriginLeft { get; set; }
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

        //public double OriginHeight { get; set; }
        public Tray Tray
        {
            get;
            set;
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
