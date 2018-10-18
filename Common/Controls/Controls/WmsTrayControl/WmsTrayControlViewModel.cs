using System;
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
        private double trayHeight;
        private double trayWidth;

        #endregion Fields

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

        public double TrayHeight
        {
            get => this.trayHeight;
            set
            {
                this.trayHeight = value;
                this.NotifyPropertyChanged(nameof(this.TrayHeight));
            }
        }

        public double TrayWidth
        {
            get => this.trayWidth;
            set
            {
                this.trayWidth = value;
                this.NotifyPropertyChanged(nameof(this.TrayWidth));
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
                    i.Top = GraphicUtils.ConvertMillimetersToPixel((int)i.CompartmentDetails.YPosition, widthTrayPixel, this.LoadingUnitProperty.Width);
                    i.Left = GraphicUtils.ConvertMillimetersToPixel((int)i.CompartmentDetails.XPosition, widthTrayPixel, this.LoadingUnitProperty.Width);
                }
            }
        }

        public void UpdateInputForm(CompartmentDetails compartment)
        {
            //var view = LayoutTreeHelper.GetVisualParents(this).OfType<WmsView>().FirstOrDefault();
            this.CompartmentDetailsProperty = compartment;
            this.NotifyPropertyChanged(nameof(this.CompartmentDetailsProperty));

            //compartment.UpdateCompartmentEvent -= this.CompatmentSelected_UpdateCompartmentEvent;

            //compartment.UpdateCompartmentEvent += this.CompatmentSelected_UpdateCompartmentEvent;
            //this.CompartmentDetailsProperty.OnUpdateCompartmentEvent(null);
        }

        public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        {
            this.items = new ObservableCollection<WmsBaseCompartment>();
            this.LoadingUnitProperty = loadingUnitDetails;

            this.TrayHeight = this.LoadingUnitProperty.Length;
            this.TrayWidth = this.LoadingUnitProperty.Width;
            this.Top = 0;
            this.Left = 0;

            loadingUnitDetails.AddedCompartmentEvent -= this.LoadingUnitDetails_AddedCompartmentEvent;
            loadingUnitDetails.AddedCompartmentEvent += this.LoadingUnitDetails_AddedCompartmentEvent;
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
            //
            //throw new NotImplementedException();
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
                    IsSelect = true
                });
            }
        }

        #endregion Methods
    }
}
