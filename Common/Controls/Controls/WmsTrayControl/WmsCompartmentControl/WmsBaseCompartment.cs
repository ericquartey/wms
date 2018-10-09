using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.Controls
{
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
        private string select;

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
