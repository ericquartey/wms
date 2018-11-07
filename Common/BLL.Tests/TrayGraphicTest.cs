using Ferretto.Common.BusinessModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Feretto.Common.BLL.Tests
{
    [TestClass]
    public class TrayGraphicTest
    {
        #region Methods

        [TestMethod]
        public void AddAdjacentCompartments()
        {
            var tray = new Tray { Dimension = new Dimension { Height = 100, Width = 100 } };
            var compartment1 = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 0, YPosition = 10, Width = 10, Height = 10 };
            var compartment3 = new CompartmentDetails { XPosition = 10, YPosition = 0, Width = 10, Height = 10 };

            tray.AddCompartment(compartment1);
            Assert.IsTrue(tray.CanAddCompartment(compartment2));
            tray.AddCompartment(compartment2);
            Assert.IsTrue(tray.CanAddCompartment(compartment3));
            tray.AddCompartment(compartment3);
        }

        [TestMethod]
        public void AddAllAdjacentCompartments()
        {
            var tray = new Tray { Dimension = new Dimension { Height = 100, Width = 100 } };
            var compartment1 = new CompartmentDetails { XPosition = 30, YPosition = 30, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 40, YPosition = 30, Width = 10, Height = 10 };
            var compartment3 = new CompartmentDetails { XPosition = 40, YPosition = 40, Width = 10, Height = 10 };
            var compartment4 = new CompartmentDetails { XPosition = 30, YPosition = 40, Width = 10, Height = 10 };
            var compartment5 = new CompartmentDetails { XPosition = 20, YPosition = 40, Width = 10, Height = 10 };
            var compartment6 = new CompartmentDetails { XPosition = 20, YPosition = 30, Width = 10, Height = 10 };
            var compartment7 = new CompartmentDetails { XPosition = 20, YPosition = 20, Width = 10, Height = 10 };
            var compartment8 = new CompartmentDetails { XPosition = 30, YPosition = 20, Width = 10, Height = 10 };
            var compartment9 = new CompartmentDetails { XPosition = 40, YPosition = 20, Width = 10, Height = 10 };

            tray.AddCompartment(compartment1);
            Assert.IsTrue(tray.CanAddCompartment(compartment2));
            tray.AddCompartment(compartment2);
            Assert.IsTrue(tray.CanAddCompartment(compartment3));
            tray.AddCompartment(compartment3);
            Assert.IsTrue(tray.CanAddCompartment(compartment4));
            tray.AddCompartment(compartment4);
            Assert.IsTrue(tray.CanAddCompartment(compartment5));
            tray.AddCompartment(compartment5);
            Assert.IsTrue(tray.CanAddCompartment(compartment6));
            tray.AddCompartment(compartment6);
            Assert.IsTrue(tray.CanAddCompartment(compartment7));
            tray.AddCompartment(compartment7);
            Assert.IsTrue(tray.CanAddCompartment(compartment8));
            tray.AddCompartment(compartment8);
            Assert.IsTrue(tray.CanAddCompartment(compartment9));
            tray.AddCompartment(compartment9);
        }

        [TestMethod]
        public void AddAllOverlapsCompartments()
        {
            var tray = new Tray { Dimension = new Dimension { Height = 100, Width = 100 } };
            var compartment1 = new CompartmentDetails { XPosition = 30, YPosition = 30, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 39, YPosition = 30, Width = 10, Height = 10 };
            var compartment3 = new CompartmentDetails { XPosition = 39, YPosition = 39, Width = 10, Height = 10 };
            var compartment4 = new CompartmentDetails { XPosition = 30, YPosition = 29, Width = 10, Height = 10 };
            var compartment5 = new CompartmentDetails { XPosition = 21, YPosition = 39, Width = 10, Height = 10 };
            var compartment6 = new CompartmentDetails { XPosition = 21, YPosition = 30, Width = 10, Height = 10 };
            var compartment7 = new CompartmentDetails { XPosition = 21, YPosition = 21, Width = 10, Height = 10 };
            var compartment8 = new CompartmentDetails { XPosition = 30, YPosition = 21, Width = 10, Height = 10 };
            var compartment9 = new CompartmentDetails { XPosition = 39, YPosition = 21, Width = 10, Height = 10 };

            tray.AddCompartment(compartment1);
            Assert.IsFalse(tray.CanAddCompartment(compartment2));
            tray.AddCompartment(compartment2);
            Assert.IsFalse(tray.CanAddCompartment(compartment3));
            tray.AddCompartment(compartment3);
            Assert.IsFalse(tray.CanAddCompartment(compartment4));
            tray.AddCompartment(compartment4);
            Assert.IsFalse(tray.CanAddCompartment(compartment5));
            tray.AddCompartment(compartment5);
            Assert.IsFalse(tray.CanAddCompartment(compartment6));
            tray.AddCompartment(compartment6);
            Assert.IsFalse(tray.CanAddCompartment(compartment7));
            tray.AddCompartment(compartment7);
            Assert.IsFalse(tray.CanAddCompartment(compartment8));
            tray.AddCompartment(compartment8);
            Assert.IsFalse(tray.CanAddCompartment(compartment9));
            tray.AddCompartment(compartment9);
        }

        [TestMethod]
        public void AddCompartmentBiggerThanTray()
        {
            var tray = new Tray { Dimension = new Dimension { Height = 100, Width = 100 } };
            var compartment = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = tray.Dimension.Height + 1, Height = tray.Dimension.Width + 1 };

            Assert.IsFalse(tray.CanAddCompartment(compartment));
        }

        [TestMethod]
        public void AddCompartmentOnTheEdgeOftray()
        {
            var tray = new Tray { Dimension = new Dimension { Height = 100, Width = 100 } };
            var compartment = new CompartmentDetails { XPosition = 99, YPosition = 0, Width = 10, Height = 10 };

            Assert.IsFalse(tray.CanAddCompartment(compartment));
        }

        [TestMethod]
        public void AddCompartmentOutOfTray()
        {
            var tray = new Tray { Dimension = new Dimension { Height = 100, Width = 100 } };
            var compartment = new CompartmentDetails { XPosition = 200, YPosition = 0, Width = 10, Height = 10 };

            Assert.IsFalse(tray.CanAddCompartment(compartment));
        }

        [TestMethod]
        public void AddCompartments()
        {
            var tray = new Tray { Dimension = new Dimension { Height = 100, Width = 100 } };
            var compartment1 = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 0, YPosition = 11, Width = 10, Height = 10 };

            tray.AddCompartment(compartment1);
            Assert.IsTrue(tray.CanAddCompartment(compartment2));
        }

        [TestMethod]
        public void AddOverlappingCompartments()
        {
            var tray = new Tray { Dimension = new Dimension { Height = 100, Width = 100 } };
            var compartment1 = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 0, YPosition = 9, Width = 10, Height = 10 };

            tray.AddCompartment(compartment1);
            Assert.IsFalse(tray.CanAddCompartment(compartment2));
        }

        #endregion Methods
    }
}
