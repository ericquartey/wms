using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataModels = Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public abstract class BaseControllerTest
    {
        #region Properties

        protected DataModels.Aisle Aisle1 { get; set; }

        protected DataModels.Aisle Aisle2 { get; set; }

        protected DataModels.Area Area1 { get; set; }

        protected DataModels.Area Area2 { get; set; }

        protected DataModels.Bay Bay1 { get; set; }

        protected DataModels.Bay Bay2 { get; set; }

        protected DataModels.BayType BayType1 { get; set; }

        protected DataModels.Cell Cell1 { get; set; }

        protected DataModels.Cell Cell2 { get; set; }

        protected DataModels.Cell Cell3 { get; set; }

        protected DataModels.Cell Cell4 { get; set; }

        protected DataModels.LoadingUnit LoadingUnit1 { get; set; }

        protected DataModels.LoadingUnit LoadingUnit2 { get; set; }

        protected DataModels.LoadingUnit LoadingUnit3 { get; set; }

        protected DataModels.LoadingUnit LoadingUnit4 { get; set; }

        protected DataModels.Machine Machine1 { get; set; }

        protected DataModels.MachineType MachineType1 { get; set; }

        #endregion Properties

        #region Methods

        protected void CleanupDatabase()
        {
            using (var context = this.CreateContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        protected DatabaseContext CreateContext()
        {
            return new DatabaseContext(
                new DbContextOptionsBuilder<DatabaseContext>()
                    .UseInMemoryDatabase(databaseName: this.GetType().FullName)
                    .Options);
        }

        protected virtual void InitializeDatabase()
        {
            this.Area1 = new DataModels.Area { Id = 1, Name = "Area #1" };
            this.Area2 = new DataModels.Area { Id = 2, Name = "Area #2" };
            this.Aisle1 = new DataModels.Aisle { Id = 1, AreaId = this.Area1.Id, Name = "Aisle #1" };
            this.Aisle2 = new DataModels.Aisle { Id = 2, AreaId = this.Area2.Id, Name = "Aisle #2" };
            this.Cell1 = new DataModels.Cell { Id = 1, AisleId = this.Aisle1.Id };
            this.Cell2 = new DataModels.Cell { Id = 2, AisleId = this.Aisle1.Id };
            this.Cell3 = new DataModels.Cell { Id = 3, AisleId = this.Aisle2.Id };
            this.Cell4 = new DataModels.Cell { Id = 4, AisleId = this.Aisle2.Id };
            this.LoadingUnit1 = new DataModels.LoadingUnit { Id = 1, Code = "Loading Unit #1", CellId = this.Cell1.Id };
            this.LoadingUnit2 = new DataModels.LoadingUnit { Id = 2, Code = "Loading Unit #2", CellId = this.Cell2.Id };
            this.LoadingUnit3 = new DataModels.LoadingUnit { Id = 3, Code = "Loading Unit #3", CellId = this.Cell3.Id };
            this.LoadingUnit4 = new DataModels.LoadingUnit { Id = 4, Code = "Loading Unit #4", CellId = this.Cell4.Id };
            this.MachineType1 = new DataModels.MachineType { Id = "T", Description = "Machine Type #1" };
            this.BayType1 = new DataModels.BayType { Id = "T", Description = "Bay Type #1" };
            this.Machine1 = new DataModels.Machine
            {
                Id = 1,
                AisleId = this.Aisle1.Id,
                MachineTypeId = this.MachineType1.Id,
                Model = "Model Machine #1",
                Nickname = "Machine #1",
                RegistrationNumber = "Registration Number Machine #1",
            };
            this.Bay1 = new DataModels.Bay
            {
                Id = 1,
                AreaId = this.Area1.Id,
                BayTypeId = this.BayType1.Id,
                Description = "Bay #1",
                LoadingUnitsBufferSize = 10,
                MachineId = this.Machine1.Id,
            };
            this.Bay2 = new DataModels.Bay
            {
                Id = 2,
                AreaId = this.Area1.Id,
                BayTypeId = this.BayType1.Id,
                Description = "Bay #2",
                LoadingUnitsBufferSize = 10,
                MachineId = this.Machine1.Id,
            };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.Area1);
                context.Areas.Add(this.Area2);
                context.Aisles.Add(this.Aisle1);
                context.Aisles.Add(this.Aisle2);
                context.Cells.Add(this.Cell1);
                context.Cells.Add(this.Cell2);
                context.Cells.Add(this.Cell3);
                context.Cells.Add(this.Cell4);
                context.LoadingUnits.Add(this.LoadingUnit1);
                context.LoadingUnits.Add(this.LoadingUnit2);
                context.LoadingUnits.Add(this.LoadingUnit3);
                context.LoadingUnits.Add(this.LoadingUnit4);
                context.MachineTypes.Add(this.MachineType1);
                context.BayTypes.Add(this.BayType1);
                context.Machines.Add(this.Machine1);
                context.Bays.Add(this.Bay1);
                context.Bays.Add(this.Bay2);

                context.SaveChanges();
            }
        }

        #endregion Methods
    }
}
