using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
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

            var provider = this.ServiceProvider
                .GetService(typeof(ISchedulerRequestProvider)) as ISchedulerRequestProvider;

            var request = new SchedulerRequest
            {
                Type = OperationType.Insertion
            };

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act + Assert

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

            var provider = this.ServiceProvider.GetService(typeof(ISchedulerRequestProvider)) as ISchedulerRequestProvider;
            SchedulerRequest request = null;

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act + Assert

                await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(() => provider.FullyQualifyWithdrawalRequestAsync(request));

                #endregion
            }
        }

        #endregion
    }
}
