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

        [TestMethod]
        [TestProperty(
           "Description",
          @"GIVEN a list with one row in the executing state and the other one in the waiting state \
            WHEN  the the mission corresponding to the executing row is completed \
            THEN  the list is in the Waiting state \
            AND   the corresponding list row is in the Completed state \
            AND   the corresponding mission is in the Completed state")]
        public async Task ExecutingListCompletionRequest()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();
            var listProvider = this.GetService<IItemListSchedulerProvider>();
            var rowProvider = this.GetService<IItemListRowSchedulerProvider>();
            var missionProvider = this.GetService<IMissionSchedulerProvider>();

            var listId = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
            };

            var row2 = new Common.DataModels.ItemListRow
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
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
                context.ItemLists.Add(list1);

                context.SaveChanges();
            }

            await schedulerService.ExecuteListAsync(list1.Id, this.Bay1.AreaId, this.Bay1.Id);
            var missions = await missionProvider.GetAllAsync();
            var row1Mission = missions.First(m => m.ItemListRowId == row1.Id);
            await schedulerService.ExecuteMissionAsync(row1Mission.Id);

            #endregion

            #region Act

            var result = await schedulerService.CompleteMissionAsync(row1Mission.Id, row1Mission.RequestedQuantity);

            #endregion

            #region Assert

            var updatedMission = result.Entity;
            var updatedList = await listProvider.GetByIdAsync(list1.Id);
            var updatedRow1 = await rowProvider.GetByIdAsync(row1.Id);

            Assert.AreEqual(
                ListStatus.Waiting,
                updatedList.Status,
                "The list should be in the Waiting state.");

            Assert.AreEqual(
                ItemListRowStatus.Completed,
                updatedRow1.Status,
                "The list row should be in the Completed state.");

            Assert.AreEqual(
                MissionStatus.Completed,
                updatedMission.Status,
                "The mission should be in the Completed state.");

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        [TestProperty(
          "Description",
         @"GIVEN a new list with prioritized rows \
             AND   a compartment that can satisfy the list \
             WHEN  the new list is requested for execution \
             THEN  a new set of requests is generated
             AND   the generated missions have as priority the sum of the row's priority and of the bay")]
        public async Task ListExecutionPriority()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();

            var listId = 1;

            var bay2 = new Common.DataModels.Bay
            {
                Id = 2,
                AreaId = this.Area1.Id,
                LoadingUnitsBufferSize = 10,
                Priority = 1
            };

            var rowHighPriority = new Common.DataModels.ItemListRow
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 20,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 2,
            };

            var rowMediumPriority = new Common.DataModels.ItemListRow
            {
                Id = 3,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 3
            };

            var rowLowPriority = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 4
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListRows = new[]
                {
                    rowLowPriority,
                    rowHighPriority,
                    rowMediumPriority
                }
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

            await schedulerService.ExecuteListAsync(list1.Id, bay2.AreaId, bay2.Id);

            #endregion

            #region Assert

            var missionProvider = this.GetService<IMissionSchedulerProvider>();
            var missions = await missionProvider.GetAllAsync();
            var updatedBayPriority = this.CreateContext().Bays.Single(b => b.Id == bay2.Id).Priority;

            var expectedPriority = bay2.Priority + 1 +
                System.Math.Max(
                    System.Math.Max(
                        rowLowPriority.Priority.Value,
                        rowHighPriority.Priority.Value),
                    rowMediumPriority.Priority.Value);

            Assert.AreEqual(
                expectedPriority,
                updatedBayPriority,
                "The priority of the bay is increased of one unit with respect to the highest priority value.");

            Assert.AreEqual(
                bay2.Priority + rowHighPriority.Priority,
                missions.SingleOrDefault(m => m.ItemListRowId == rowHighPriority.Id)?.Priority,
                "The generated mission related to the high priority row should have as priority the sum of the row's priority and of the bay.");

            Assert.AreEqual(
                bay2.Priority + rowLowPriority.Priority,
                missions.SingleOrDefault(m => m.ItemListRowId == rowLowPriority.Id)?.Priority,
                "The generated mission related to the high priority row should have as priority the sum of the row's priority and of the bay.");

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
           @"GIVEN  a new list with 2 rows \
                AND a compartment that can satisfy the list \
                AND a bay that can accept two new missions \
               WHEN the new list is requested for execution \
                AND the number of scheduler requests matches the number of list rows \
                AND the number of missions matches the number of list rows \
                AND the total amount of items for each row is covered by the requests \
                AND the list is in the Waiting state \
                AND the list row is in the Waiting state \
                AND the mission is in the Waiting state")]
        public async Task NewListExecutionRequest()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();

            var listId = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
            };

            var row2 = new Common.DataModels.ItemListRow
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
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
                context.ItemLists.Add(list1);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var requestedBay = this.Bay1.Id;
            var result = await schedulerService.ExecuteListAsync(list1.Id, this.Bay1.AreaId, requestedBay);

            #endregion

            #region Assert

            var requests = result.Entity;
            var missionProvider = this.GetService<IMissionSchedulerProvider>();
            var listProvider = this.GetService<IItemListSchedulerProvider>();

            var updatedList = await listProvider.GetByIdAsync(list1.Id);
            var missions = await missionProvider.GetAllAsync();

            Assert.AreEqual(
                ListStatus.Waiting,
                updatedList.Status,
                "The list should be in the Waiting state.");

            Assert.IsTrue(
                requests.All(r => r.BayId == requestedBay),
                "All requests should address the same bay.");

            Assert.AreEqual(
                list1.ItemListRows.Sum(r => r.RequestedQuantity),
                requests.Sum(r => r.RequestedQuantity),
                "The total quantity recorded in the requests should be the same as the quantity reported in the list rows.");

            Assert.AreEqual(
                list1.ItemListRows.Count(),
                requests.Count(),
                "Number of scheduler requests should match the number of list rows.");

            Assert.AreEqual(
                 list1.ItemListRows.Count(),
                missions.Count(),
                "The number of missions shouls match the number of list rows.");

            Assert.IsTrue(
                requests.All(r => r.BayId == this.Bay1.Id),
                "All requests should address the same bay.");

            Assert.AreEqual(
                list1.ItemListRows.Sum(r => r.RequestedQuantity),
                requests.Sum(r => r.RequestedQuantity),
                "The total quantity recorded in the requests should be the same as the quantity reported in the list rows.");

            #endregion
        }

        [TestMethod]
        [TestProperty(
           "Description",
          @"GIVEN   a list with 2 rows in the waiting state \
               WHEN the one of the missions corresponding to one of the rows is executed on the bay \
                THEN the list is in the Executing state \
                AND the corresponding list rows are in the Executing state \
                AND the corresponding missions are in the Executing state")]
        public async Task WaitingListExecutionRequest()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();
            var listProvider = this.GetService<IItemListSchedulerProvider>();
            var rowProvider = this.GetService<IItemListRowSchedulerProvider>();
            var missionProvider = this.GetService<IMissionSchedulerProvider>();

            var listId = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
            };

            var row2 = new Common.DataModels.ItemListRow
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
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
                context.ItemLists.Add(list1);

                context.SaveChanges();
            }

            await schedulerService.ExecuteListAsync(list1.Id, this.Bay1.AreaId, this.Bay1.Id);
            var missions = await missionProvider.GetAllAsync();
            var row1Mission = missions.First(m => m.ItemListRowId == row1.Id);

            #endregion

            #region Act

            var result = await schedulerService.ExecuteMissionAsync(row1Mission.Id);

            #endregion

            #region Assert

            var updatedMission = result.Entity;
            var updatedList = await listProvider.GetByIdAsync(list1.Id);
            var updatedRow1 = await rowProvider.GetByIdAsync(row1.Id);

            Assert.AreEqual(
                ListStatus.Executing,
                updatedList.Status,
                "The list should be in the Executing state.");

            Assert.AreEqual(
                ItemListRowStatus.Executing,
                updatedRow1.Status,
                "The list row should be in the Executing state.");

            Assert.AreEqual(
                MissionStatus.Executing,
                updatedMission.Status,
                "The mission should be in the Executing state.");

            #endregion
        }

        #endregion
    }
}
