using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class MachineService : IMachine
    {
        #region Fields

        private readonly DatabaseContext dbContext;

        private readonly Machine[] machines = new Machine[]
               {
            new Machine
            {
                Id = 1,
                AisleName = "Vertimag 1",
                MachineTypeDescription = "Vertimag",
                LastPowerOn = System.DateTime.Now,
                Model = "2018/XS"
            },
            new Machine
            {
                Id = 2,
                AisleName = "Vertimag 2",
                MachineTypeDescription = "Vertimag",
                LastPowerOn = System.DateTime.Now.Subtract(System.TimeSpan.FromMinutes(15)),
                Model = "2018/XS"
            },
       };

        #endregion Fields

        #region Constructors

        public MachineService()
        {
            this.dbContext = new DatabaseContext();
        }

        #endregion Constructors

        #region Properties

        private IMachineCallback Callback => OperationContext.Current.GetCallbackChannel<IMachineCallback>();

        #endregion Properties

        #region Methods

        public double CompleteMission(double n1, double n2)
        {
            var result = n1 + n2;
            Console.WriteLine("Received Add({0},{1})", n1, n2);
            // Code added to write output to the console window.
            Console.WriteLine("Return: {0}", result);
            return result;
        }

        public IEnumerable<Machine> GetAll()
        {
            this.Callback.WakeUpClients("Wake up!!");
            return this.machines;
        }

        public IEnumerable<Item> GetAllItems()
        {
            return this.dbContext.Items
               .AsNoTracking()
               .Take(100)
               .Include(i => i.AbcClass)
               .Include(i => i.ItemManagementType)
               .Include(i => i.ItemCategory)
               .GroupJoin(
                   this.dbContext.Compartments
                       .AsNoTracking()
                       .Where(c => c.ItemId != null)
                       .GroupBy(c => c.ItemId)
                       .Select(j => new
                       {
                           ItemId = j.Key,
                           TotalStock = j.Sum(x => x.Stock),
                           TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                           TotalReservedToStore = j.Sum(x => x.ReservedToStore)
                       }),
                   i => i.Id,
                   c => c.ItemId,
                   (i, c) => new
                   {
                       Item = i,
                       CompartmentsAggregation = c
                   })
               .SelectMany(
                   temp => temp.CompartmentsAggregation.DefaultIfEmpty(),
                   (a, b) => new Item
                   {
                       Id = a.Item.Id,
                       AbcClassDescription = a.Item.AbcClass.Description,
                       AverageWeight = a.Item.AverageWeight,
                       CreationDate = a.Item.CreationDate,
                       FifoTimePick = a.Item.FifoTimePick,
                       FifoTimeStore = a.Item.FifoTimeStore,
                       Height = a.Item.Height,
                       InventoryDate = a.Item.InventoryDate,
                       InventoryTolerance = a.Item.InventoryTolerance,
                       ItemManagementTypeDescription = a.Item.ItemManagementType.Description,
                       ItemCategoryDescription = a.Item.ItemCategory.Description,
                       LastModificationDate = a.Item.LastModificationDate,
                       LastPickDate = a.Item.LastPickDate,
                       LastStoreDate = a.Item.LastStoreDate,
                       Length = a.Item.Length,
                       MeasureUnitDescription = a.Item.MeasureUnit.Description,
                       PickTolerance = a.Item.PickTolerance,
                       ReorderPoint = a.Item.ReorderPoint,
                       ReorderQuantity = a.Item.ReorderQuantity,
                       StoreTolerance = a.Item.StoreTolerance,
                       Width = a.Item.Width,
                       Code = a.Item.Code,
                       Description = a.Item.Description,
                       TotalReservedForPick = b != null ? b.TotalReservedForPick : 0,
                       TotalReservedToStore = b != null ? b.TotalReservedToStore : 0,
                       TotalStock = b != null ? b.TotalStock : 0,
                       TotalAvailable = b != null
                           ? (b.TotalStock + b.TotalReservedToStore - b.TotalReservedForPick)
                           : 0,
                   }
               ).ToArray();
        }

        #endregion Methods
    }
}
