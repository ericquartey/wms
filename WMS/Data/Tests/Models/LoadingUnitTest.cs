using System.Collections.Generic;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.Common.BLL.Tests
{
    [TestClass]
    public class LoadingUnitTest
    {
        #region Methods

        [TestMethod]
        public void AddAdjacentCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100, LoadingUnitTypeHasCompartments = true };
            var compartment1 = new CompartmentDetails { Id = 1, XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { Id = 2, XPosition = 0, YPosition = 10, Width = 10, Height = 10 };
            var compartment3 = new CompartmentDetails { Id = 3, XPosition = 10, YPosition = 0, Width = 10, Height = 10 };

            var compartments = new List<CompartmentDetails> { compartment1 };
            Assert.IsTrue(compartment2.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment2);
            Assert.IsTrue(compartment3.CanAddToLoadingUnit(compartments, loadingUnit));
        }

        [TestMethod]
        public void AddAllAdjacentCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100, LoadingUnitTypeHasCompartments = true };
            var compartment1 = new CompartmentDetails { XPosition = 30, YPosition = 30, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 40, YPosition = 30, Width = 10, Height = 10 };
            var compartment3 = new CompartmentDetails { XPosition = 40, YPosition = 40, Width = 10, Height = 10 };
            var compartment4 = new CompartmentDetails { XPosition = 30, YPosition = 40, Width = 10, Height = 10 };
            var compartment5 = new CompartmentDetails { XPosition = 20, YPosition = 40, Width = 10, Height = 10 };
            var compartment6 = new CompartmentDetails { XPosition = 20, YPosition = 30, Width = 10, Height = 10 };
            var compartment7 = new CompartmentDetails { XPosition = 20, YPosition = 20, Width = 10, Height = 10 };
            var compartment8 = new CompartmentDetails { XPosition = 30, YPosition = 20, Width = 10, Height = 10 };
            var compartment9 = new CompartmentDetails { XPosition = 40, YPosition = 20, Width = 10, Height = 10 };

            var compartments = new List<CompartmentDetails> { compartment1 };
            Assert.IsTrue(compartment2.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment2);
            Assert.IsTrue(compartment3.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment3);
            Assert.IsTrue(compartment4.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment4);
            Assert.IsTrue(compartment5.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment5);
            Assert.IsTrue(compartment6.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment6);
            Assert.IsTrue(compartment7.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment7);
            Assert.IsTrue(compartment8.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment8);
            Assert.IsTrue(compartment9.CanAddToLoadingUnit(compartments, loadingUnit));
            compartments.Add(compartment9);
        }

        [TestMethod]
        public void AddAllOverlapsCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100, LoadingUnitTypeHasCompartments = true };
            var compartment1 = new CompartmentDetails { Id = 1, XPosition = 30, YPosition = 30, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { Id = 2, XPosition = 39, YPosition = 30, Width = 10, Height = 10 };

            var compartments = new List<CompartmentDetails> { compartment1 };
            Assert.IsFalse(compartment2.CanAddToLoadingUnit(compartments, loadingUnit));
        }

        [TestMethod]
        public void AddCompartmentBiggerThanLoadingUnit()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100, LoadingUnitTypeHasCompartments = true };
            var compartment = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = loadingUnit.Length + 1, Height = loadingUnit.Width + 1 };
            var compartments = new List<CompartmentDetails> { compartment };
            Assert.IsFalse(compartment.CanAddToLoadingUnit(compartments, loadingUnit));
        }

        [TestMethod]
        public void AddCompartmentOnTheEdgeOfLoadingUnit()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100, LoadingUnitTypeHasCompartments = true };
            var compartment = new CompartmentDetails { XPosition = 99, YPosition = 0, Width = 10, Height = 10 };
            var compartments = new List<CompartmentDetails> { compartment };
            Assert.IsFalse(compartment.CanAddToLoadingUnit(compartments, loadingUnit));
        }

        [TestMethod]
        public void AddCompartmentOutOfLoadingUnit()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100, LoadingUnitTypeHasCompartments = true };
            var compartment = new CompartmentDetails { XPosition = 200, YPosition = 0, Width = 10, Height = 10 };
            var compartments = new List<CompartmentDetails> { compartment };
            Assert.IsFalse(compartment.CanAddToLoadingUnit(compartments, loadingUnit));
        }

        [TestMethod]
        public void AddCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100, LoadingUnitTypeHasCompartments = true };
            var compartment1 = new CompartmentDetails { XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { XPosition = 0, YPosition = 11, Width = 10, Height = 10 };
            var compartmentsDetails = new List<CompartmentDetails>();
            compartmentsDetails.Add(compartment1);
            Assert.IsTrue(compartment2.CanAddToLoadingUnit(compartmentsDetails, loadingUnit));
        }

        [TestMethod]
        public void AddOverlappingCompartments()
        {
            var loadingUnit = new LoadingUnitDetails { Length = 100, Width = 100, LoadingUnitTypeHasCompartments = true };
            var compartment1 = new CompartmentDetails { Id = 1, XPosition = 0, YPosition = 0, Width = 10, Height = 10 };
            var compartment2 = new CompartmentDetails { Id = 2, XPosition = 0, YPosition = 9, Width = 10, Height = 10 };
            var compartmentsDetails = new List<CompartmentDetails>();
            compartmentsDetails.Add(compartment1);
            Assert.IsFalse(compartment2.CanAddToLoadingUnit(compartmentsDetails, loadingUnit));
        }

        #endregion
    }
}
