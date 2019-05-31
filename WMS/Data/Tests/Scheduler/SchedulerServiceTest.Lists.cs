using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests.Scheduler
{
    public partial class SchedulerServiceTest
    {
        #region Methods

        [TestMethod]
        [TestProperty(
           "Description",
          @"GIVEN a list with one row in the executing state and the other one in the waiting state \
            WHEN  the the mission corresponding to the executing row is completed \
            THEN  the list is in the Executing state \
            AND   the corresponding list row is in the Completed state \
            AND   the corresponding mission is in the Completed state")]
        public async Task CompleteItemMissionAsync_ExecutingState()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();
            var listExecutionProvider = this.GetService<IItemListExecutionProvider>();
            var rowExecutionProvider = this.GetService<IItemListRowExecutionProvider>();
            var missionExecutionProvider = this.GetService<IMissionExecutionProvider>();

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
                ItemListType = Common.DataModels.ItemListType.Pick,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
            };

            var compartmentType = new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 1 };

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType.Id,
                ItemId = this.ItemFifo.Id,
                MaxCapacity = 100
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = itemCompartmentType.ItemId,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100,
                CompartmentTypeId = compartmentType.Id
            };

            using (var context = this.CreateContext())
            {
                context.CompartmentTypes.Add(compartmentType);
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(row1);
                context.ItemListRows.Add(row2);
                context.ItemLists.Add(list1);

                context.SaveChanges();
            }

            var listExecutionResult = await schedulerService.ExecuteListAsync(
                list1.Id,
                this.Bay1Aisle1.AreaId,
                this.Bay1Aisle1.Id);

            if (!listExecutionResult.Success)
            {
                Assert.Inconclusive(listExecutionResult.Description);
            }

            var missions = await missionExecutionProvider.GetAllAsync();
            var row1Mission = missions.First(m => m.ItemListRowId == row1.Id);
            var missionExecutionResult = await schedulerService.ExecuteMissionAsync(row1Mission.Id);

            if (!missionExecutionResult.Success)
            {
                Assert.Inconclusive(missionExecutionResult.Description);
            }

            #endregion

            #region Act

            var result = await schedulerService.CompleteItemMissionAsync(row1Mission.Id, row1Mission.RequestedQuantity);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            var updatedMission = result.Entity;
            var updatedList = await listExecutionProvider.GetByIdAsync(list1.Id);
            var updatedRow1 = await rowExecutionProvider.GetByIdAsync(row1.Id);

            Assert.AreEqual(
                 ItemListStatus.Executing,
                 updatedList.Status,
                 "The list should be in the Executing state.");

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

        [TestMethod]
        [TestProperty(
            "Description",
           @"GIVEN  a new pick list with 2 rows \
                AND a compartment that can satisfy the list \
                AND a bay that can accept two new missions \
               WHEN the new list is requested for execution \
               THEN the number of scheduler requests matches the number of list rows \
                AND the number of missions matches the number of list rows \
                AND the total amount of items for each row is covered by the requests \
                AND the list is in the Waiting state \
                AND the list row is in the Ready state \
                AND the mission is in the New state")]
        public async Task ExecuteListAsync_PickNewState()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();

            var missionExecutionProvider = this.GetService<IMissionExecutionProvider>();

            var listExecutionProvider = this.GetService<IItemListExecutionProvider>();

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
                ItemListType = Common.DataModels.ItemListType.Pick,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
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

            var requestedBay = this.Bay1Aisle1.Id;
            var result = await schedulerService.ExecuteListAsync(list1.Id, this.Bay1Aisle1.AreaId, requestedBay);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            var requests = result.Entity;

            var updatedList = await listExecutionProvider.GetByIdAsync(list1.Id);
            var missions = await missionExecutionProvider.GetAllAsync();

            Assert.AreEqual(
                ItemListStatus.Ready,
                updatedList.Status,
                "The list should be in the Ready state.");

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
                "The number of missions should match the number of list rows.");

            Assert.IsTrue(
                requests.All(r => r.BayId == this.Bay1Aisle1.Id),
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
        @"GIVEN a new list with prioritized rows \
             AND   a compartment that can satisfy the list \
             WHEN  the new list is requested for execution \
             THEN  a new set of requests is generated
             AND   the generated missions have as priority the sum of the row's priority and of the bay")]
        public async Task ExecuteListAsync_PickWithAndWithoutPriority()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();

            var missionExecutionProvider = this.GetService<IMissionExecutionProvider>();

            var listId = 1;

            var otherBay = new Common.DataModels.Bay
            {
                Id = 1000,
                AreaId = this.Area1.Id,
                LoadingUnitsBufferSize = 10,
                Priority = 1,
                MachineId = 1,
            };

            var row1WithPriority = new Common.DataModels.ItemListRow
            {
                Id = 2,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 20,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 2,
            };

            var row2WithoutPriority = new Common.DataModels.ItemListRow
            {
                Id = 3,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = null
            };

            var row3WithoutPriority = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = null
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListType = Common.DataModels.ItemListType.Pick,
                ItemListRows = new[]
                {
                    row2WithoutPriority,
                    row1WithPriority,
                    row3WithoutPriority
                }
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(row1WithPriority);
                context.ItemListRows.Add(row2WithoutPriority);
                context.ItemListRows.Add(row3WithoutPriority);
                context.ItemLists.Add(list1);
                context.Bays.Add(otherBay);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var requestsResult = await schedulerService.ExecuteListAsync(list1.Id, otherBay.AreaId, otherBay.Id);

            #endregion

            #region Assert

            Assert.IsTrue(requestsResult.Success, requestsResult.Description);

            var missions = await missionExecutionProvider.GetAllAsync();
            var updatedBayPriority = this.CreateContext().Bays.Single(b => b.Id == otherBay.Id).Priority;

            var expectedPriority = otherBay.Priority + row1WithPriority.Priority + 1;

            Assert.AreEqual(
                expectedPriority,
                updatedBayPriority,
                "The priority of the bay is as much as the priority of the only row with priority, plus one to cater for the rows without priority.");

            Assert.AreEqual(
                list1.ItemListRows.Count(),
                requestsResult.Entity.Count(),
                "Rows's Count is not equals of generated Scheduler Request.");

            Assert.AreEqual(
                list1.ItemListRows.Count(),
                missions.Count(),
                "Mission's Count is not equals of generated Scheduler Request.");

            Assert.AreEqual(
                expectedPriority,
                missions.SingleOrDefault(m => m.ItemListRowId == row2WithoutPriority.Id)?.Priority,
                "The generated mission related to the rows 2 without priority should be equal to the priority of the last row with priority + 1.");

            Assert.AreEqual(
                expectedPriority,
                missions.SingleOrDefault(m => m.ItemListRowId == row3WithoutPriority.Id)?.Priority,
                 "The generated mission related to the rows 3 without priority should be equal to the priority of the last row with priority + 1.");

            #endregion
        }

        [TestMethod]
        [TestProperty(
          "Description",
         @"GIVEN a new list with prioritized rows \
             AND   a compartment that can satisfy the list \
             WHEN  the new list is requested for execution \
             THEN  a new set of requests is generated
             AND   the generated missions have as priority the sum of the row's priority and of the bay")]
        public async Task ExecuteListAsync_PickWithPriority()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();

            var missionExecutionProvider = this.GetService<IMissionExecutionProvider>();

            var listId = 1;

            var otherBay = new Common.DataModels.Bay
            {
                Id = 1000,
                AreaId = this.Area1.Id,
                LoadingUnitsBufferSize = 10,
                Priority = 1,
                MachineId = 1,
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
                ItemListType = Common.DataModels.ItemListType.Pick,
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
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(rowLowPriority);
                context.ItemListRows.Add(rowHighPriority);
                context.ItemListRows.Add(rowMediumPriority);
                context.ItemLists.Add(list1);
                context.Bays.Add(otherBay);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var requestsResult = await schedulerService.ExecuteListAsync(list1.Id, otherBay.AreaId, otherBay.Id);

            #endregion

            #region Assert

            Assert.IsTrue(requestsResult.Success, requestsResult.Description);

            var missions = await missionExecutionProvider.GetAllAsync();
            var updatedBayPriority = this.CreateContext().Bays.Single(b => b.Id == otherBay.Id).Priority;

            var expectedPriority = otherBay.Priority +
                System.Math.Max(
                    System.Math.Max(
                        rowLowPriority.Priority.Value,
                        rowHighPriority.Priority.Value),
                    rowMediumPriority.Priority.Value);

            Assert.AreEqual(
                expectedPriority,
                updatedBayPriority,
                "The priority of the bay is as much as the highest priority value.");

            Assert.AreEqual(
                list1.ItemListRows.Count(),
                requestsResult.Entity.Count(),
                "Rows's Count is not equals of generated Scheduler Request.");

            Assert.AreEqual(
                list1.ItemListRows.Count(),
                missions.Count(),
                "Missions's Count is not equals of generated Scheduler Request.");

            Assert.AreEqual(
                otherBay.Priority + rowHighPriority.Priority,
                missions.SingleOrDefault(m => m.ItemListRowId == rowHighPriority.Id)?.Priority,
                "The generated mission related to the high priority row should have as priority the sum of the row's priority and of the bay.");

            Assert.AreEqual(
                otherBay.Priority + rowLowPriority.Priority,
                missions.SingleOrDefault(m => m.ItemListRowId == rowLowPriority.Id)?.Priority,
                "The generated mission related to the high priority row should have as priority the sum of the row's priority and of the bay.");

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
           @"GIVEN  a new put list with 2 rows \
                AND a compartment that can satisfy the list \
                AND a bay that can accept two new missions \
               WHEN the new list is requested for execution \
               THEN the number of scheduler requests matches the number of list rows \
                AND the number of missions matches the number of list rows \
                AND the total amount of items for each row is covered by the requests \
                AND the list is in the Waiting state \
                AND the list row is in the Ready state \
                AND the mission is in the New state")]
        public async Task ExecuteListAsync_PutNewState()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();

            var missionProvider = this.GetService<IMissionExecutionProvider>();

            var listProvider = this.GetService<IItemListExecutionProvider>();

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
                ItemListType = Common.DataModels.ItemListType.Put,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
            };

            var compartmentType = new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 1 };

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType.Id,
                ItemId = this.ItemFifo.Id,
                MaxCapacity = 100,
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 20,
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType.Id,
                Stock = 50
            };

            using (var context = this.CreateContext())
            {
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.CompartmentTypes.Add(compartmentType);
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(row1);
                context.ItemListRows.Add(row2);
                context.ItemLists.Add(list1);

                context.SaveChanges();
            }

            var requestedBay = this.Bay1Aisle1.Id;

            #endregion

            #region Act

            var result = await schedulerService.ExecuteListAsync(list1.Id, this.Bay1Aisle1.AreaId, requestedBay);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            var updatedList = await listProvider.GetByIdAsync(list1.Id);

            Assert.IsNotNull(updatedList);

            Assert.AreEqual(
                ItemListStatus.Ready,
                updatedList.Status,
                "The list should be in the Ready state.");

            using (var context = this.CreateContext())
            {
                var updatedCompartment = context.Compartments.SingleOrDefault(c => c.Id == compartment1.Id);

                Assert.IsNotNull(updatedCompartment);

                Assert.AreEqual(
                    list1.ItemListRows.Sum(r => r.RequestedQuantity),
                    updatedCompartment.ReservedToPut);
            }

            var requests = result.Entity;

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

            Assert.IsTrue(
               requests.All(r => r.BayId == this.Bay1Aisle1.Id),
               "All requests should address the same bay.");

            Assert.AreEqual(
                list1.ItemListRows.Sum(r => r.RequestedQuantity),
                requests.Sum(r => r.RequestedQuantity),
                "The total quantity recorded in the requests should be the same as the quantity reported in the list rows.");

            var missions = await missionProvider.GetAllAsync();

            Assert.AreEqual(
                list1.ItemListRows.Count(),
                missions.Count(),
                "The number of missions should match the number of list rows.");

            #endregion
        }

        [TestMethod]
        [TestProperty(
           "Description",
          @"GIVEN a list with 1 row in the Suspended state \
               WHEN the list is executed on the bay \
                THEN the execute operation should be permitted")]
        public async Task ExecuteListAsync_SuspendedState()
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
                Status = Common.DataModels.ItemListRowStatus.Suspended,
                Priority = 1,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListRows = new[]
                {
                    row1,
                }
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(row1);
                context.ItemLists.Add(list1);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var result = await schedulerService.ExecuteListAsync(listId, this.Bay1Aisle1.AreaId, this.Bay1Aisle1.Id);

            #endregion

            #region Assert

            // TODO: review this test when the Suspend functionality is implemented
            Assert.IsFalse(result.Success, result.Description);

            #endregion
        }

        [TestMethod]
        [TestProperty(
           "Description",
          @"GIVEN a list with 1 row in the Waiting state \
               WHEN the list is executed but no bay is indicated \
                THEN the execute operation should not be permitted")]
        public async Task ExecuteListAsync_WaitingListWithoutBay()
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
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 1,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListRows = new[]
                {
                    row1,
                }
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(row1);
                context.ItemLists.Add(list1);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var result = await schedulerService.ExecuteListAsync(listId, this.Bay1Aisle1.AreaId, null);

            #endregion

            #region Assert

            var success = result.Success;

            Assert.IsFalse(
                success,
                "The execute operation should not be permitted.");

            #endregion
        }

        [TestMethod]
        [TestProperty(
          "Description",
         @"GIVEN a new list pick with prioritized rows \
             AND a compartment that can satisfy the list \
            WHEN a single row is executed \
            THEN the row is in the Waiting state  \
             AND the list is in the Waiting state  \
             AND the priority of the bay is incremented by the row priority")]
        public async Task ExecuteListRowAsync_PickWithPriority()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();

            var missionExecutionProvider = this.GetService<IMissionExecutionProvider>();

            var listId = 1;

            var itemId = this.ItemFifo.Id;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = itemId,
                RequestedQuantity = 1,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 32,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListType = Common.DataModels.ItemListType.Pick,
                ItemListRows = new[] { row1 }
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = itemId,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(row1);
                context.ItemLists.Add(list1);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var result = await schedulerService.ExecuteListRowAsync(row1.Id, this.Bay1Aisle1.AreaId, this.Bay1Aisle1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            var updatedBayPriority = this.CreateContext().Bays.Single(b => b.Id == this.Bay1Aisle1.Id).Priority;

            Assert.AreEqual(
                this.Bay1Aisle1.Priority + row1.Priority,
                updatedBayPriority,
                "The priority of the bay should be incremented by the row priority");

            var missions = await missionExecutionProvider.GetByListRowIdAsync(row1.Id);

            Assert.AreEqual(1, missions.Count(), "Number of generated Mission is not equal to 1.");

            Assert.AreEqual(
                this.Bay1Aisle1.Priority + row1.Priority,
                missions.First().Priority,
                "The generated mission related to the row should have as priority the sum of the row's priority and of the bay.");

            #endregion
        }

        [TestMethod]
        [TestProperty(
           "Description",
          @"GIVEN   a pick list with 2 rows in the waiting state \
               WHEN the one of the missions corresponding to one of the rows is executed on the bay \
                THEN the list is in the Executing state \
                AND the corresponding list rows are in the Executing state \
                AND the corresponding missions are in the Executing state")]
        public async Task ExecuteMissionAsync_PickWaitingListState()
        {
            #region Arrange

            var schedulerService = this.GetService<ISchedulerService>();
            var listExecutionProvider = this.GetService<IItemListExecutionProvider>();
            var rowExecutionProvider = this.GetService<IItemListRowExecutionProvider>();
            var missionExecutionProvider = this.GetService<IMissionExecutionProvider>();

            var listId = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1,
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 1,
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
                ItemListType = Common.DataModels.ItemListType.Pick,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
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

            var listExecutionResult = await schedulerService.ExecuteListAsync(list1.Id, this.Bay1Aisle1.AreaId, this.Bay1Aisle1.Id);
            if (!listExecutionResult.Success)
            {
                Assert.Inconclusive(listExecutionResult.Description);
            }

            var missions = await missionExecutionProvider.GetAllAsync();
            if (!missions.Any())
            {
                Assert.Inconclusive(listExecutionResult.Description);
            }

            var row1Mission = missions.FirstOrDefault(m => m.ItemListRowId == row1.Id);
            if (row1Mission == null)
            {
                Assert.Inconclusive(listExecutionResult.Description, "Row mission can't be null");
            }

            #endregion

            #region Act

            var result = await schedulerService.ExecuteMissionAsync(row1Mission.Id);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            var updatedMission = result.Entity;
            var updatedList = await listExecutionProvider.GetByIdAsync(list1.Id);
            var updatedRow1 = await rowExecutionProvider.GetByIdAsync(row1.Id);

            Assert.AreEqual(
                ItemListStatus.Executing,
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
