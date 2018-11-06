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

        private readonly List<Enumeration> filterColoringCompartment = new List<Enumeration>
        {
            new Enumeration(1, "% Filling"),
            new Enumeration(2, "Linked Item (Null/ Any)"),
            new Enumeration(3, "Type Scompartment"),
            new Enumeration(4, "Article"),
            new Enumeration(5, "X"),
        };

        private ICommand createNewCompartmentCommand;
        private ICommand resetSelectedCompartment;
        private Func<CompartmentDetails, CompartmentDetails, Color> selectedColorFilterFunc;
        private CompartmentDetails selectedCompartment;
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

        public ICommand CreateNewCompartmentCommand => this.createNewCompartmentCommand ??
                         (this.createNewCompartmentCommand = new DelegateCommand(this.ExecuteNewCreateCompartmentCommand));

        public List<Enumeration> FilterColoringCompartment { get => this.filterColoringCompartment; }

        public ICommand ResetSelectedCompartment => this.resetSelectedCompartment ??
                         (this.resetSelectedCompartment = new DelegateCommand(this.ExecuteResetSelectedCompartment));

        public Func<CompartmentDetails, CompartmentDetails, Color> SelectedColorFilterFunc
        {
            get => this.selectedColorFilterFunc;

            set => this.SetProperty(ref this.selectedColorFilterFunc, value);
        }

        public CompartmentDetails SelectedCompartment
        {
            get => this.selectedCompartment;
            set
            {
                this.SetProperty(ref this.selectedCompartment, value);
            }
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

        #region Methods

        protected override void OnAppear()
        {
            this.TestInitializationTray();
        }

        private void ChangeFilterColoringCompartment(int selectedFilterColor)
        {
            IFilter filterSelected = null;
            Func<CompartmentDetails, CompartmentDetails, Color> testfunc = null;
            switch (selectedFilterColor)
            {
                case 1:
                    filterSelected = new FillingFilter();
                    break;

                case 2:
                    filterSelected = new LinkedItemFilter();
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
            filterSelected.Selected = this.SelectedCompartment;
            testfunc = filterSelected.ColorFunc;

            this.SetProperty(ref this.selectedColorFilterFunc, testfunc);
            this.RaisePropertyChanged(nameof(this.SelectedColorFilterFunc));
        }

        private void CompatmentSelected_UpdateCompartmentEvent(Object sender, EventArgs e)
        {
            this.SelectedCompartment = (CompartmentDetails)sender;
        }

        private void ExecuteNewCreateCompartmentCommand()
        {
            var compartmentDetails = new CompartmentDetails
            {
                Width = this.SelectedCompartment.Width,
                Height = this.SelectedCompartment.Height,
                XPosition = this.SelectedCompartment.XPosition,
                YPosition = this.SelectedCompartment.YPosition
            };
            this.Tray.AddCompartment(compartmentDetails);
        }

        private void ExecuteResetSelectedCompartment()
        {
            this.SelectedCompartment = null;
        }

        private void TestCompartment100()
        {
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 0,
                Code = "3",
                Id = 3,
                ItemDescription = "Palle",
                Stock = 70,
                MaxCapacity = 100,
                MaterialStatusId = 7,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 100,
                Code = "2",
                Id = 2,
                ItemDescription = "Cavolfiori",
                Stock = 45,
                MaxCapacity = 100,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 200,
                Code = "3",
                Id = 3,
                ItemDescription = "Palle",
                Stock = 70,
                MaxCapacity = 100,
                MaterialStatusId = 7,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 300,
                Code = "2",
                Id = 2,
                ItemDescription = "Cavolfiori",
                Stock = 45,
                MaxCapacity = 100,
            });
        }

        private void TestCompartment200()
        {
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 200,
                YPosition = 0,
                Code = "1",
                Id = 1,
                CompartmentStatusDescription = "Sardine",
                Stock = 0,
                MaxCapacity = 100,
            });

            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 200,
                YPosition = 200,
                Code = "4",
                Id = 4,
                ItemDescription = "Spugne",
                Stock = 80,
                MaxCapacity = 100,
                CompartmentTypeId = 4,
                MaterialStatusId = 7,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 400,
                YPosition = 0,
                Code = "5",
                Id = 5,
                ItemDescription = "Chiodi",
                Stock = 100,
                MaxCapacity = 100,
                ItemPairing = 2,
                CompartmentTypeId = 4,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 400,
                YPosition = 200,
                Code = "6",
                Id = 6,
                ItemDescription = "Chiodi",
                Stock = 100,
                MaxCapacity = 100,
                ItemPairing = 2,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 600,
                YPosition = 0,
                Code = "7",
                Id = 7,
                ItemDescription = "Chiodi",
                Stock = 100,
                MaxCapacity = 100,
                ItemPairing = 2,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 600,
                YPosition = 200,
                Code = "7",
                Id = 7,
                ItemDescription = "Chiodi",
                Stock = 100,
                MaxCapacity = 100,
                ItemPairing = 2,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 800,
                YPosition = 0,
                Code = "7",
                Id = 7,
                ItemDescription = "Chiodi",
                Stock = 100,
                MaxCapacity = 100,
                ItemPairing = 2,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 800,
                YPosition = 200,
                Code = "7",
                Id = 7,
                ItemDescription = "Chiodi",
                Stock = 100,
                MaxCapacity = 100,
                ItemPairing = 2,
            });
        }

        private void TestCompartment50()
        {
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 0,
                Code = "3",
                Id = 3,
                ItemDescription = "Palle",
                Stock = 70,
                MaxCapacity = 100,
                MaterialStatusId = 7,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 50,
                Code = "2",
                Id = 2,
                ItemDescription = "Cavolfiori",
                Stock = 45,
                MaxCapacity = 100,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 100,
                Code = "3",
                Id = 3,
                ItemDescription = "Palle",
                Stock = 70,
                MaxCapacity = 100,
                MaterialStatusId = 7,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 150,
                Code = "2",
                Id = 2,
                ItemDescription = "Cavolfiori",
                Stock = 45,
                MaxCapacity = 100,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 200,
                Code = "3",
                Id = 3,
                ItemDescription = "Palle",
                Stock = 70,
                MaxCapacity = 100,
                MaterialStatusId = 7,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 250,
                Code = "2",
                Id = 2,
                ItemDescription = "Cavolfiori",
                Stock = 45,
                MaxCapacity = 100,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 300,
                Code = "3",
                Id = 3,
                ItemDescription = "Palle",
                Stock = 70,
                MaxCapacity = 100,
                MaterialStatusId = 7,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 350,
                Code = "2",
                Id = 2,
                ItemDescription = "Cavolfiori",
                Stock = 45,
                MaxCapacity = 100,
            });
        }

        private void TestCompartmentDefault()
        {
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
                MaterialStatusId = 7,
            });
            this.tray.AddCompartment(new CompartmentDetails()
            {
                Width = 200,
                Height = 200,
                XPosition = 0,
                YPosition = 200,
                Code = "2",
                Id = 2,
                ItemDescription = "Cavolfiori",
                Stock = 45,
                MaxCapacity = 100,
            });
        }

        private void TestInitializationTray()
        {
            //Initialize without Origin, default: BOTTOM-LEFT
            this.tray = new Tray
            {
                Dimension = new Dimension { Height = 400, Width = 1000 },
                //Origin = new Position { X = 0, Y = 0 },
                ReadOnly = true,
            };

            //this.TestCompartmentDefault();
            this.TestCompartment100();
            this.RaisePropertyChanged(nameof(this.Tray));
        }

        private void TestInitializeInput()
        {
            this.SelectedCompartment = new CompartmentDetails
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
