using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.Controls
{
    internal class WmsCompartment
    {
        #region Fields

        private float _borderThickness;
        private Color _colorBorder;
        private string _colorFill;
        private double _height;
        private double _positionX;
        private double _positionY;
        private double _width;

        #endregion Fields

        #region Constructors

        public WmsCompartment(double Width, double Height, double PositionX, double PositionY, string ColorFill)
        {
            this.Width = Width;
            this.Height = Height;
            this.PositionX = PositionX;
            this.PositionY = PositionY;
            this.ColorFill = ColorFill;
        }

        #endregion Constructors

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
                this.OnPropertyChanged("BorderThickness");
            }
        }

        public Color ColorBorder
        {
            get { return this._colorBorder; }
            set
            {
                this._colorBorder = value;
                this.OnPropertyChanged("ColorBorder");
            }
        }

        public string ColorFill
        {
            get { return this._colorFill; }
            set
            {
                this._colorFill = value;
                this.OnPropertyChanged("ColorFill");
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
                this.OnPropertyChanged("Height");
            }
        }

        public double PositionX
        {
            get
            {
                return this._positionX;
            }
            set
            {
                this._positionX = value;
                this.OnPropertyChanged("PositionX");
            }
        }

        public double PositionY
        {
            get { return this._positionY; }
            set
            {
                this._positionY = value;
                this.OnPropertyChanged("PositionY");
            }
        }

        public double Width
        {
            get { return this._width; }
            set
            {
                this._width = value;
                this.OnPropertyChanged("Width");
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
