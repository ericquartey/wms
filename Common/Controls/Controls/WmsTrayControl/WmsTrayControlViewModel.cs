using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class WmsTrayControlViewModel : INotifyPropertyChanged
    {
        #region Fields

        private static readonly Func<IFilter, Color> DefaultColorCompartment = (x) => Colors.Yellow;

        //private Func<CompartmentDetails, Enumeration, Color> coloringFuncCompartment = DefaultColorCompartment;
        //private Func<IFilter, Color> coloringFuncCompartment = delegate (IFilter selectedFilter)
        private Func<Color> coloringFuncCompartment = delegate ()
        {
            //= x => Colors.Green;
            Color color = Colors.Gray;
            //if (selectedFilter != null)
            //{
            var idFilter = 1;//selectedFilter.Id;
            switch (idFilter)
            {
                case 1:
                    color = Colors.Violet;
                    break;

                case 2:
                    color = Colors.Orange;
                    break;

                case 3:
                    color = Colors.Green;
                    break;

                case 4:
                    color = Colors.Blue;
                    break;

                default:
                    color = Colors.Black;
                    break;
            }
            //}
            return color;
        };

        private ObservableCollection<WmsBaseCompartment> items;
        private int left;
        private SolidColorBrush penBrush;
        private int penThickness;
        private int top;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public WmsTrayControlViewModel()
        {
        }

        #endregion Constructors

        #region Events

        // { R = 0, G = 0, B = 100 };
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        //public Func<IFilter, Color> ColoringFuncCompartment
        public Func<Color> ColoringFuncCompartment
        {
            get { return this.coloringFuncCompartment; }
            set
            {
                if (value == null)
                {
                    //this.coloringFuncCompartment = DefaultColorCompartment;
                }
                else
                {
                    this.coloringFuncCompartment = value;
                    var color = this.coloringFuncCompartment();
                    //Color color = this.coloringFuncCompartment.Invoke();
                    this.UpdateColorCompartments(color);
                    //this.ColoringFuncCompartment(this.SelectedFilter);
                }
                //this.NotifyPropertyChanged(nameof(this.ColoringFuncCompartment));
            }
        }

        public CompartmentDetails CompartmentDetailsProperty { get; set; }
        public ObservableCollection<WmsBaseCompartment> Items { get => this.items; set => this.items = value; }

        public int Left
        {
            get { return this.left; }
            set
            {
                this.left = value;
                this.NotifyPropertyChanged(nameof(this.Left));
            }
        }

        public SolidColorBrush PenBrush
        {
            get => this.penBrush;
            set
            {
                this.penBrush = value;
                this.NotifyPropertyChanged(nameof(this.PenBrush));
            }
        }

        public int PenThickness
        {
            get => this.penThickness;
            set
            {
                this.penThickness = value;
                this.NotifyPropertyChanged(nameof(this.PenThickness));
            }
        }

        public Enumeration SelectedFilter { get; set; }

        public int Top
        {
            get { return this.top; }
            set
            {
                this.top = value;
                this.NotifyPropertyChanged(nameof(this.Top));
            }
        }

        /// <summary>
        /// Property Tray
        /// If Origin is not explicit initialized, Set default : Botton - Left
        /// </summary>
        public Tray Tray
        {
            get { return this.tray; }
            set
            {
                this.tray = value;
                if (this.tray.Origin == null)
                {
                    this.tray.Origin = new Position
                    {
                        XPosition = 0,
                        YPosition = this.tray.Dimension.Height
                    };
                }
                this.NotifyPropertyChanged(nameof(this.Tray));
            }
        }

        #endregion Properties

        #region Methods

        public void Resize(double widthTrayPixel, double heightTrayPixel)
        {
            if (this.items != null)
            {
                Debug.WriteLine($"DRAW-COMPARTMENT: TRAY: W_PIXEL={widthTrayPixel} W={this.Tray.Dimension.Width}");
                foreach (var i in this.items)
                {
                    i.Width = GraphicUtils.ConvertMillimetersToPixel((int)i.CompartmentDetails.Width, widthTrayPixel, this.Tray.Dimension.Width);
                    i.Height = GraphicUtils.ConvertMillimetersToPixel((int)i.CompartmentDetails.Height, widthTrayPixel, this.Tray.Dimension.Width);
                    Dimension compartmentDimension = new Dimension() { Width = (int)i.CompartmentDetails.Width, Height = (int)i.CompartmentDetails.Height };
                    Position compartmentOrigin = new Position { XPosition = (int)i.CompartmentDetails.XPosition, YPosition = (int)i.CompartmentDetails.YPosition };

                    Position convertedCompartmentOrigin = GraphicUtils.ConvertWithStandardOrigin(compartmentOrigin, this.Tray.Origin, this.Tray.Dimension, compartmentDimension);
                    i.Top = GraphicUtils.ConvertMillimetersToPixel(convertedCompartmentOrigin.YPosition, widthTrayPixel, this.Tray.Dimension.Width);
                    i.Left = GraphicUtils.ConvertMillimetersToPixel(convertedCompartmentOrigin.XPosition, widthTrayPixel, this.Tray.Dimension.Width);
                }
            }
        }

        public void UpdateCompartments(IEnumerable<CompartmentDetails> compartments, float ratio = 1)
        {
            if (this.Tray != null)
            {
                foreach (var compartment in compartments)
                {
                    this.items.Add(new WmsCompartmentViewModel
                    {
                        Tray = this.Tray,
                        CompartmentDetails = compartment,

                        Width = (int)(compartment.Width * ratio),
                        Height = (int)(compartment.Height * ratio),
                        Left = (int)(compartment.XPosition * ratio),
                        Top = (int)(compartment.YPosition * ratio),
                        ColorFill = //this.ColoringFuncCompartment(this.SelectedFilter).ToString(),
                        Colors.Aquamarine.ToString(),
                        Selected = Colors.RoyalBlue.ToString(),
                        IsSelected = true
                    });
                }
            }
        }

        public void UpdateInputForm(CompartmentDetails compartment)
        {
            this.CompartmentDetailsProperty = compartment;
            this.NotifyPropertyChanged(nameof(this.CompartmentDetailsProperty));
        }

        public void UpdateTray(Tray tray)
        {
            this.Tray = tray;
            this.items = new ObservableCollection<WmsBaseCompartment>();

            this.TransformDataInput();
            this.NotifyPropertyChanged(nameof(this.Items));
        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void LoadingUnitDetails_AddedCompartmentEvent(Object sender, EventArgs e)
        {
            this.items = new ObservableCollection<WmsBaseCompartment>();
            this.TransformDataInput();
            this.NotifyPropertyChanged(nameof(this.Items));
        }

        private void TransformDataInput(float ratio = 1)
        {
            foreach (var compartment in this.Tray.Compartments)
            {
                this.items.Add(new WmsCompartmentViewModel
                {
                    Tray = this.Tray,
                    CompartmentDetails = compartment,

                    Width = (int)(compartment.Width * ratio),
                    Height = (int)(compartment.Height * ratio),
                    Left = (int)(compartment.XPosition * ratio),
                    Top = (int)(compartment.YPosition * ratio),
                    ColorFill =
                    //this.ColoringFuncCompartment(this.SelectedFilter).ToString(),
                        Colors.Aquamarine.ToString(),
                    Selected = Colors.RoyalBlue.ToString(),
                    RectangleBorderThickness = new Thickness(1),
                    IsSelected = true
                });
            }
        }

        private void UpdateColorCompartments(Color color)
        {
            if (this.items != null)
            {
                foreach (var item in this.items)
                {
                    item.ColorFill = color.ToString();//this.ColoringFuncCompartment(this.SelectedFilter).ToString();
                    item.RectangleBorderThickness = new Thickness(5);
                    item.Selected = Colors.Violet.ToString();
                }
                this.NotifyPropertyChanged(nameof(this.Items));
            }
        }

        #endregion Methods
    }
}
