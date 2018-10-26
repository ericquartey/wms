using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Compartment
{
    public class CompartmentViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly List<Enumeration> filterColoringCompartment = new List<Enumeration>
        {
            new Enumeration(1, "% Filling"),
            new Enumeration(2, "Linked Item (Null/ Any)"),
            new Enumeration(3, "Type Scompartment"),
            new Enumeration(4, "Article"),
        };

        //private Func<CompartmentDetails, Color> coloringFuncCompartment = x => Colors.Green;
        private Func<CompartmentDetails, Color> coloringFuncCompartment = delegate (CompartmentDetails compartment)
        {
            //= x => Colors.Green;
            Color color = Colors.Gray;
            if (compartment.FilterColoring != null)
            {
                var idFilter = compartment.FilterColoring.Id;
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
            }
            return color;
        };

        private CompartmentDetails compartmentSelected;

        // { R = 100, G = 100, B = 100 };
        private ICommand createNewCompartmentCommand;

        private Enumeration selected;

        private bool showBackground;

        private Tray tray;

        #endregion Fields

        #region Constructors

        public CompartmentViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public Func<CompartmentDetails, Color> ColoringFuncCompartment
        {
            get
            {
                return this.coloringFuncCompartment;
            }
            set
            {
                this.coloringFuncCompartment = value;
            }
        }

        public CompartmentDetails CompartmentSelected
        {
            get => this.compartmentSelected;
            set
            {
                this.compartmentSelected = value;
                this.RaisePropertyChanged(nameof(this.CompartmentSelected));
            }
        }

        public ICommand CreateNewCompartmentCommand => this.createNewCompartmentCommand ??
                         (this.createNewCompartmentCommand = new DelegateCommand(this.ExecuteNewCreateCompartmentCommand));

        public List<Enumeration> FilterColoringCompartment { get => this.filterColoringCompartment; }

        public Enumeration Selected
        {
            get
            {
                if (this.selected == null)
                {
                    this.selected = this.FilterColoringCompartment[0];
                }
                return this.selected;
            }
            set
            {
                this.selected = value;
                this.ChangeFilterColoringCompartment(this.selected);
            }
        }

        public bool ShowBackground
        {
            get => this.showBackground;
            set => this.SetProperty(ref this.showBackground, value);
        }

        public Tray Tray { get => this.tray; set => this.SetProperty(ref this.tray, value); }

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            //Initialize without Origin, default: BOTTOM-LEFT
            this.tray = new Tray
            {
                Dimension = new Dimension { Height = 500, Width = 1960 }
            };

            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 800, YPosition = 0, Code = "1", Id = 1 });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1000, YPosition = 0, Code = "2", Id = 2 });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 0, YPosition = 0, Code = "3", Id = 3 });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1760, YPosition = 300, Code = "4", Id = 4 });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 0, YPosition = 300, Code = "4", Id = 4 });
            this.RaisePropertyChanged(nameof(this.Tray));
            this.RaisePropertyChanged(nameof(this.Tray.Compartments));

            this.InitializeInput();
        }

        private void ChangeFilterColoringCompartment(Enumeration selectedFilterColor)
        {
            foreach (var compartment in this.Tray.Compartments)
            {
                compartment.FilterColoring = selectedFilterColor;
            }
            this.RaisePropertyChanged(nameof(this.Tray.Compartments));
        }

        private void CompatmentSelected_UpdateCompartmentEvent(Object sender, EventArgs e)
        {
            this.CompartmentSelected = (CompartmentDetails)sender;
        }

        private void ExecuteNewCreateCompartmentCommand()
        {
            var compartmentDetails = new CompartmentDetails
            {
                Width = this.CompartmentSelected.Width,
                Height = this.CompartmentSelected.Height,
                XPosition = this.CompartmentSelected.XPosition,
                YPosition = this.CompartmentSelected.YPosition
            };
            this.Tray.AddCompartment(compartmentDetails);
        }

        private void InitializeInput()
        {
            this.CompartmentSelected = new CompartmentDetails
            {
                Width = 1,
                Height = 1,
                XPosition = 0,
                YPosition = 0,
                Stock = 0,
                ItemCode = ""
            };
        }

        #endregion Methods
    }
}
