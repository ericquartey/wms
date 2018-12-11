using Ferretto.Common.BusinessModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Feretto.Common.BLL.Tests
{
    [TestClass]
    public class TrayControlTest
    {
        #region Methods

        //For test check on WIDTH = 1027 (2 border's pixel + 25 width ruler) HEIGHT= 427
        //To help add to CompartmentDetails: CompartmentName, binding on label
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
                Code = "3",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 100,
                Code = "2",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 200,
                Code = "3",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 100,
                Height = 100,
                XPosition = 0,
                YPosition = 300,
                Code = "2",
            });
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
                Code = "3",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 0,
                YPosition = 200,
                Code = "2",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 200,
                YPosition = 0,
                Code = "1",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 200,
                YPosition = 200,
                Code = "4",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 400,
                YPosition = 0,
                Code = "5",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 400,
                YPosition = 200,
                Code = "6",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 600,
                YPosition = 0,
                Code = "7",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 600,
                YPosition = 200,
                Code = "7",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 800,
                YPosition = 0,
                Code = "7",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 200,
                Height = 200,
                XPosition = 800,
                YPosition = 200,
                Code = "7",
            });
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
                Code = "3",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 50,
                Code = "2",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 100,
                Code = "3",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 150,
                Code = "2",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 200,
                Code = "3",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 250,
                Code = "2",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 300,
                Code = "3",
            });

            tray.AddCompartment(new CompartmentDetails
            {
                Width = 50,
                Height = 50,
                XPosition = 0,
                YPosition = 350,
                Code = "2",
            });
        }

        #endregion Methods
    }
}
