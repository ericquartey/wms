using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DevExpress.Mvvm;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Compartment
{
    public class CompartmentViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly Func<Color> DefaultColorCompartment = null;

        private readonly List<Enumeration> filterColoringCompartment = new List<Enumeration>
        {
            new Enumeration(0, "% Filling"),
            new Enumeration(1, "Linked Item (Null/ Any)"),
            new Enumeration(2, "Type Scompartment"),
            new Enumeration(3, "Article"),
        };

        private Func<Color> coloringFuncCompartment;
        private CompartmentDetails compartmentSelected;

        //            default:
        //                color = Colors.Black;
        //                break;
        //        }
        //    }
        //    return color;
        //};
        // { R = 100, G = 100, B = 100 };
        private ICommand createNewCompartmentCommand;

        //            case 4:
        //                color = Colors.Blue;
        //                break;
        private bool open = false;

        //            case 3:
        //                color = Colors.Green;
        //                break;
        private int selectedFilter;

        //            case 2:
        //                color = Colors.Orange;
        //                break;
        private bool showBackground;

        //private Func<CompartmentDetails, Enumeration, Color> coloringFuncCompartment = delegate (CompartmentDetails compartment, Enumeration selectedFilter)
        //{
        //    //= x => Colors.Green;
        //    Color color = Colors.Gray;
        //    if (selectedFilter != null)
        //    {
        //        var idFilter = selectedFilter.Id;
        //        switch (idFilter)
        //        {
        //            case 1:
        //                color = Colors.Violet;
        //                break;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public CompartmentViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public Func<Color> ColoringFuncCompartment
        {
            get
            {
                //if (!this.open)
                //{
                //    this.TestInitialization();
                //    this.open = true;
                //}
                //this.SetProperty(ref this.coloringFuncCompartment, this.coloringFuncCompartment);
                return this.coloringFuncCompartment;
            }
            set
            {
                //if (value == null)
                //{
                //    this.coloringFuncCompartment = DefaultColorCompartment;
                //}
                //else
                //{
                //    this.coloringFuncCompartment = value;
                //}

                this.SetProperty(ref this.coloringFuncCompartment, value);
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

        //public Func<Enumeration, Color> ColoringFuncCompartment
        //{
        //    get
        //    {
        //        return this.coloringFuncCompartment;
        //    }
        //    set
        //    {
        //        this.coloringFuncCompartment = value;
        //    }
        //}
        public ICommand CreateNewCompartmentCommand => this.createNewCompartmentCommand ??
                         (this.createNewCompartmentCommand = new DelegateCommand(this.ExecuteNewCreateCompartmentCommand));

        public List<Enumeration> FilterColoringCompartment { get => this.filterColoringCompartment; }

        public int SelectedFilter
        {
            get
            {
                if (this.selectedFilter == null)
                {
                    this.selectedFilter = 0;
                }
                return this.selectedFilter;
            }
            set
            {
                this.selectedFilter = value;
                //this.RaisePropertyChanged(nameof(this.SelectedFilter));
                //this.ColoringFuncCompartment(this.FilterColoringCompartment[this.SelectedFilter]);
                //this.RaisePropertyChanged(nameof(this.ColoringFuncCompartment));

                this.ChangeFilterColoringCompartment(this.selectedFilter);
                //this.RaisePropertyChanged(nameof(this.Tray.Compartments));
            }
        }

        public bool ShowBackground
        {
            get => this.showBackground;
            set => this.SetProperty(ref this.showBackground, value);
        }

        public Tray Tray
        {
            get => this.tray;
            set => this.SetProperty(ref this.tray, value);
        }

        #endregion Properties

        #region Methods

        public Func<Color> ChangeBlue()
        {
            return () => Colors.Blue;
        }

        public Func<Color> ChangeYellow()
        {
            return () => Colors.Yellow;
        }

        protected override void OnAppear()
        {
            this.TestInitialization();
            //this.InitializeInput();
        }

        private void ChangeFilterColoringCompartment(int selectedFilterColor)
        {
            //foreach (var compartment in this.Tray.Compartments)
            //{
            //this.SelectedFilter = selectedFilterColor;
            //this.ColoringFuncCompartment();
            ////}
            //this.RaisePropertyChanged(nameof(this.Tray.Compartments));

            //this.SelectedFilter = selectedFilterColor;

            //foreach (this.Tray.Compartments
            IFilter filterSelected;
            Color color;
            Func<Color> testfunc = null;
            switch (selectedFilterColor)
            {
                case 0:
                    testfunc = this.ChangeBlue();
                    filterSelected = new ArticleFilter();
                    break;

                case 1:
                    testfunc = this.ChangeYellow();
                    filterSelected = new CompartmentFilter();
                    break;
            }

            //this.coloringFuncCompartment = testfunc;
            //this.ColoringFuncCompartment = testfunc;
            //var x = this.ColoringFuncCompartment();

            //foreach(CompartmentDetails compartment in this.Tray.Compartments)
            //{
            //    compartment.
            //}

            //this.RaisePropertyChanged(nameof(this.ColoringFuncCompartment));

            //this.coloringFuncCompartment = testfunc;

            this.SetProperty(ref this.coloringFuncCompartment, testfunc);
            this.RaisePropertyChanged(nameof(this.ColoringFuncCompartment));
            //this.ColoringFuncCompartment();
            this.ColoringFuncCompartment.Invoke();

            //this.SetProperty(ref this.coloringFuncCompartment, testfunc);
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

        private void TestInitialization()
        {
            //Initialize without Origin, default: BOTTOM-LEFT
            this.tray = new Tray
            {
                Dimension = new Dimension { Height = 500, Width = 1960 }
            };

            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 800, YPosition = 0, Code = "1", Id = 1, CompartmentStatusDescription = "Sardine" });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1000, YPosition = 0, Code = "2", Id = 2, ItemDescription = "Cavolfiori" });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 0, YPosition = 0, Code = "3", Id = 3, ItemDescription = "Palle" });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1760, YPosition = 300, Code = "4", Id = 4, ItemDescription = "Spugne" });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 0, YPosition = 300, Code = "5", Id = 5, ItemDescription = "Chiodi" });
            this.RaisePropertyChanged(nameof(this.Tray));
            //this.RaisePropertyChanged(nameof(this.Tray.Compartments));
        }

        #endregion Methods
    }
}
