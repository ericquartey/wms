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
using System.Linq;

namespace Ferretto.Common.Controls
{
    public class WmsTrayControlViewModel : INotifyPropertyChanged
    {
        #region Fields

        private static readonly Func<IFilter, Color> DefaultColorCompartment = (x) => Colors.Yellow;

        private readonly int BORDER_TRAY_HEIGHT = 1;
        private bool isCompartmentSelectable;
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

        public bool IsCompartmentSelectable
        {
            get => this.isCompartmentSelectable;
            set
            {
                this.isCompartmentSelectable = value;
                this.UpdateIsSelectablePropertyToCompartments();
                this.NotifyPropertyChanged(nameof(this.IsCompartmentSelectable));
            }
        }

        public ObservableCollection<WmsBaseCompartment> Items
        {
            get => this.items;
            set => this.items = value;
        }

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
                this.UpdateTray();
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

        public void UpdateCompartments(IEnumerable<CompartmentDetails> compartments)//, float ratio = 1)
        {
            if (this.Tray != null)
            {
                foreach (var compartment in compartments)
                {
                    compartment.PropertyChanged += this.Compartment_PropertyChanged;
                    this.items.Add(new WmsCompartmentViewModel
                    {
                        Tray = this.Tray,

                        CompartmentDetails = compartment,
                        Width = compartment.Width ?? 0,
                        Height = compartment.Height ?? 0,
                        Left = compartment.XPosition ?? 0,
                        Top = compartment.YPosition ?? 0,
                        ColorFill = Colors.Aquamarine.ToString(),
                        Selected = Colors.RoyalBlue.ToString(),
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

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Compartment_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (sender is CompartmentDetails compartmentDetails &&
                (
                e.PropertyName == nameof(CompartmentDetails.Width) ||
                e.PropertyName == nameof(CompartmentDetails.Height) ||
                e.PropertyName == nameof(CompartmentDetails.XPosition) ||
                e.PropertyName == nameof(CompartmentDetails.YPosition)
                ))
            {
                var item = this.items.Single(i => i.CompartmentDetails == compartmentDetails);

                item.Width = compartmentDetails.Width ?? 0;
                item.Height = compartmentDetails.Height ?? 0;
                item.Left = compartmentDetails.XPosition ?? 0;
                item.Top = compartmentDetails.YPosition ?? 0;

                this.NotifyPropertyChanged(nameof(this.Items));
            }
        }

        private void LoadingUnitDetails_AddedCompartmentEvent(Object sender, EventArgs e)
        {
            this.UpdateTray();
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

        private void UpdateIsSelectablePropertyToCompartments()
        {
            if (this.items != null)
            {
                foreach (var item in this.Items)
                {
                    item.IsSelectable = this.isCompartmentSelectable;
                }
            }
        }

        private void UpdateTray()
        {
            this.items = new ObservableCollection<WmsBaseCompartment>();
            this.UpdateCompartments(this.Tray.Compartments);
            this.NotifyPropertyChanged(nameof(this.Items));
        }

        #endregion Methods
    }
}
