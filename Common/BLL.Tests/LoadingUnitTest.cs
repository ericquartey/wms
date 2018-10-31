using Ferretto.Common.BusinessModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Feretto.Common.BLL.Tests
{
    [TestClass]
    public class LoadingUnitTest
    {
        #region Methods

        [TestMethod]
        public void AddAdjacentCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100 };
            var compartment1 = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 0, YPosition = 10, Width = 10, Height = 10 };
            var compartment3 = new CompartmentDetails { XPosition = 10, YPosition = 0, Width = 10, Height = 10 };

            loadingUnit.AddCompartment(compartment1);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment2));
            loadingUnit.AddCompartment(compartment2);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment3));
            loadingUnit.AddCompartment(compartment3);
        }

        [TestMethod]
        public void AddAllAdjacentCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100 };
            var compartment1 = new CompartmentDetails { XPosition = 30, YPosition = 30, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 40, YPosition = 30, Width = 10, Height = 10 };
            var compartment3 = new CompartmentDetails { XPosition = 40, YPosition = 40, Width = 10, Height = 10 };
            var compartment4 = new CompartmentDetails { XPosition = 30, YPosition = 40, Width = 10, Height = 10 };
            var compartment5 = new CompartmentDetails { XPosition = 20, YPosition = 40, Width = 10, Height = 10 };
            var compartment6 = new CompartmentDetails { XPosition = 20, YPosition = 30, Width = 10, Height = 10 };
            var compartment7 = new CompartmentDetails { XPosition = 20, YPosition = 20, Width = 10, Height = 10 };
            var compartment8 = new CompartmentDetails { XPosition = 30, YPosition = 20, Width = 10, Height = 10 };
            var compartment9 = new CompartmentDetails { XPosition = 40, YPosition = 20, Width = 10, Height = 10 };

            loadingUnit.AddCompartment(compartment1);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment2));
            loadingUnit.AddCompartment(compartment2);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment3));
            loadingUnit.AddCompartment(compartment3);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment4));
            loadingUnit.AddCompartment(compartment4);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment5));
            loadingUnit.AddCompartment(compartment5);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment6));
            loadingUnit.AddCompartment(compartment6);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment7));
            loadingUnit.AddCompartment(compartment7);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment8));
            loadingUnit.AddCompartment(compartment8);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment9));
            loadingUnit.AddCompartment(compartment9);
        }

        [TestMethod]
        public void AddAllOverlapsCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100 };
            var compartment1 = new CompartmentDetails { XPosition = 30, YPosition = 30, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 39, YPosition = 30, Width = 10, Height = 10 };
            var compartment3 = new CompartmentDetails { XPosition = 39, YPosition = 39, Width = 10, Height = 10 };
            var compartment4 = new CompartmentDetails { XPosition = 30, YPosition = 29, Width = 10, Height = 10 };
            var compartment5 = new CompartmentDetails { XPosition = 21, YPosition = 39, Width = 10, Height = 10 };
            var compartment6 = new CompartmentDetails { XPosition = 21, YPosition = 30, Width = 10, Height = 10 };
            var compartment7 = new CompartmentDetails { XPosition = 21, YPosition = 21, Width = 10, Height = 10 };
            var compartment8 = new CompartmentDetails { XPosition = 30, YPosition = 21, Width = 10, Height = 10 };
            var compartment9 = new CompartmentDetails { XPosition = 39, YPosition = 21, Width = 10, Height = 10 };

            loadingUnit.AddCompartment(compartment1);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment2));
            loadingUnit.AddCompartment(compartment2);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment3));
            loadingUnit.AddCompartment(compartment3);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment4));
            loadingUnit.AddCompartment(compartment4);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment5));
            loadingUnit.AddCompartment(compartment5);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment6));
            loadingUnit.AddCompartment(compartment6);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment7));
            loadingUnit.AddCompartment(compartment7);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment8));
            loadingUnit.AddCompartment(compartment8);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment9));
            loadingUnit.AddCompartment(compartment9);
        }

        [TestMethod]
        public void AddCompartmentBiggerThanLoadingUnit()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100 };
            var compartment = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = loadingUnit.Length + 1, Height = loadingUnit.Width + 1 };

            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment));
        }

        [TestMethod]
        public void AddCompartmentOnTheEdgeOfLoadingUnit()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100 };
            var compartment = new CompartmentDetails { XPosition = 99, YPosition = 0, Width = 10, Height = 10 };

            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment));
        }

        [TestMethod]
        public void AddCompartmentOutOfLoadingUnit()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100 };
            var compartment = new CompartmentDetails { XPosition = 200, YPosition = 0, Width = 10, Height = 10 };

            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment));
        }

        [TestMethod]
        public void AddCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100 };
            var compartment1 = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 0, YPosition = 11, Width = 10, Height = 10 };

            loadingUnit.AddCompartment(compartment1);
            Assert.IsTrue(loadingUnit.CanAddCompartment(compartment2));
        }

        [TestMethod]
        public void AddOverlappingCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100 };
            var compartment1 = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 0, YPosition = 9, Width = 10, Height = 10 };

            loadingUnit.AddCompartment(compartment1);
            Assert.IsFalse(loadingUnit.CanAddCompartment(compartment2));
        }

        #endregion Methods
    }
}
