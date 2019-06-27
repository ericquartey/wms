using Ferretto.WMS.App.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.App.Tests
{
    [TestClass]
    public class BulkCompartmentTest
    {
        #region Methods

        [TestMethod]
        [DataRow(5, 30, 30, 6, 6, 30, 30)]
        [DataRow(5, 30, 30, 5, 5, 25, 25)]
        [DataRow(5, 30, 30, 4, 4, 20, 20)]
        [DataRow(5, 30, 30, 8, 8, 0, 0)]
        public void ApplyCorrectionOnSingleCompartmentTest(double minStep, double width, double height, int row, int column, int widthCorrected, int heightCorrected)
        {
            var bulkCompartment = new BulkCompartment
            {
                Width = width,
                Depth = height,
                Rows = row,
                Columns = column,
            };
            bulkCompartment.ApplyCorrectionOnSingleCompartment(minStep);

            Assert.AreEqual(bulkCompartment.Width, widthCorrected);
            Assert.AreEqual(bulkCompartment.Depth, heightCorrected);
        }

        #endregion
    }
}
