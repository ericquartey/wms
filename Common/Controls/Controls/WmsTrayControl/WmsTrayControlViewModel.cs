using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
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

        private readonly int BORDER_TRAY_HEIGHT = 1;
        private ObservableCollection<WmsBaseCompartment> items;
        private int left;
        private SolidColorBrush penBrush;
        private int penThickness;
        private Func<CompartmentDetails, CompartmentDetails, Color> selectedColorFilterFunc;
        private CompartmentDetails selectedCompartment;
        private int top;

        private Tray tray;

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
        public bool IsCompartmentSelectable { get; set; }
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

        public bool ReadOnly { get; set; }

        public Func<CompartmentDetails, CompartmentDetails, Color> SelectedColorFilterFunc
        {
            get { return this.selectedColorFilterFunc; }
            set
            {
                this.selectedColorFilterFunc = value;
                this.UpdateColorCompartments();
            }
        }

        public CompartmentDetails SelectedCompartment
        {
            get => this.selectedCompartment;
            set
            {
                this.selectedCompartment = value;
                this.UpdateColorCompartments();
            }
        }

        public bool ShowBackground { get; set; }
        public bool ShowRuler { get; set; }

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
                        X = 0,
                        Y = this.tray.Dimension.Height
                    };
                }
                this.NotifyPropertyChanged(nameof(this.Tray));
            }
        }

        #endregion Properties

        #region Methods

        public void ResizeCompartments(double widthTrayPixel, double heightTrayPixel)
        {
            if (this.items != null)
            {
                Debug.WriteLine($"CanvasComp: pixel->W={widthTrayPixel} H={heightTrayPixel}  reale->W={this.Tray.Dimension.Width} H={this.Tray.Dimension.Height}");
                var widthTray = widthTrayPixel - this.Tray.DOUBLE_BORDER_TRAY;
                foreach (var i in this.items)
                {
                    if (i.CompartmentDetails.Width != null && i.CompartmentDetails.Height != null && i.CompartmentDetails.XPosition != null && i.CompartmentDetails.YPosition != null)
                    {
                        i.Width = GraphicUtils.ConvertMillimetersToPixel((int)i.CompartmentDetails.Width, widthTrayPixel, this.Tray.Dimension.Width);
                        i.Height = GraphicUtils.ConvertMillimetersToPixel((int)i.CompartmentDetails.Height, widthTrayPixel, this.Tray.Dimension.Width);

                        Dimension compartmentDimension = new Dimension() { Width = (int)i.CompartmentDetails.Width, Height = (int)i.CompartmentDetails.Height };
                        Position compartmentOrigin = new Position { X = (int)i.CompartmentDetails.XPosition, Y = (int)i.CompartmentDetails.YPosition };

                        double originY = GraphicUtils.ConvertMillimetersToPixel(compartmentOrigin.Y, widthTrayPixel, this.Tray.Dimension.Width);
                        double originX = GraphicUtils.ConvertMillimetersToPixel(compartmentOrigin.X, widthTrayPixel, this.Tray.Dimension.Width);
                        Position compartmentOriginPixel = new Position { X = (int)Math.Floor(originX), Y = (int)Math.Floor(originY) };

                        Position convertedCompartmentOrigin = GraphicUtils.ConvertWithStandardOriginPixel(compartmentOriginPixel, this.Tray, widthTrayPixel, heightTrayPixel, i.Width, i.Height);
                        i.Top = convertedCompartmentOrigin.Y;
                        i.Left = convertedCompartmentOrigin.X;
                    }
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
                        ColorFill = Colors.Aquamarine.ToString(),
                        Selected = Colors.RoyalBlue.ToString(),
                        //IsSelected = true,
                        ReadOnly = this.ReadOnly,
                        IsSelectable = this.IsCompartmentSelectable
                    });
                }
            }
        }

        public void UpdateInputForm(CompartmentDetails compartment)
        {
            this.CompartmentDetailsProperty = compartment;
            this.NotifyPropertyChanged(nameof(this.CompartmentDetailsProperty));
        }

        public void UpdateIsSelectablePropertyToCompartments(bool value)
        {
            if (this.items != null)
            {
                foreach (var item in this.Items)
                {
                    item.IsSelectable = value;
                }
            }
        }

        public void UpdateReadOnlyPropertyToCompartments(bool value)
        {
            if (this.items != null)
            {
                foreach (var item in this.Items)
                {
                    item.ReadOnly = value;
                }
            }
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
                int width = 0, height = 0, x = 0, y = 0;
                if (compartment.Width != null)
                {
                    width = (int)compartment.Width;
                }
                if (compartment.Height != null)
                {
                    height = (int)compartment.Height;
                }
                if (compartment.XPosition != null)
                {
                    x = (int)compartment.XPosition;
                }
                if (compartment.YPosition != null)
                {
                    y = (int)compartment.YPosition;
                }

                this.items.Add(new WmsCompartmentViewModel
                {
                    Tray = this.Tray,
                    CompartmentDetails = compartment,

                    Width = (int)(width * ratio),
                    Height = (int)(height * ratio),
                    Left = (int)(x * ratio),
                    Top = (int)(y * ratio),
                    ColorFill = Colors.Aquamarine.ToString(),
                    Selected = Colors.RoyalBlue.ToString(),
                    //IsSelected = true,
                    ReadOnly = this.ReadOnly,
                    IsSelectable = this.IsCompartmentSelectable
                });
            }
        }

        private void UpdateColorCompartments()
        {
            if (this.items != null && this.selectedColorFilterFunc != null)
            {
                foreach (var item in this.Items)
                {
                    item.ColorFill = this.selectedColorFilterFunc.Invoke(item.CompartmentDetails, this.SelectedCompartment).ToString();
                }
            }
        }

        #endregion Methods
    }
}
