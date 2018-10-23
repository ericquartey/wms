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

        //private Dimension trayDimension;
        //private Position trayOrigin;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public WmsTrayControlViewModel()
        {
        }

        #endregion Constructors

        #region Events

        //public Dimension Dimension
        //{
        //    get { return dimension; }
        //    set { dimension = value; }
        //}
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        //public Position position
        //{
        //    get { return position; }
        //    set { position = value; }
        //}
        //private Dimension dimension;
        public CompartmentDetails CompartmentDetailsProperty { get; set; }

        //private Position position;
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

        //public LoadingUnitDetails LoadingUnitProperty { get; set; }
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

        //public Dimension TrayDimension
        //{
        //    get => this.trayDimension;
        //    set
        //    {
        //        this.trayDimension = value;
        //        this.NotifyPropertyChanged(nameof(this.TrayDimension));
        //    }
        //}

        //public Position TrayOrigin
        //{
        //    get => this.trayOrigin;
        //    set
        //    {
        //        this.trayOrigin = value;
        //        this.NotifyPropertyChanged(nameof(this.TrayOrigin));
        //    }
        //}

        #region Methods

        public void Resize(double widthTrayPixel, double heightTrayPixel)
        {
            if (this.items != null)
            {
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
                        //Tray = new Tray { WidthMm = this.Tray.Dimension.Width, HeightMm = this.LoadingUnitProperty.Length },
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

        public void UpdateTray(Tray tray)
        {
            this.Tray = tray;
            this.items = new ObservableCollection<WmsBaseCompartment>();

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
            foreach (var compartment in this.Tray.Compartments)
            {
                this.items.Add(new WmsCompartmentViewModel
                {
                    Tray = this.Tray,//new Tray { WidthMm = this.LoadingUnitProperty.Width, HeightMm = this.LoadingUnitProperty.Length },
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
