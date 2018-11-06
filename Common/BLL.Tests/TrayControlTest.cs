using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Feretto.Common.BLL.Tests
{
    [TestClass]
    internal class TrayControlTest
    {
        #region Methods

        [TestMethod]
        public void TestControlSize100()
        {
            Tray tray = new Tray
            {
                Dimension = new Dimension { Height = 400, Width = 1000 },
                ReadOnly = false
            };
            //100
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "1"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "2"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "1"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "2"
            });
        }

        [TestMethod]
        public void TestControlSize200()
        {
            Tray tray = new Tray
            {
                Dimension = new Dimension { Height = 400, Width = 1000 },
                ReadOnly = false
            };
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "1"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "2"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "3"
            });

            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "4"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "5"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "6"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "7"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "8"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "9"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "10"
            });
            //RaisePropertyChanged(nameof(Tray));
        }

        [TestMethod]
        public void TestControlSize50()
        {
            Tray tray = new Tray
            {
                Dimension = new Dimension { Height = 400, Width = 1000 },
                ReadOnly = false
            };
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "1"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "2"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "1"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "2"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "1"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "2"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "1"
            });
            tray.AddCompartment(new CompartmentDetails()
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
                CompartmentName = "2"
            });
        }

        #endregion Methods
    }
}
