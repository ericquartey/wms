using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CompartmentRectangle : Control
    {
        double originX;
        double originY;

        public System.Double OriginX { get => this.originX; set => this.originX = value; }
        public System.Double OriginY { get => this.originY; set => this.originY = value; }

        static CompartmentRectangle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CompartmentRectangle), new FrameworkPropertyMetadata(typeof(CompartmentRectangle)));
        }
        public CompartmentRectangle()
        {            
            MouseDown += this.FocusTestControl_MouseDown;
        }

        

        void FocusTestControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
            this.Focus();
        }
    }
}
