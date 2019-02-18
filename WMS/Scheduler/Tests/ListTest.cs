using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public class ListTest : BaseWarehouseTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        [TestProperty(
            "Description",
           @"GIVEN a list with prioritized rows \
                AND some compartments that can satisfy the list \
               WHEN the new list is requested for execution \
               THEN a new set of requests is generated
                AND the total amount of items for each row is covered by the requests
                AND the requests are transformed in missions, ordered by row priority")]
        public async Task ListExecutionRequest()
        {
            #region Arrange

            var listId = 1;

            var bay2 = new Common.DataModels.Bay
            {
                Id = 2,
                AreaId = this.Area1.Id,
                LoadingUnitsBufferSize = 10
            };

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                RequiredQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 3
            };

            var row2 = new Common.DataModels.ItemListRow
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                RequiredQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 1,
            };

            var row3 = new Common.DataModels.ItemListRow
            {
                Id = 3,
                ItemId = this.ItemFifo.Id,
                RequiredQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 2
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListRows = new[] { row1, row2, row3 },
                Status = Common.DataModels.ItemListStatus.Waiting
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(row1);
                context.ItemListRows.Add(row2);
                context.ItemListRows.Add(row3);
                context.ItemLists.Add(list1);
                context.Bays.Add(bay2);

                context.SaveChanges();
            }

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                var warehouse = new Warehouse(
                    new DataProvider(context),
                    new SchedulerRequestProvider(context),
                    new Mock<ILogger<Warehouse>>().Object);

                var requests = await warehouse.PrepareListForExecutionAsync(list1.Id, bay2.AreaId, bay2.Id);

                #endregion

                #region Assert

                Assert.AreEqual(
                    3,
                    requests.Count(),
                    "Number of scheduler requests should match the number of list rows.");

                Assert.IsTrue(
                    requests.All(r => r.BayId == bay2.Id),
                    "All requests should address the same bay.");

                Assert.AreEqual(
                    list1.ItemListRows.Sum(r => r.RequiredQuantity),
                    requests.Sum(r => r.RequestedQuantity),
                    "The total quantity recorded in the requests should be the same as the quantity reported in the list rows.");

                Assert.AreEqual(
                    3,
                    context.Missions.Count(),
                    "A total of three missions should be generated.");

                Assert.AreEqual(
                    listId,
                    context.Missions.First().ItemListId,
                    "The first generated mission should refer to the list with highest priority.");

                Assert.AreEqual(
                    row2.Id,
                    context.Missions.First().ItemListRowId,
                    "The first generated mission should refer to the row with highest priority.");

                Assert.AreEqual(
                    row1.Id,
                    context.Missions.Last().ItemListRowId,
                    "The last generated mission should refer to the row with lowest priority.");

                #endregion
            }
        }

        #endregion
    }
}
