using System;
using System.Collections.Generic;
using System.Linq;
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
using Ferretto.VW.VerticalWarehousesApp.Models;

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for InputCompartmentControl.xaml
    /// </summary>
    
    public partial class InputCompartmentControl : UserControl
    {
       

        public event EventHandler ButtonClick;


        public InputCompartment InputCompartment;
        public InputCompartmentControl()
        {
            this.InitializeComponent();
        }


        private void Width_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void Height_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void PositionX_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void PositionY_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void CreateCompartmentBtn_Click(Object sender, RoutedEventArgs e)
        {
            Console.WriteLine("InputCompartment::CreateCompartmentBtn_Click");
            var Width = this.WidthText.Text;
            var Height = this.HeightText.Text;
            var PositionX = this.PositionXText.Text;
            var PositionY = this.PositionYText.Text;
            this.InputCompartment = new InputCompartment()
            {
                PositionX = Double.Parse(PositionX),
                PositionY = Double.Parse(PositionY),
                Width = Double.Parse(Width),
                Height = Double.Parse(Height)
            };
            //this.btnevents.ClickButton(sender, e);
            Button b = (Button)e.Source as Button;

            if (this.ButtonClick != null)
            {
                this.ButtonClick(this, e);
            }
        }

        public double GetWidth()
        {
            if(this.WidthText.Text != null)
            {
                try
                {
                    return Double.Parse(this.WidthText.Text.Trim());
                }
                catch (Exception) { return 0; }
            }
            else
            {
                return 0;
            }
        }
        public double GetHeight()
        {
            if (this.HeightText.Text != null)
            {
                try
                {
                    return Double.Parse(this.HeightText.Text.Trim());
                }
                catch (Exception) { return 0; }
            }
            else
            {
                return 0;
            }
        }
        public double GetPositionX()
        {
            if (this.PositionXText.Text != null)
            {
                try
                {
                    return Double.Parse(this.PositionXText.Text.Trim());
                }
                catch (Exception) { return 0; }
            }
            else
            {
                return 0;
            }
        }
        public double GetPositionY()
        {
            if (this.PositionYText.Text != null)
            {
                try
                {
                    return Double.Parse(this.PositionYText.Text.Trim());
                }
                catch (Exception) { return 0; }
            }
            else
            {
                return 0;
            }
        }
    }
}
