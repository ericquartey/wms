using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        #region Fields

        private readonly DatabaseContext dbContext;

        #endregion Fields

        #region Constructors

        public ItemsController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #endregion Constructors

        #region Methods

        [HttpGet("{id}")]
        public ActionResult<ItemDetails> Get(int id)
        {
            var itemDetails = this.dbContext.Items
                .Where(i => i.Id == id)
                .Select(i => new ItemDetails
                {
                    Id = i.Id,
                    Code = i.Code,
                    Description = i.Description,
                    ItemCategoryId = i.ItemCategoryId,
                    Note = i.Note,

                    AbcClassId = i.AbcClassId,
                    MeasureUnitId = i.MeasureUnitId,
                    ItemManagementTypeId = i.ItemManagementTypeId,
                    FifoTimePick = i.FifoTimePick,
                    FifoTimeStore = i.FifoTimeStore,
                    ReorderPoint = i.ReorderPoint,
                    ReorderQuantity = i.ReorderQuantity,

                    Height = i.Height,
                    Length = i.Length,
                    Width = i.Width,
                    PickTolerance = i.PickTolerance,
                    StoreTolerance = i.StoreTolerance,
                    InventoryTolerance = i.InventoryTolerance,
                    AverageWeight = i.AverageWeight,

                    Image = i.Image,

                    CreationDate = i.CreationDate,
                    InventoryDate = i.InventoryDate,
                    LastModificationDate = i.LastModificationDate,
                    LastPickDate = i.LastPickDate,
                    LastStoreDate = i.LastStoreDate,
                }
                )
                .AsNoTracking()
                .Single();
            /*
            itemDetails.AbcClassChoices = this.enumerationProvider.GetAllAbcClasses();
            itemDetails.MeasureUnitChoices = this.enumerationProvider.GetAllMeasureUnits();
            itemDetails.ItemManagementTypeChoices = this.enumerationProvider.GetAllItemManagementTypes();
            itemDetails.ItemCategoryChoices = this.enumerationProvider.GetAllItemCategories();
            */
            return itemDetails;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Item>> GetAll()
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

        [HttpPut()]
        public ActionResult SaveComplete([FromBody] Item model)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var existingModel = this.dbContext.Items.Find(model.Id);

            this.dbContext.Entry(existingModel).CurrentValues.SetValues(model);
            existingModel.LastModificationDate = DateTime.Now;
            this.dbContext.SaveChanges();
            sw.Stop();

            Console.WriteLine($"!!!!! {sw.ElapsedMilliseconds}");
            return this.Ok();
        }

        [HttpPost]
        public ActionResult Withdraw(int id, int quantity)
        {
            return this.Ok($"Item {id} withdrawn with quantity {quantity}");
        }

        #endregion Methods
    }
}
