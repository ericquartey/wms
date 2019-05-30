using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests.Scheduler
{
    [TestClass]
    public partial class SchedulerRequestPickProviderTest : BaseWarehouseTest
    {
        #region Methods

        [TestMethod]
        public async Task FullyQualifyPickRequestAsync_Args_InvalidItemId()
        {
            #region Arrange

            var requestProvider = this.GetService<ISchedulerRequestPickProvider>();

            var invalidItemId = 666;

            #endregion

            #region Act + Assert

            var result = await requestProvider.FullyQualifyPickRequestAsync(
                invalidItemId,
                new ItemOptions { RequestedQuantity = 1 });

            #endregion

            #region Assert

            Assert.IsFalse(result.Success);

            #endregion
        }

        [TestMethod]
        public async Task FullyQualifyPickRequestAsync_Args_NegativeRequestedQuantity()
        {
            #region Arrange

            var requestProvider = this.GetService<ISchedulerRequestPickProvider>();

            #endregion

            #region Act + Assert

            var result = await requestProvider.FullyQualifyPickRequestAsync(
                this.Item1.Id,
                new ItemOptions { RequestedQuantity = -1 });

            #endregion

            #region Assert

            Assert.IsFalse(result.Success);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN that a null request is created \
               WHEN the FullyQualifyPickRequestAsync method is called with the null new request\
               THEN the method should throw an exception")]
        public async Task FullyQualifyPickRequestAsync_Args_NullItemOptions()
        {
            #region Arrange

            var requestProvider = this.GetService<ISchedulerRequestPickProvider>();

            #endregion

            #region Act + Assert

            await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(
                () => requestProvider.FullyQualifyPickRequestAsync(
                        this.Item1.Id,
                        itemPickOptions: null));

            #endregion
        }

        [TestMethod]
        public async Task FullyQualifyPickRequestAsync_ZeroRequestedQuantity()
        {
            #region Arrange

            var requestProvider = this.GetService<ISchedulerRequestPickProvider>();

            #endregion

            #region Act + Assert

            var result = await requestProvider.FullyQualifyPickRequestAsync(
                this.Item1.Id,
                new ItemOptions { RequestedQuantity = 0 });

            #endregion

            #region Assert

            Assert.IsFalse(result.Success);

            #endregion
        }

        #endregion
    }
}
