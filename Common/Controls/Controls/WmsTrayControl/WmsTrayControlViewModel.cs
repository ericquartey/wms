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
        private double heightTrayPixel;
        private bool isCompartmentSelectable;
        private ObservableCollection<WmsBaseCompartment> items;
        private int left;
        private SolidColorBrush penBrush;
        private int penThickness;
        private Func<CompartmentDetails, CompartmentDetails, Color> selectedColorFilterFunc;
        private CompartmentDetails selectedCompartment;
        private int top;
        private Tray tray;
        private double widthTrayPixel;

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
            set
            {
                this.items = value;
                this.NotifyPropertyChanged(nameof(this.Items));
            }
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
                if (this.tray != null)
                {
                    this.tray.Compartments.ListChanged -= this.Compartments_ListChanged;
                }

                this.tray = value;
                this.tray.Compartments.ListChanged += this.Compartments_ListChanged;

                if (this.tray.Origin == null)
                {
                    this.tray.Origin = new Position
                    {
                        X = 0,
                        Y = this.tray.Dimension.Height
                    };
                }
                this.UpdateCompartments(this.Tray.Compartments);
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

                foreach (var compartment in this.items)
                {
                    this.ResizeCompartment(widthTrayPixel, heightTrayPixel, compartment);
                }
            }
        }

        public void UpdateCompartments(IEnumerable<CompartmentDetails> compartments)
        {
            if (this.Tray != null)
            {
                var newItems = new ObservableCollection<WmsBaseCompartment>();
                foreach (var compartment in compartments)
                {
                    //compartment.PropertyChanged += this.Compartment_PropertyChanged;
                    newItems.Add(new WmsCompartmentViewModel
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

                this.Items = newItems;
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

        private WmsCompartmentViewModel AddCompartment(CompartmentDetails compartment)
        {
            if (this.Tray != null)
            {
                return new WmsCompartmentViewModel
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
                };
            }
            return null;
        }

        private void Compartments_ListChanged(Object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                this.items.RemoveAt(e.NewIndex);
            }
            else if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (sender is IList<CompartmentDetails> compartments)
                {
                    Debug.WriteLine($"Compartments: {compartments.Count}");
                    var addedCompartment = compartments[compartments.Count - 1];
                    var compartmentGraphic = this.AddCompartment(addedCompartment);

                    this.ResizeCompartment(this.widthTrayPixel, this.heightTrayPixel, compartmentGraphic);

                    this.items.Add(compartmentGraphic);
                }
            }

            this.NotifyPropertyChanged(nameof(this.Items));
        }

        //private void GenerateBulkCompartments(Position start, Dimension size, int row, int column, CompartmentDetails detail)
        //{
        //    var tempList = new List<CompartmentDetails>();
        //    int startX = start.X;
        //    int startY = start.Y;
        //    for (int i = 0; i < row; i++)
        //    {
        //        for (int j = 0; j < column; j++)
        //        {
        //            tempList.Add(new CompartmentDetails()
        //            {
        //                Width = size.Width,
        //                Height = size.Height,
        //                XPosition = startX * i,
        //                YPosition = startY * j,
        //                ItemPairing = detail.ItemPairing,
        //                ItemCode = detail.ItemCode,
        //                Stock = detail.Stock,
        //                MaxCapacity = detail.MaxCapacity
        //            });
        //        }
        //    }

        //    this.tray.AddCompartmentsRange(tempList);
        //}

        private void LoadingUnitDetails_AddedCompartmentEvent(Object sender, EventArgs e)
        {
            this.UpdateCompartments(this.Tray.Compartments);
        }

        private void ResizeCompartment(double widthTrayPixel, double heightTrayPixel, WmsBaseCompartment compartment)
        // TODO: consider moving this into the view
        {
            this.widthTrayPixel = widthTrayPixel;
            this.heightTrayPixel = heightTrayPixel;

            if (compartment.CompartmentDetails.Width != null && compartment.CompartmentDetails.Height != null && compartment.CompartmentDetails.XPosition != null && compartment.CompartmentDetails.YPosition != null)
            {
                compartment.Width = GraphicUtils.ConvertMillimetersToPixel((int)compartment.CompartmentDetails.Width, widthTrayPixel, this.Tray.Dimension.Width);
                compartment.Height = GraphicUtils.ConvertMillimetersToPixel((int)compartment.CompartmentDetails.Height, widthTrayPixel, this.Tray.Dimension.Width);

                Dimension compartmentDimension = new Dimension() { Width = (int)compartment.CompartmentDetails.Width, Height = (int)compartment.CompartmentDetails.Height };
                Position compartmentOrigin = new Position { X = (int)compartment.CompartmentDetails.XPosition, Y = (int)compartment.CompartmentDetails.YPosition };

                double originY = GraphicUtils.ConvertMillimetersToPixel(compartmentOrigin.Y, widthTrayPixel, this.Tray.Dimension.Width);
                double originX = GraphicUtils.ConvertMillimetersToPixel(compartmentOrigin.X, widthTrayPixel, this.Tray.Dimension.Width);
                Position compartmentOriginPixel = new Position { X = (int)Math.Floor(originX), Y = (int)Math.Floor(originY) };

                Position convertedCompartmentOrigin = GraphicUtils.ConvertWithStandardOriginPixel(compartmentOriginPixel, this.Tray, widthTrayPixel, heightTrayPixel, compartment.Width, compartment.Height);
                compartment.Top = convertedCompartmentOrigin.Y;
                compartment.Left = convertedCompartmentOrigin.X;
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

        #endregion Methods
    }
}
