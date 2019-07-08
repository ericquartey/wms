using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
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

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var missionSchedulerService = this.GetService<IMissionSchedulerService>();

            var listExecutionProvider = this.GetService<IItemListExecutionProvider>();

            var rowExecutionProvider = this.GetService<IItemListRowExecutionProvider>();

            var missionProvider = this.GetService<IMissionProvider>();

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "Row1",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
            };

            var row2 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "Row2",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = GetNewId(),
                ItemListType = Common.DataModels.ItemListType.Pick,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
            };

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = this.CompartmentType.Id,
                ItemId = this.ItemFifo.Id,
                MaxCapacity = 100,
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = itemCompartmentType.ItemId,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = this.CompartmentType.Id,
                Stock = 100,
            };

            using (var context = this.CreateContext())
            {
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

            var operationId = 0;
            using (var context = this.CreateContext())
            {
                var operation = context.MissionOperations.SingleOrDefault(o => o.ItemListRowId == row1.Id);
                operationId = operation.Id;

                var operationExecutionResult = await missionSchedulerService.ExecuteOperationAsync(operationId);
                if (!operationExecutionResult.Success)
                {
                    Assert.Inconclusive(operationExecutionResult.Description);
                }
            }

            #endregion

            #region Act

            var result = await missionSchedulerService.CompleteOperationAsync(operationId, row1.RequestedQuantity);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            var updatedOperation = result.Entity;

            var updatedMission = (await missionProvider.GetAllAsync(0, 0))
                .Single(m => m.Id == updatedOperation.MissionId);

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
               updatedOperation.DispatchedQuantity,
               updatedRow1.DispatchedQuantity,
               "All the quantity of the row should be dispatched.");

            Assert.AreEqual(
                MissionStatus.Executing,
                updatedMission.Status,
                "The mission should be in the Executing state.");

            Assert.AreEqual(
                MissionOperationStatus.Completed,
                updatedOperation.Status,
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

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var missionProvider = this.GetService<IMissionProvider>();

            var listExecutionProvider = this.GetService<IItemListExecutionProvider>();

            var listId = GetNewId();

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
            };

            var row2 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
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
                CompartmentTypeId = this.CompartmentType.Id,
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

            var requestedBay = this.Bay1Aisle1.Id;

            var requestedArea = this.Bay1Aisle1.AreaId;

            #endregion

            #region Act

            var result = await schedulerService.ExecuteListAsync(list1.Id, requestedArea, requestedBay);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            var requests = result.Entity;

            var updatedList = await listExecutionProvider.GetByIdAsync(list1.Id);

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

            var missions = await missionProvider.GetAllAsync(0, 0);

            Assert.AreEqual(
                1,
                missions.Count(),
                "A mission should be created.");

            Assert.AreEqual(
                list1.ItemListRows.Count(),
                missions.Single().Operations.Count(),
                "The number of operations should match the number of list rows.");

            Assert.IsTrue(
                requests.All(r => r.BayId == requestedBay),
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
        public async Task ExecuteListAsync_PickWithPriority()
        {
            #region Arrange

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var listId = GetNewId();

            var bay = new Common.DataModels.Bay
            {
                Id = GetNewId(),
                AreaId = this.Area1.Id,
                LoadingUnitsBufferSize = 10,
                Priority = 10,
                MachineId = this.Machine1Aisle1.Id,
            };

            var rowHighPriority = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "high",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 20,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 2,
            };

            var rowMediumPriority = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "medium",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 3
            };

            var rowLowPriority = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "low",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 4
            };

            var list = new Common.DataModels.ItemList
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

            var compartment = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment);
                context.ItemListRows.Add(rowLowPriority);
                context.ItemListRows.Add(rowHighPriority);
                context.ItemListRows.Add(rowMediumPriority);
                context.ItemLists.Add(list);
                context.Bays.Add(bay);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var requestsResult = await schedulerService.ExecuteListAsync(list.Id, bay.AreaId, bay.Id);

            #endregion

            #region Assert

            Assert.IsTrue(requestsResult.Success, requestsResult.Description);

            using (var context = this.CreateContext())
            {
                var updatedBayPriority = context.Bays.Single(b => b.Id == bay.Id).Priority;

                var originalBayPriority = bay.Priority;

                var expectedPriority = bay.Priority +
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
                    list.ItemListRows.Count(),
                    requestsResult.Entity.Count(),
                    "Rows's count should be equal to the generated Scheduler Requests.");

                Assert.AreEqual(
                   1,
                   context.Missions.Count(),
                   "A mission should be generated");

                var missionOperations = context.Missions
                    .Include(m => m.Operations)
                    .ThenInclude(o => o.ItemListRow)
                    .Single().Operations;

                Assert.AreEqual(
                    list.ItemListRows.Count(),
                    missionOperations.Count(),
                    "Operations's Count is not equals of generated Scheduler Request.");

                var highPriorityOperation = missionOperations
                    .SingleOrDefault(o => o.ItemListRow.Code == rowHighPriority.Code);

                Assert.IsNotNull(highPriorityOperation);

                Assert.AreEqual(
                    originalBayPriority + rowHighPriority.Priority,
                    highPriorityOperation.Priority,
                    "The generated operation related to the high priority row should have as priority the sum of the row's priority and of the bay.");

                var lowPriorityOperation = missionOperations
                    .SingleOrDefault(o => o.ItemListRow.Code == rowLowPriority.Code);

                Assert.IsNotNull(lowPriorityOperation);

                Assert.AreEqual(
                    originalBayPriority + rowLowPriority.Priority,
                    lowPriorityOperation.Priority,
                    "The generated operation related to the low priority row should have as priority the sum of the row's priority and of the bay.");
            }

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

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var missionProvider = this.GetService<IMissionProvider>();

            var listProvider = this.GetService<IItemListExecutionProvider>();

            var listId = GetNewId();

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
            };

            var row2 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
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

            var compartmentType = new Common.DataModels.CompartmentType
            {
                Id = GetNewId(),
                Depth = 1,
                Width = 1
            };

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType.Id,
                ItemId = this.ItemFifo.Id,
                MaxCapacity = 100,
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType.Id,
                FifoStartDate = System.DateTime.Now.AddDays(-0.5),
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

            var missions = await missionProvider.GetAllAsync(0, 0);

            Assert.AreEqual(
                1,
                missions.Count(),
                "One mission is generated.");

            Assert.AreEqual(
               list1.ItemListRows.Count(),
               missions.Single().Operations.Count(),
               "The number of missions should match the number of list rows.");

            #endregion
        }

        [TestMethod]
        [TestProperty(
                 "Description",
                @"GIVEN a new [pick/put] list with prioritized rows \
             AND   a compartment that can satisfy the list \
             WHEN  the new list is requested for execution \
             THEN  a new set of requests is generated
             AND   the generated missions have as priority the sum of the row's priority and of the bay")]
        [DataRow(Common.DataModels.ItemListType.Pick)]
        [DataRow(Common.DataModels.ItemListType.Put)]
        public async Task ExecuteListAsync_RowsWithAndWithoutPriority(Common.DataModels.ItemListType itemListType)
        {
            #region Arrange

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var missionProvider = this.GetService<IMissionProvider>();

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = this.CompartmentType.Id,
                ItemId = this.ItemFifo.Id,
                MaxCapacity = 10000,
            };

            var otherBay = new Common.DataModels.Bay
            {
                Id = GetNewId(),
                AreaId = this.Area1.Id,
                LoadingUnitsBufferSize = 10,
                Priority = 10,
                MachineId = this.Machine1Aisle1.Id,
            };

            var row1WithPriority = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "row1",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 20,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 2,
            };

            var row2WithoutPriority = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "row2",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = null
            };

            var row3WithoutPriority = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "row3",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = null
            };

            var list = new Common.DataModels.ItemList
            {
                Id = GetNewId(),
                ItemListType = itemListType,
                ItemListRows = new[]
                {
                    row2WithoutPriority,
                    row1WithPriority,
                    row3WithoutPriority
                }
            };

            var compartment = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = this.CompartmentType.Id,
                FifoStartDate = System.DateTime.Now.AddDays(-0.5),
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment);
                context.ItemListRows.Add(row1WithPriority);
                context.ItemListRows.Add(row2WithoutPriority);
                context.ItemListRows.Add(row3WithoutPriority);
                context.ItemLists.Add(list);
                context.Bays.Add(otherBay);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var requestsResult = await schedulerService.ExecuteListAsync(list.Id, otherBay.AreaId, otherBay.Id);

            #endregion

            #region Assert

            Assert.IsTrue(requestsResult.Success, requestsResult.Description);

            var missions = await missionProvider.GetAllAsync(0, 0);
            var updatedBayPriority = this.CreateContext().Bays.Single(b => b.Id == otherBay.Id).Priority;

            var expectedPriority = otherBay.Priority + row1WithPriority.Priority + 1;

            Assert.AreEqual(
                  expectedPriority,
                  updatedBayPriority,
                  "The priority of the bay is as much as the priority of the only row with priority, plus one to cater for the rows without priority.");

            Assert.AreEqual(
                list.ItemListRows.Count(),
                requestsResult.Entity.Count(),
                "Rows's Count is not equals of generated Scheduler Request.");

            Assert.IsNotNull(missions);

            Assert.AreEqual(
                1,
                missions.Count(),
                "One mission has to be generated.");

            Assert.AreEqual(
              list.ItemListRows.Count(),
              missions.Single().Operations.Count(),
              "The generated mission has to contain an amount of operations equal to the number of list rows.");

            var operationRow2 = missions.Single().Operations.SingleOrDefault(o => o.ItemListRowCode == row2WithoutPriority.Code);
            Assert.AreEqual(
                expectedPriority,
                operationRow2?.Priority,
                "The generated mission related to the row 2 without priority should be equal to the priority of the last row with priority + 1.");

            var operationRow3 = missions.Single().Operations.SingleOrDefault(o => o.ItemListRowCode == row3WithoutPriority.Code);
            Assert.AreEqual(
                expectedPriority,
                operationRow3?.Priority,
                 "The generated mission related to the row 3 without priority should be equal to the priority of the last row with priority + 1.");

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

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var listId = GetNewId();

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
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

            await schedulerService.ExecuteListAsync(listId, this.Bay1Aisle1.AreaId, this.Bay1Aisle1.Id);

            #endregion

            #region Assert

            Assert.Inconclusive("Review this test when the Suspend functionality is implemented");

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

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var listId = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
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
         @"GIVEN a new pick list with one row \
             AND a compartment that can satisfy the list \
            WHEN the single row is executed \
            THEN the row is put the Waiting state  \
             AND the list is in the Waiting state  \
             AND the priority of the bay is incremented by the row priority")]
        public async Task ExecuteListRowAsync_PickWithPriority()
        {
            #region Arrange

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var listId = GetNewId();

            var itemId = this.ItemFifo.Id;

            var row = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                ItemId = itemId,
                RequestedQuantity = 1,
                ItemListId = listId,
                Status = Common.DataModels.ItemListRowStatus.New,
                Priority = 32,
            };

            var list = new Common.DataModels.ItemList
            {
                Id = listId,
                ItemListType = Common.DataModels.ItemListType.Pick
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = itemId,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.ItemListRows.Add(row);
                context.ItemLists.Add(list);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var result = await schedulerService.ExecuteListRowAsync(row.Id, this.Bay1Aisle1.AreaId, this.Bay1Aisle1.Id);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            var updatedBayPriority = this.CreateContext().Bays.Single(b => b.Id == this.Bay1Aisle1.Id).Priority;

            Assert.AreEqual(
                this.Bay1Aisle1.Priority + row.Priority,
                updatedBayPriority,
                "The priority of the bay should be incremented by the row priority");

            using (var context = this.CreateContext())
            {
                Assert.AreEqual(1, context.Missions.Count(), "A mission should be generated.");
                var operation = context.MissionOperations.SingleOrDefault(o => o.ItemListRowId == row.Id);

                Assert.IsNotNull(operation, "An operation for the specified row should be generated.");

                Assert.AreEqual(
                   this.Bay1Aisle1.Priority + row.Priority,
                   operation.Priority,
               "The generated mission related to the row should have as priority the sum of the row's priority and of the bay.");
            }

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

            var schedulerService = this.GetService<IItemListSchedulerService>();

            var missionSchedulerService = this.GetService<IMissionSchedulerService>();

            var listExecutionProvider = this.GetService<IItemListExecutionProvider>();

            var rowExecutionProvider = this.GetService<IItemListRowExecutionProvider>();

            var missionProvider = this.GetService<IMissionProvider>();

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "row1",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 10,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 1,
            };

            var row2 = new Common.DataModels.ItemListRow
            {
                Id = GetNewId(),
                Code = "row2",
                ItemId = this.ItemFifo.Id,
                RequestedQuantity = 30,
                Status = Common.DataModels.ItemListRowStatus.Waiting,
                Priority = 1,
            };

            var list = new Common.DataModels.ItemList
            {
                Id = GetNewId(),
                ItemListType = Common.DataModels.ItemListType.Pick,
                ItemListRows = new[]
                {
                    row1,
                    row2
                }
            };

            var compartment = new Common.DataModels.Compartment
            {
                ItemId = this.ItemFifo.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = this.CompartmentType.Id,
                Stock = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment);
                context.ItemListRows.Add(row1);
                context.ItemListRows.Add(row2);
                context.ItemLists.Add(list);

                context.SaveChanges();
            }

            var listExecutionResult = await schedulerService.ExecuteListAsync(list.Id, this.Bay1Aisle1.AreaId, this.Bay1Aisle1.Id);
            if (!listExecutionResult.Success)
            {
                Assert.Inconclusive(listExecutionResult.Description);
            }

            var missions = await missionProvider.GetAllAsync(0, 0);
            if (missions.Count() != 1)
            {
                Assert.Inconclusive("One mission should be generated.");
            }

            var row1Operation = missions.First().Operations.SingleOrDefault(o => o.ItemListRowCode == row1.Code);
            if (row1Operation == null)
            {
                Assert.Inconclusive("Row mission can't be null");
            }

            #endregion

            #region Act

            var result = await missionSchedulerService.ExecuteOperationAsync(row1Operation.Id);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            using (var context = this.CreateContext())
            {
                var updatedOperation = context.MissionOperations.Single(o => o.Id == result.Entity.Id);
                var updatedMission = (await missionProvider.GetAllAsync(0, 1)).FirstOrDefault();
                var updatedList = await listExecutionProvider.GetByIdAsync(list.Id);
                var updatedRow1 = await rowExecutionProvider.GetByIdAsync(row1.Id);

                Assert.AreEqual(
                   Common.DataModels.MissionOperationStatus.Executing,
                   updatedOperation.Status,
                   "The operation should be in the Executing state.");

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
            }

            #endregion
        }

        #endregion
    }
}
