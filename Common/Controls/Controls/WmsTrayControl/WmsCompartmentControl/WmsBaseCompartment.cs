using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class WmsBaseCompartment : Prism.Mvvm.BindableBase
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
        private Thickness rectangleBorderThickness;

        private string selected;

        private double top;

        private double width;

        #endregion Fields

        #region Properties

        public string Article
        {
            get => this.article;
            set => this.SetProperty(ref this.article, value);
        }

        public int Capacity
        {
            get => this.capacity;
            set => this.SetProperty(ref this.capacity, value);
        }

        public string ColorBorder
        {
            get => this.colorBorder;
            set => this.SetProperty(ref this.colorBorder, value);
        }

        public string ColorFill
        {
            get => this.colorFill;
            set => this.SetProperty(ref this.colorFill, value);
        }

        public CompartmentDetails CompartmentDetails { get; set; }

        public double Height
        {
            get => this.height;
            set => this.SetProperty(ref this.height, value);
        }

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        public double Left
        {
            get => this.left;
            set => this.SetProperty(ref this.left, value);
        }

        public int Quantity
        {
            get => this.quantity;
            set => this.SetProperty(ref this.quantity, value);
        }

        public Thickness RectangleBorderThickness
        {
            get => this.rectangleBorderThickness;
            set => this.SetProperty(ref this.rectangleBorderThickness, value);
        }

        public string Selected
        {
            get => this.selected;
            set => this.SetProperty(ref this.selected, value);
        }

        public double Top
        {
            get => this.top;
            set => this.SetProperty(ref this.top, value);
        }

        public Tray Tray
        {
            get;
            set;
        }

        public double Width
        {
            get => this.width;
            set => this.SetProperty(ref this.width, value);
        }

        #endregion Properties
    }
}
