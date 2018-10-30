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

        private readonly Func<IFilter, Color> DefaultColorCompartment = null;

        private readonly List<Enumeration> filterColoringCompartment = new List<Enumeration>
        {
            new Enumeration(1, "% Filling"),
            new Enumeration(2, "Linked Item (Null/ Any)"),
            new Enumeration(3, "Type Scompartment"),
            new Enumeration(4, "Article"),
            new Enumeration(5, "X"),
        };

        private CompartmentDetails compartmentSelected;
        private ICommand createNewCompartmentCommand;

        private ICommand resetCompartmentSelected;
        private Func<CompartmentDetails, CompartmentDetails, Color> selectedColorFilterFunc;
        private int selectedFilter;
        private bool showBackground;

        private Tray tray;

        #endregion Fields

        #region Constructors

        public CompartmentViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails CompartmentSelected
        {
            get => this.compartmentSelected;
            set
            {
                this.SetProperty(ref this.compartmentSelected, value);
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

        public ICommand ResetCompartmentSelected => this.resetCompartmentSelected ??
                         (this.resetCompartmentSelected = new DelegateCommand(this.ExecuteResetCompartmentSelected));

        private void ExecuteResetCompartmentSelected()
        {
            this.CompartmentSelected = null;
        }

        public Func<CompartmentDetails, CompartmentDetails, Color> SelectedColorFilterFunc
        {
            get => this.selectedColorFilterFunc;

            set => this.SetProperty(ref this.selectedColorFilterFunc, value);
        }

        public int SelectedFilter
        {
            get
            {
                if (this.selectedFilter == null)
                {
                    this.selectedFilter = -1;
                }
                return this.selectedFilter;
            }
            set
            {
                this.selectedFilter = value;
                this.ChangeFilterColoringCompartment(this.selectedFilter);
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

        //public Color ChangeBlue(CompartmentDetails compartment)
        //{
        //    return Colors.Blue;
        //}

        //public Color ChangeYellow(CompartmentDetails compartment)
        //{
        //    return Colors.Yellow;
        //}

        #region Methods

        protected override void OnAppear()
        {
            this.TestInitialization();
            //this.InitializeInput();
        }

        private void ChangeFilterColoringCompartment(int selectedFilterColor)
        {
            IFilter filterSelected = null;
            Color color;
            Func<CompartmentDetails, CompartmentDetails, Color> testfunc = null;
            switch (selectedFilterColor)
            {
                case 1:
                    filterSelected = new FillingFilter();
                    //testfunc = this.ChangeBlue;
                    break;

                case 2:
                    filterSelected = new LinkedItemFilter();
                    //testfunc = this.ChangeYellow;
                    break;

                case 3:
                    filterSelected = new CompartmentFilter();
                    break;

                case 4:
                    filterSelected = new ArticleFilter();
                    break;

                default:
                    filterSelected = new NotImplementdFilter();
                    break;
            }
            filterSelected.Selected = this.CompartmentSelected;
            testfunc = filterSelected.ColorFunc;

            this.SetProperty(ref this.selectedColorFilterFunc, testfunc);
            this.RaisePropertyChanged(nameof(this.SelectedColorFilterFunc));
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

            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 800,
                YPosition = 0,
                Code = "1",
                Id = 1,
                CompartmentStatusDescription = "Sardine",
                Stock = 0,
                MaxCapacity = 100
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 1000,
                YPosition = 0,
                Code = "2",
                Id = 2,
                ItemDescription = "Cavolfiori",
                Stock = 45,
                MaxCapacity = 100
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 0,
                YPosition = 0,
                Code = "3",
                Id = 3,
                ItemDescription = "Palle",
                Stock = 70,
                MaxCapacity = 100,
                MaterialStatusId = 7
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 1760,
                YPosition = 300,
                Code = "4",
                Id = 4,
                ItemDescription = "Spugne",
                Stock = 80,
                MaxCapacity = 100,
                CompartmentTypeId = 4,
                MaterialStatusId = 7
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 0,
                YPosition = 300,
                Code = "5",
                Id = 5,
                ItemDescription = "Chiodi",
                Stock = 100,
                MaxCapacity = 100,
                ItemPairing = 2,
                CompartmentTypeId = 4
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 400,
                YPosition = 300,
                Code = "6",
                Id = 6,
                ItemDescription = "Chiodi",
                Stock = 100,
                MaxCapacity = 100,
                ItemPairing = 2
            });
            this.RaisePropertyChanged(nameof(this.Tray));
            //this.RaisePropertyChanged(nameof(this.Tray.Compartments));
        }

        #endregion Methods
    }
}
