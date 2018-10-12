using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    public class WmsTrayControlViewModel : INotifyPropertyChanged
    {
        #region Fields

        private int heightTray;
        private ObservableCollection<WmsBaseCompartment> items;

        private SolidColorBrush penBrush;

        private int penThickness;

        private int widthTray;

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

        public int HeightTray
        {
            get { return this.heightTray; }
            set
            {
                this.heightTray = value;
                this.NotifyPropertyChanged(nameof(this.HeightTray));
            }
        }

        public ObservableCollection<WmsBaseCompartment> Items { get { return this.items; } set { this.items = value; } }

        public LoadingUnitDetails LoadingUnitProperty { get; set; }

        public SolidColorBrush PenBrush
        {
            get { return this.penBrush; }
            set
            {
                this.penBrush = value;
                this.NotifyPropertyChanged(nameof(this.PenBrush));
            }
        }

        public int PenThickness
        {
            get { return this.penThickness; }
            set
            {
                this.penThickness = value;
                this.NotifyPropertyChanged(nameof(this.PenThickness));
            }
        }

        public int WidthTray
        {
            get { return this.widthTray; }
            set
            {
                this.widthTray = value;
                this.NotifyPropertyChanged(nameof(this.WidthTray));
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
                    i.Width = ConvertMillimetersToPixel(i.OriginWidth, widthTrayPixel, i.Tray.WidthMm);
                    i.Height = ConvertMillimetersToPixel(i.OriginHeight, heightTrayPixel, i.Tray.HeightMm);
                    i.Top = ConvertMillimetersToPixel(i.Top, heightTrayPixel, i.Tray.HeightMm);
                    i.Left = ConvertMillimetersToPixel(i.Left, widthTrayPixel, i.Tray.WidthMm);
                }
            }
        }

        public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        {
            this.items = new ObservableCollection<WmsBaseCompartment>();
            this.LoadingUnitProperty = loadingUnitDetails;

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

        private static double ConvertMillimetersToPixel(double value, double pixel, double mm, int offsetMM = 0)
        {
            if (mm > 0)
            {
                return (pixel * value) / mm + offsetMM;
            }
            return value;
        }

        private void LoadingUnitDetails_AddedCompartmentEvent(Object sender, EventArgs e)
        {
            this.TransformDataInput();
            this.NotifyPropertyChanged(nameof(this.Items));
        }

        private void TransformDataInput(float ratio = 1)
        {
            var compartments = this.LoadingUnitProperty.Compartments;
            foreach (var compartment in compartments)
            {
                this.items.Add(new WmsCompartmentViewModel
                {
                    Tray = new Tray { WidthMm = this.LoadingUnitProperty.Width, HeightMm = this.LoadingUnitProperty.Length },
                    OriginHeight = (int)compartment.Height,
                    OriginWidth = (int)compartment.Width,
                    Width = (int)(compartment.Width * ratio),
                    Height = (int)(compartment.Height * ratio),
                    Left = (int)(compartment.XPosition * ratio),
                    Top = (int)(compartment.YPosition * ratio),
                    ColorFill = Colors.Red.ToString(),
                    Select = Colors.RoyalBlue.ToString()
                });
            }
        }

        #endregion Methods
    }
}
