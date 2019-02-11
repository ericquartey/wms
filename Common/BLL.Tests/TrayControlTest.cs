using Ferretto.Common.BusinessModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Feretto.Common.BLL.Tests
{
    [TestClass]
    public class TrayControlTest
    {
        #region Methods

        // For test check on WIDTH = 1027 (2 border's pixel + 25 width ruler) HEIGHT= 427
        // To help add to CompartmentDetails: CompartmentName, binding on label
        [TestMethod]
        public void TestControlSize100()
        {
            var tray = new Tray
            {
                Dimension = new Dimension { Height = 400, Width = 1000 }
            };

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 0,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 100,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 200,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 300,
            });

            Assert.AreEqual(4, tray.Compartments.Count);
        }

        [TestMethod]
        public void TestControlSize200()
        {
            var tray = new Tray
            {
                Dimension = new Dimension { Height = 400, Width = 1000 }
            };

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 0,
                YPosition = 0,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 0,
                YPosition = 200,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 200,
                YPosition = 0,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 200,
                YPosition = 200,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 400,
                YPosition = 0,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 400,
                YPosition = 200,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 600,
                YPosition = 0,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 600,
                YPosition = 200,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 800,
                YPosition = 0,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 800,
                YPosition = 200,
            });

            Assert.AreEqual(10, tray.Compartments.Count);
        }

        [TestMethod]
        public void TestControlSize50()
        {
            var tray = new Tray
            {
                Dimension = new Dimension { Height = 400, Width = 1000 },
            };

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 0,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 50,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 100,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 150,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 200,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 250,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 300,
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 350,
            });

            Assert.AreEqual(8, tray.Compartments.Count);
        }

        #endregion
    }
}
