using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CompartmentRectangle : Control
    {
        #region Fields

        /// <summary>
        /// Correction due to canvas' parent position
        /// </summary>
        private const int HEIGHT_CORRECTION = 50;

        private double originX;
        private double originY;

        #endregion Fields

        #region Constructors

        static CompartmentRectangle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CompartmentRectangle), new FrameworkPropertyMetadata(typeof(CompartmentRectangle)));
        }

        public CompartmentRectangle()
        {
            MouseDown += this.FocusTestControl_MouseDown;
        }

        #endregion Constructors

        #region Properties

        public System.Double OriginX { get => this.originX; set => this.originX = value; }

        public System.Double OriginY { get => this.originY; set => this.originY = value - HEIGHT_CORRECTION; }

        #endregion Properties

        #region Methods

        public bool Contains(Point p)
        {
            return ((p.X > this.originX) && (p.X - this.Width < this.originX) &&
                       (p.Y > this.OriginY) && (p.Y - this.Height < this.OriginY));
        }

        public bool ContainsOrOnFrontier(Point p)
        {
            return ((p.X >= this.originX) && (p.X - this.Width <= this.originX) &&
                       (p.Y >= this.OriginY) && (p.Y - this.Height <= this.OriginY));
        }

        private void FocusTestControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
            this.Focus();
        }

        #endregion Methods
    }
}
