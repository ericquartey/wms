using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public class SchedulerRequestProviderTest : BaseWarehouseTest
    {
        #region Methods

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN that a non-withdrawal request is created \
               WHEN the FullyQualifyWithdrawalRequestAsync method is called with the invalid request\
               THEN the method should throw an exception")]
        public async Task FullyQualifyWithdrawalRequestWithInvalidArgumentTest()
        {
            #region Arrange

            var request = new SchedulerRequest
            {
                Type = OperationType.Insertion
            };

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act + Assert

                this.ServiceProvider.GetService
                var provider = new SchedulerRequestProvider(context);

                await Assert.ThrowsExceptionAsync<System.ArgumentException>(() => provider.FullyQualifyWithdrawalRequestAsync(request));

                #endregion
            }
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN that a null request is created \
               WHEN the FullyQualifyWithdrawalRequestAsync method is called with the null new request\
               THEN the method should throw an exception")]
        public async Task FullyQualifyWithdrawalRequestWithNullArgumentTest()
        {
            #region Arrange

            SchedulerRequest request = null;

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act + Assert

                var provider = new SchedulerRequestProvider(context);

                await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(() => provider.FullyQualifyWithdrawalRequestAsync(request));

                #endregion
            }
        }

        #endregion
    }
}
