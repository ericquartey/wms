using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.Common.BusinessModels;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class WmsBaseCompartment : BindableBase
    {
        #region Fields

        private string article;

        private int capacity;

        private string colorBorder;

        private string colorFill;

        private double height;

        private bool isSelected;

        private double left;

        private int quantity;

        private float rectangleBorderThickness;

        private string selected;

        private double top;

        private double width;

        #endregion Fields

        #region Properties

        public string Article
        {
            get { return this.article; }
            set { this.SetProperty(ref this.article, value); }
        }

        public int Capacity
        {
            get { return this.capacity; }
            set { this.SetProperty(ref this.capacity, value); }
        }

        public string ColorBorder
        {
            get { return this.colorBorder; }
            set { this.SetProperty(ref this.colorBorder, value); }
        }

        public string ColorFill
        {
            get { return this.colorFill; }
            set { this.SetProperty(ref this.colorFill, value); }
        }

        public CompartmentDetails CompartmentDetails { get; set; }

        public double Height
        {
            get { return this.height; }
            set { this.SetProperty(ref this.height, value); }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.SetProperty(ref this.isSelected, value); }
        }

        public double Left
        {
            get { return this.left; }
            set { this.SetProperty(ref this.left, value); }
        }

        public int Quantity
        {
            get { return this.quantity; }
            set { this.SetProperty(ref this.quantity, value); }
        }

        public float RectangleBorderThickness
        {
            get { return this.rectangleBorderThickness; }
            set { this.SetProperty(ref this.rectangleBorderThickness, value); }
        }

        public string Selected
        {
            get { return this.selected; }
            set { this.SetProperty(ref this.selected, value); }
        }

        public double Top
        {
            get { return this.top; }
            set { this.SetProperty(ref this.top, value); }
        }

        public Tray Tray
        {
            get;
            set;
        }

        public double Width
        {
            get { return this.width; }
            set { this.SetProperty(ref this.width, value); }
        }

        #endregion Properties
    }
}
