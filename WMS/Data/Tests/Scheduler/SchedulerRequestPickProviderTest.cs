using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public class SchedulerRequestPickProviderTest : BaseWarehouseTest
    {
        #region Methods

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN that a null request is created \
               WHEN the FullyQualifyPickRequestAsync method is called with the null new request\
               THEN the method should throw an exception")]
        public async Task FullyQualifyPickRequestWithNullArgumentTest()
        {
            #region Arrange

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();
            ItemOptions options = null;

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act + Assert

                await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(
                    () => schedulerRequestPickProvider.FullyQualifyPickRequestAsync(0, options));

                #endregion
            }
        }

        #endregion
    }
}
