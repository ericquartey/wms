using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.VerticalWarehousesApp.Models
{
    public class InputCompartment
    {
        //public InputCompartment() { }
        //public InputCompartment(double Width, double Height, double PositionX, double PositionY)
        //{
        //    this.Width = Width;
        //    this.Height = Height;
        //    this.PositionX = PositionX;
        //    this.PositionY = PositionY;
        //}
        public double Width { get; set; }
        public double Height { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
    }
}
