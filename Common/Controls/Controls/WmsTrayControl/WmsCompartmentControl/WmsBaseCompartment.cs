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
        #region Fields

        private double heightMM;
        private double widthMM;

        #endregion Fields

        #region Properties

        public double HeightMM
        {
            get { return this.heightMM; }
            set { this.heightMM = value; }
        }

        public double WidthMM
        {
            get { return this.widthMM; }
            set { this.widthMM = value; }
        }

        #endregion Properties
    }

    public class WmsBaseCompartment : INotifyPropertyChanged
    {
        #region Fields

        private float _borderThickness;
        private string _colorBorder;
        private string _colorFill;
        private double _height;
        private double _left;
        private double _top;
        private double _width;
        private string capacity;
        private double originHeight;
        private double originWidth;

        private string select;
        private Tray tray;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public float BorderThickness
        {
            get { return this._borderThickness; }
            set
            {
                this._borderThickness = value;
                this.OnPropertyChanged(nameof(this.BorderThickness));
            }
        }

        public string Capacity
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
            get { return this._colorBorder; }
            set
            {
                this._colorBorder = value;
                this.OnPropertyChanged(nameof(this.ColorBorder));
            }
        }

        public string ColorFill
        {
            get { return this._colorFill; }
            set
            {
                this._colorFill = value;
                this.OnPropertyChanged(nameof(this.ColorFill));
            }
        }

        public double Height
        {
            get
            {
                return this._height;
            }
            set
            {
                this._height = value;
                this.OnPropertyChanged(nameof(this.Height));
            }
        }

        public double Left
        {
            get { return this._left; }
            set
            {
                this._left = value;
                this.OnPropertyChanged(nameof(this.Left));
            }
        }

        public double OriginHeight
        {
            get { return this.originHeight; }
            set { this.originHeight = value; }
        }

        public double OriginWidth
        {
            get { return this.originWidth; }
            set { this.originWidth = value; }
        }

        public string Select
        {
            get { return this.select; }
            set { this.select = value; this.OnPropertyChanged(nameof(this.Select)); }
        }

        public double Top
        {
            get
            {
                return this._top;
            }
            set
            {
                this._top = value;
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
            get { return this._width; }
            set
            {
                this._width = value;
                this.OnPropertyChanged(nameof(this.Width));
            }
        }

        #endregion Properties

        #region Methods

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion Methods
    }
}
