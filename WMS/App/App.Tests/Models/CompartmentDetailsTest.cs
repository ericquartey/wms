using Ferretto.WMS.App.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.App.Tests
{
    [TestClass]
    public class CompartmentDetailsTest
    {
        #region Methods

        [TestMethod]
        [DataRow(null, true)]
        [DataRow(1, false)]
        public void LoadingUnitValidation(
           int? loadingUnitId,
           bool hasError)
        {
            var compartmentDetails = new CompartmentDetails
            {
                IsValidationEnabled = true,
                LoadingUnitId = loadingUnitId
            };

            Assert.AreEqual(
                hasError,
                !string.IsNullOrEmpty(compartmentDetails[nameof(compartmentDetails.LoadingUnitId)]),
                compartmentDetails.Error);
        }

        [TestMethod]
        [DataRow(null, null, true, true)]
        [DataRow(-1, -1, true, true)]
        [DataRow(0, 0, false, false)]
        [DataRow(1, 1, false, false)]
        public void PositionValidation(
            int? xPosition,
            int? yPosition,
            bool xPositionHasError,
            bool yPositionHasError)
        {
            var compartmentDetails = new CompartmentDetails
            {
                IsValidationEnabled = true,
                XPosition = xPosition,
                YPosition = yPosition
            };

            Assert.AreEqual(
                xPositionHasError,
                !string.IsNullOrEmpty(compartmentDetails[nameof(compartmentDetails.XPosition)]),
                compartmentDetails.Error);

            Assert.AreEqual(
                yPositionHasError,
                !string.IsNullOrEmpty(compartmentDetails[nameof(compartmentDetails.YPosition)]),
                compartmentDetails.Error);
        }

        [TestMethod]
        [DataRow(-1, -1, true, true)]
        [DataRow(0, 0, false, false)]
        [DataRow(1, 1, false, false)]
        public void ReservedQuantitiesValidation(
            double reservedForPick,
            double reservedToPut,
            bool pickHasError,
            bool putHasError)
        {
            var compartmentDetails = new CompartmentDetails
            {
                IsValidationEnabled = true,
                ReservedForPick = reservedForPick,
                ReservedToPut = reservedToPut
            };

            Assert.AreEqual(
                pickHasError,
                !string.IsNullOrEmpty(compartmentDetails[nameof(compartmentDetails.ReservedForPick)]),
                compartmentDetails.Error);

            Assert.AreEqual(
                putHasError,
                !string.IsNullOrEmpty(compartmentDetails[nameof(compartmentDetails.ReservedToPut)]),
                compartmentDetails.Error);
        }

        [TestMethod]
        [DataRow(null, null, true, true)]
        [DataRow(-1.0, -1.0, true, true)]
        [DataRow(0.0, 0.0, true, true)]
        [DataRow(1.0, 1.0, false, false)]
        public void SizeValidation(
            double? width,
            double? height,
            bool widthHasError,
            bool heightHasError)
        {
            var compartmentDetails = new CompartmentDetails
            {
                IsValidationEnabled = true,
                Width = width,
                Height = height
            };

            Assert.AreEqual(
                widthHasError,
                !string.IsNullOrEmpty(compartmentDetails[nameof(compartmentDetails.Width)]),
                compartmentDetails.Error);

            Assert.AreEqual(
                heightHasError,
                !string.IsNullOrEmpty(compartmentDetails[nameof(compartmentDetails.Height)]),
                compartmentDetails.Error);
        }

        [TestMethod]
        [DataRow(0.0, null, false, 1, false, DisplayName = "Item with no max capacity (invalid)")]
        [DataRow(null, 1.0, false, 1, false, DisplayName = "Item with no stock (invalid)")]
        [DataRow(3.0, 1.0, false, 1, false, DisplayName = "Item with max capacity < stock (invalid)")]
        [DataRow(0.0, 1.0, false, 1, false, DisplayName = "Item with stock = 0 and pairing = free (invalid)")]
        [DataRow(0.0, 1.0, true, 1, true, DisplayName = "Item with stock = 0 and pairing = fixed (valid)")]
        [DataRow(1.0, 1.0, false, 1, true, DisplayName = "Item with stock = max capacity (valid)")]
        [DataRow(1.0, 2.0, false, 1, true, DisplayName = "Item with stock < max capacity (valid)")]
        public void StockValidation(
            double? stock,
            double? maxCapacity,
            bool isPairingFixed,
            int? itemId,
            bool isValid)
        {
            var compartmentDetails = new CompartmentDetails
            {
                IsValidationEnabled = true,
                ItemId = itemId,
                XPosition = 0,
                YPosition = 0,
                Width = 1,
                Height = 1,
                LoadingUnitId = 1,
                Stock = stock,
                IsItemPairingFixed = isPairingFixed,
                MaxCapacity = maxCapacity
            };

            Assert.AreEqual(
                isValid,
                string.IsNullOrEmpty(compartmentDetails.Error),
                compartmentDetails.Error);
        }

        #endregion
    }
}
