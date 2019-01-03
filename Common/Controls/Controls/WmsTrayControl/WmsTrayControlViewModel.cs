using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class WmsTrayControlViewModel : INotifyPropertyChanged
    {
        #region Fields

        private double heightTrayPixel;

        private bool isCompartmentSelectable = true;

        private ObservableCollection<WmsBaseCompartment> items;

        private int left;

        private SolidColorBrush penBrush;

        private int penThickness;

        private Func<ICompartment, ICompartment, string> selectedColorFilterFunc;

        private ICompartment selectedCompartment;

        private WmsBaseCompartment selectedItem;

        private int top;

        private Tray tray;

        private double widthTrayPixel;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public ICompartment CompartmentDetailsProperty { get; set; }

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

        public bool IsReadOnly { get; set; }

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
            get => this.left;
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

        public Func<ICompartment, ICompartment, string> SelectedColorFilterFunc
        {
            get => this.selectedColorFilterFunc;
            set
            {
                this.selectedColorFilterFunc = value;
                this.UpdateColorCompartments();
            }
        }

        public ICompartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set
            {
                if (this.selectedCompartment != value)
                {
                    this.selectedCompartment = value;
                    this.SetSelectedItem();
                }
            }
        }

        public WmsBaseCompartment SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.selectedItem != value)
                {
                    this.selectedItem = value;
                    this.NotifyPropertyChanged(nameof(this.SelectedItem));
                }
            }
        }

        public bool ShowBackground { get; set; }

        public bool ShowRuler { get; set; }

        public int Top
        {
            get => this.top;
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
            get => this.tray;
            set
            {
                if (this.tray != null)
                {
                    this.tray.Compartments.ListChanged -= this.Compartments_ListChanged;
                    this.tray.CompartmentChangedEvent -= this.Tray_CompartmentChangedEvent;
                }

                this.tray = value;
                this.tray.Compartments.ListChanged += this.Compartments_ListChanged;
                this.tray.CompartmentChangedEvent += this.Tray_CompartmentChangedEvent;

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
            if (this.items == null)
            {
                return;
            }

            foreach (var compartment in this.items)
            {
                this.ResizeCompartment(widthTrayPixel, heightTrayPixel, compartment);
            }

            this.SetSelectedItem();
        }

        public void UpdateCompartments(IEnumerable<ICompartment> compartments)
        {
            if (this.Tray == null)
            {
                return;
            }

            var newItems = new ObservableCollection<WmsBaseCompartment>();
            foreach (var compartment in compartments)
            {
                newItems.Add(new WmsCompartmentViewModel
                {
                    Tray = this.Tray,

                    CompartmentDetails = compartment,
                    Width = compartment.Width ?? 0,
                    Height = compartment.Height ?? 0,
                    Left = compartment.XPosition ?? 0,
                    Top = compartment.YPosition ?? 0,
                    ColorFill = this.SelectedColorFilterFunc?.Invoke(compartment, this.SelectedCompartment) ?? Application.Current.Resources["DefaultCompartmentColor"].ToString(),
                    IsReadOnly = this.IsReadOnly,
                    IsSelectable = this.IsCompartmentSelectable
                });
            }

            this.Items = newItems;
        }

        public void UpdateInputForm(CompartmentDetails compartment)
        {
            this.CompartmentDetailsProperty = compartment;
            this.NotifyPropertyChanged(nameof(this.CompartmentDetailsProperty));
        }

        public void UpdateIsReadOnlyPropertyToCompartments(bool value)
        {
            if (this.items == null)
            {
                return;
            }

            foreach (var item in this.Items)
            {
                item.IsReadOnly = value;
            }
        }

        public void UpdateIsSelectablePropertyToCompartments(bool value)
        {
            if (this.items == null)
            {
                return;
            }

            foreach (var item in this.Items)
            {
                item.IsSelectable = value;
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    ColorFill = this.SelectedColorFilterFunc?.Invoke(compartment, this.SelectedCompartment) ?? Application.Current.Resources["DefaultCompartmentColor"].ToString(),
                    IsReadOnly = this.IsReadOnly,
                    IsSelectable = this.IsCompartmentSelectable
                };
            }
            return null;
        }

        private void Compartments_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                this.items.RemoveAt(e.NewIndex);
            }
            else if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (sender is IList<CompartmentDetails> compartments)
                {
                    var addedCompartment = compartments[compartments.Count - 1];
                    var compartmentGraphic = this.AddCompartment(addedCompartment);

                    this.ResizeCompartment(this.widthTrayPixel, this.heightTrayPixel, compartmentGraphic);

                    this.items.Add(compartmentGraphic);
                }
            }

            this.NotifyPropertyChanged(nameof(this.Items));
        }

        private void LoadingUnitDetails_AddedCompartmentEvent(object sender, EventArgs e)
        {
            this.UpdateCompartments(this.Tray.Compartments);
        }

        private void ResizeCompartment(double widthTrayPixel, double heightTrayPixel, WmsBaseCompartment compartment)
        // TODO: consider moving this into the view
        {
            if (compartment == null ||
                this.tray == null)
            {
                return;
            }

            this.widthTrayPixel = widthTrayPixel;
            this.heightTrayPixel = heightTrayPixel;

            if (compartment.CompartmentDetails.Width == null || compartment.CompartmentDetails.Height == null ||
                compartment.CompartmentDetails.XPosition == null || compartment.CompartmentDetails.YPosition == null)
            {
                return;
            }

            var compartmentOrigin = new Position
            {
                X = (int)compartment.CompartmentDetails.XPosition,
                Y = (int)compartment.CompartmentDetails.YPosition
            };
            var convertedCompartmentOrigin = GraphicUtils.ConvertWithStandardOrigin(
                compartmentOrigin,
                this.Tray,
                (int)compartment.CompartmentDetails.Width,
                (int)compartment.CompartmentDetails.Height);

            var compartmentEnd = new Position
            {
                X = convertedCompartmentOrigin.X + (int)compartment.CompartmentDetails.Width,
                Y = convertedCompartmentOrigin.Y + (int)compartment.CompartmentDetails.Height,
            };

            compartment.Top = GraphicUtils.ConvertMillimetersToPixel(
                convertedCompartmentOrigin.Y,
                heightTrayPixel,
                this.Tray.Dimension.Height);
            compartment.Left = GraphicUtils.ConvertMillimetersToPixel(
                convertedCompartmentOrigin.X,
                widthTrayPixel,
                this.Tray.Dimension.Width);

            var bottom = GraphicUtils.ConvertMillimetersToPixel(
                compartmentEnd.Y,
                heightTrayPixel,
                this.Tray.Dimension.Height);
            var right = GraphicUtils.ConvertMillimetersToPixel(
                compartmentEnd.X,
                widthTrayPixel,
                this.Tray.Dimension.Width);
            compartment.Height = bottom - compartment.Top;
            compartment.Width = right - compartment.Left;
        }

        private void SetSelectedItem()
        {
            if (this.selectedCompartment == null ||
                this.Items == null)
            {
                this.SelectedItem = null;
                return;
            }
            var foundCompartment = this.Items.FirstOrDefault(c => c.CompartmentDetails.Id == this.selectedCompartment.Id);
            if (foundCompartment == null)
            {
                this.SelectedItem = null;
                return;
            }

            if (this.selectedItem != null &&
                this.selectedItem.CompartmentDetails.Id == this.selectedCompartment.Id)
            {
                return;
            }

            this.SelectedItem = foundCompartment;
            this.UpdateColorCompartments();
            this.ResizeCompartment(this.widthTrayPixel, this.heightTrayPixel, this.selectedItem);
        }

        private void Tray_CompartmentChangedEvent(object sender, CompartmentEventArgs e)
        {
            this.SelectedCompartment = e.Compartment;
        }

        private void UpdateColorCompartments()
        {
            if (this.items == null || this.selectedColorFilterFunc == null)
            {
                return;
            }

            foreach (var item in this.Items)
            {
                item.ColorFill = this.selectedColorFilterFunc.Invoke(item.CompartmentDetails, this.SelectedCompartment) ?? Application.Current.Resources["DefaultCompartmentColor"].ToString();
            }
        }

        private void UpdateIsSelectablePropertyToCompartments()
        {
            if (this.items == null)
            {
                return;
            }

            foreach (var item in this.Items)
            {
                item.IsSelectable = this.isCompartmentSelectable;
            }
        }

        #endregion Methods
    }
}
