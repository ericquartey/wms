using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private ObservableCollection<WmsBaseCompartment> items;

        private int left;
        private SolidColorBrush penBrush;
        private int penThickness;
        private int top;
        private Dimension trayDimension;
        private Position trayOrigin;

        #endregion Fields

        //private Position position;

        //public Position position
        //{
        //    get { return position; }
        //    set { position = value; }
        //}
        //private Dimension dimension;

        //public Dimension Dimension
        //{
        //    get { return dimension; }
        //    set { dimension = value; }
        //}

        #region Constructors

        public WmsTrayControlViewModel()
        {
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

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

        public LoadingUnitDetails LoadingUnitProperty { get; set; }

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

        public int Top
        {
            get { return this.top; }
            set
            {
                this.top = value;
                this.NotifyPropertyChanged(nameof(this.Top));
            }
        }

        public Dimension TrayDimension
        {
            get => this.trayDimension;
            set
            {
                this.trayDimension = value;
                this.NotifyPropertyChanged(nameof(this.TrayDimension));
            }
        }

        public Position TrayOrigin
        {
            get => this.trayOrigin;
            set
            {
                this.trayOrigin = value;
                this.NotifyPropertyChanged(nameof(this.TrayOrigin));
            }
        }

        #endregion Properties

        #region Methods

        public void Resize(double widthTrayPixel, double heightTrayPixel)
        {
            if (this.items != null)
            {
                foreach (var i in this.items)
                {
                    i.Width = GraphicUtils.ConvertMillimetersToPixel((int)i.CompartmentDetails.Width, widthTrayPixel, this.LoadingUnitProperty.Width);
                    i.Height = GraphicUtils.ConvertMillimetersToPixel((int)i.CompartmentDetails.Height, widthTrayPixel, this.LoadingUnitProperty.Width);
                    Dimension compartmentDimension = new Dimension() { Width = (int)i.CompartmentDetails.Width, Height = (int)i.CompartmentDetails.Height };
                    var top = GraphicUtils.ConvertWithStandardOrigin((int)i.CompartmentDetails.YPosition, PositionType.Y, this.TrayOrigin, this.TrayDimension, compartmentDimension);
                    i.Top = GraphicUtils.ConvertMillimetersToPixel(top, widthTrayPixel, this.LoadingUnitProperty.Width);
                    var left = GraphicUtils.ConvertWithStandardOrigin((int)i.CompartmentDetails.XPosition, PositionType.X, this.TrayOrigin, this.TrayDimension, compartmentDimension);
                    i.Left = GraphicUtils.ConvertMillimetersToPixel(left, widthTrayPixel, this.LoadingUnitProperty.Width);
                }
            }
        }

        public void UpdateCompartments(IEnumerable<CompartmentDetails> compartments, float ratio = 1)
        {
            if (this.LoadingUnitProperty != null)
            {
                foreach (var compartment in compartments)
                {
                    this.items.Add(new WmsCompartmentViewModel
                    {
                        Tray = new Tray { WidthMm = this.LoadingUnitProperty.Width, HeightMm = this.LoadingUnitProperty.Length },
                        CompartmentDetails = compartment,

                        Width = (int)(compartment.Width * ratio),
                        Height = (int)(compartment.Height * ratio),
                        Left = (int)(compartment.XPosition * ratio),
                        Top = (int)(compartment.YPosition * ratio),
                        ColorFill = Colors.Aquamarine.ToString(),
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

        public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        {
            this.items = new ObservableCollection<WmsBaseCompartment>();
            this.LoadingUnitProperty = loadingUnitDetails;

            this.TrayDimension = new Dimension()
            {
                Width = this.LoadingUnitProperty.Width,
                Height = this.LoadingUnitProperty.Length
            };
            this.TrayOrigin = new Position()
            {
                XPosition = this.LoadingUnitProperty.OriginTray.XPosition,
                YPosition = this.LoadingUnitProperty.OriginTray.YPosition
            };
            this.Top = 0;
            this.Left = 0;

            //loadingUnitDetails.AddedCompartmentEvent -= this.LoadingUnitDetails_AddedCompartmentEvent;
            //loadingUnitDetails.AddedCompartmentEvent += this.LoadingUnitDetails_AddedCompartmentEvent;
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

        private void CompatmentSelected_UpdateCompartmentEvent(Object sender, EventArgs e)
        {
        }

        private void LoadingUnitDetails_AddedCompartmentEvent(Object sender, EventArgs e)
        {
            this.items = new ObservableCollection<WmsBaseCompartment>();
            this.TransformDataInput();
            this.NotifyPropertyChanged(nameof(this.Items));
        }

        private void TransformDataInput(float ratio = 1)
        {
            foreach (var compartment in this.LoadingUnitProperty.Compartments)
            {
                this.items.Add(new WmsCompartmentViewModel
                {
                    Tray = new Tray { WidthMm = this.LoadingUnitProperty.Width, HeightMm = this.LoadingUnitProperty.Length },
                    CompartmentDetails = compartment,

                    Width = (int)(compartment.Width * ratio),
                    Height = (int)(compartment.Height * ratio),
                    Left = (int)(compartment.XPosition * ratio),
                    Top = (int)(compartment.YPosition * ratio),
                    ColorFill = Colors.Aquamarine.ToString(),
                    Selected = Colors.RoyalBlue.ToString(),
                    RectangleBorderThickness = 1,
                    IsSelected = true
                });
            }
        }

        #endregion Methods
    }
}
