using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public class ListProviderTest : BaseWarehouseTest
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
                AND a set of compartments that can satisfy the list \
               WHEN the new list is requested for execution \
               THEN a new set of requests is generated
                AND the total amount of items for each row is covered by the requests
                AND the requests are transformed in missions, ordered by row priority")]
        public async Task ListExecutionRequest()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();

            var listId = 1;

            var bay2 = new Common.DataModels.Bay
            {
                Id = 2,
                AreaId = this.Area1.Id,
                LoadingUnitsBufferSize = 10
            };

            var rowLowPriority = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 3
            };

            var rowHighPriority = new Common.DataModels.ItemListRow
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 1,
            };

            var rowMediumPriority = new Common.DataModels.ItemListRow
            {
                Id = 3,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 2
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListRows = new[] { rowLowPriority, rowHighPriority, rowMediumPriority }
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
                context.ItemListRows.Add(rowLowPriority);
                context.ItemListRows.Add(rowHighPriority);
                context.ItemListRows.Add(rowMediumPriority);
                context.ItemLists.Add(list1);
                context.Bays.Add(bay2);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var result = await schedulerService.ExecuteListAsync(list1.Id, bay2.AreaId, bay2.Id);
            var requests = result.Entity;

            #endregion

            #region Assert

            var missionProvider = this.GetService<IMissionSchedulerProvider>();
            var listProvider = this.GetService<IItemListSchedulerProvider>();

            var updatedList = await listProvider.GetByIdAsync(list1.Id);
            var missions = await missionProvider.GetAllAsync();

            Assert.AreEqual(
                ListStatus.Executing,
                updatedList.Status,
                "The list should be in the Executing state.");

            Assert.AreEqual(
                3,
                requests.Count(),
                "Number of scheduler requests should match the number of list rows.");

            Assert.AreEqual(
                3,
                requests.Count(),
                "Number of scheduler requests should match the number of list rows.");

            Assert.IsTrue(
                requests.All(r => r.BayId == bay2.Id),
                "All requests should address the same bay.");

            Assert.AreEqual(
                list1.ItemListRows.Sum(r => r.RequestedQuantity),
                requests.Sum(r => r.RequestedQuantity),
                "The total quantity recorded in the requests should be the same as the quantity reported in the list rows.");

            Assert.AreEqual(
                3,
                missions.Count(),
                "A total of three missions should be generated.");

            Assert.AreEqual(
                listId,
                missions.First().ItemListId,
                "The first generated mission should refer to the list with highest priority.");

            Assert.AreEqual(
                rowHighPriority.Id,
                missions.First().ItemListRowId,
                "The first generated mission should refer to the row with highest priority.");

            Assert.AreEqual(
                rowLowPriority.Id,
                missions.Last().ItemListRowId,
                "The last generated mission should refer to the row with lowest priority.");

            #endregion
        }

        #endregion
    }
}
