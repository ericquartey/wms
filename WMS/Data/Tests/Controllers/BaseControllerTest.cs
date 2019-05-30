using System.Collections.Generic;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public abstract class BaseControllerTest
    {
        #region Fields

        private ServiceProvider serviceProvider;

        #endregion

        #region Properties

        protected Common.DataModels.AbcClass AbcClass1 { get; set; }

        protected Common.DataModels.Aisle Aisle1 { get; set; }

        protected Common.DataModels.Aisle Aisle2 { get; set; }

        protected Common.DataModels.Aisle Aisle3 { get; set; }

        protected Common.DataModels.Area Area1 { get; set; }

        protected Common.DataModels.Area Area2 { get; set; }

        protected Common.DataModels.Area Area3 { get; set; }

        protected Common.DataModels.Bay Bay1 { get; set; }

        protected Common.DataModels.Bay Bay2 { get; set; }

        protected Common.DataModels.BayType BayType1 { get; set; }

        protected Common.DataModels.Cell Cell1 { get; set; }

        protected Common.DataModels.Cell Cell2 { get; set; }

        protected Common.DataModels.Cell Cell3 { get; set; }

        protected Common.DataModels.Cell Cell4 { get; set; }

        protected Common.DataModels.Cell Cell5 { get; set; }

        protected Common.DataModels.Cell Cell6 { get; set; }

        protected Common.DataModels.CellStatus CellStatus1 { get; set; }

        protected Common.DataModels.CellType CellType1 { get; set; }

        protected Common.DataModels.LoadingUnit LoadingUnit1 { get; set; }

        protected Common.DataModels.LoadingUnit LoadingUnit2 { get; set; }

        protected Common.DataModels.LoadingUnit LoadingUnit3 { get; set; }

        protected Common.DataModels.LoadingUnit LoadingUnit4 { get; set; }

        protected Common.DataModels.LoadingUnitHeightClass LoadingUnitHeightClass1 { get; set; }

        protected Common.DataModels.LoadingUnitSizeClass LoadingUnitSizeClass1 { get; set; }

        protected Common.DataModels.LoadingUnitWeightClass LoadingUnitWeightClass1 { get; set; }

        protected Common.DataModels.LoadingUnitType LoadingUnitType1 { get; set; }

        protected Common.DataModels.LoadingUnitType LoadingUnitType2 { get; set; }

        protected Common.DataModels.Machine Machine1 { get; set; }

        protected Common.DataModels.MachineType MachineType1 { get; set; }

        protected ServiceProvider ServiceProvider => this.serviceProvider ?? (this.serviceProvider = CreateServices());

        #endregion

        #region Methods

        protected static ServiceProvider CreateServices()
        {
            var databaseName = typeof(BaseControllerTest).FullName;

            var services = new ServiceCollection();
            services.AddDataServiceProviders();
            services.AddSchedulerServiceProviders();
            services.AddSingleton(new Mock<IConfiguration>().Object);
            services.AddSingleton(new Mock<IContentTypeProvider>().Object);
            services.AddSingleton(new Mock<IHostingEnvironment>().Object);

            services.AddDbContext<DatabaseContext>(
                options => options.UseInMemoryDatabase(databaseName),
                ServiceLifetime.Transient);

            return services.BuildServiceProvider();
        }

        protected DatabaseContext CreateContext()
        {
            return this.ServiceProvider.GetService<DatabaseContext>();
        }

        protected virtual void InitializeDatabase()
        {
            this.AbcClass1 = new Common.DataModels.AbcClass { Id = "A", Description = "A Class" };
            this.Area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
            this.Area2 = new Common.DataModels.Area { Id = 2, Name = "Area #2" };
            this.Area3 = new Common.DataModels.Area { Id = 3, Name = "Area #3" };
            this.Aisle1 = new Common.DataModels.Aisle { Id = 1, AreaId = this.Area1.Id, Name = "Aisle #1" };
            this.Aisle2 = new Common.DataModels.Aisle { Id = 2, AreaId = this.Area2.Id, Name = "Aisle #2" };
            this.Aisle3 = new Common.DataModels.Aisle { Id = 3, AreaId = this.Area2.Id, Name = "Aisle #3" };
            this.MachineType1 = new Common.DataModels.MachineType { Id = "T", Description = "Machine Type #1" };
            this.Machine1 = new Common.DataModels.Machine
            {
                Id = 1,
                AisleId = this.Aisle1.Id,
                MachineTypeId = this.MachineType1.Id,
                Model = "Model Machine #1",
                Nickname = "Machine #1",
                RegistrationNumber = "Registration Number Machine #1",
                TotalMaxWeight = 70000
            };
            this.BayType1 = new Common.DataModels.BayType { Id = "T", Description = "Bay Type #1" };
            this.Bay1 = new Common.DataModels.Bay
            {
                Id = 1,
                AreaId = this.Area1.Id,
                BayTypeId = this.BayType1.Id,
                Description = "Bay #1",
                LoadingUnitsBufferSize = 10,
                MachineId = this.Machine1.Id,
            };
            this.Bay2 = new Common.DataModels.Bay
            {
                Id = 2,
                AreaId = this.Area1.Id,
                BayTypeId = this.BayType1.Id,
                Description = "Bay #2",
                LoadingUnitsBufferSize = 10,
                MachineId = this.Machine1.Id,
            };
            this.CellStatus1 = new Common.DataModels.CellStatus { Id = 1, Description = "Cell Status #1" };
            this.CellType1 = new Common.DataModels.CellType { Id = 1, Description = "Cell Type #1" };
            this.Cell1 = new Common.DataModels.Cell { Id = 1, AisleId = this.Aisle1.Id, AbcClassId = this.AbcClass1.Id, CellTypeId = this.CellType1.Id, CellStatusId = this.CellStatus1.Id };
            this.Cell2 = new Common.DataModels.Cell { Id = 2, AisleId = this.Aisle1.Id, AbcClassId = this.AbcClass1.Id, CellTypeId = this.CellType1.Id, CellStatusId = this.CellStatus1.Id };
            this.Cell3 = new Common.DataModels.Cell { Id = 3, AisleId = this.Aisle2.Id, AbcClassId = this.AbcClass1.Id, CellTypeId = this.CellType1.Id, CellStatusId = this.CellStatus1.Id };
            this.Cell4 = new Common.DataModels.Cell { Id = 4, AisleId = this.Aisle2.Id, AbcClassId = this.AbcClass1.Id, CellTypeId = this.CellType1.Id, CellStatusId = this.CellStatus1.Id };
            this.Cell5 = new Common.DataModels.Cell { Id = 5, AisleId = this.Aisle2.Id, AbcClassId = this.AbcClass1.Id, CellTypeId = this.CellType1.Id, CellStatusId = this.CellStatus1.Id };
            this.Cell6 = new Common.DataModels.Cell { Id = 6, AisleId = this.Aisle2.Id, AbcClassId = this.AbcClass1.Id, CellTypeId = this.CellType1.Id, CellStatusId = this.CellStatus1.Id };
            this.LoadingUnitHeightClass1 = new Common.DataModels.LoadingUnitHeightClass { Id = 1, Description = "Loading Unit Height Class #1", MaxHeight = 1, MinHeight = 100 };
            this.LoadingUnitSizeClass1 = new Common.DataModels.LoadingUnitSizeClass { Id = 1, Description = "Loading Unit Size Class #1", Length = 100, Width = 100 };
            this.LoadingUnitWeightClass1 = new Common.DataModels.LoadingUnitWeightClass { Id = 1, Description = "Loading Unit Weight Class #1", MinWeight = 1, MaxWeight = 10 };
            this.LoadingUnitType1 = new Common.DataModels.LoadingUnitType
            {
                Id = 1,
                LoadingUnitHeightClassId = this.LoadingUnitHeightClass1.Id,
                LoadingUnitWeightClassId = this.LoadingUnitWeightClass1.Id,
                LoadingUnitSizeClassId = this.LoadingUnitSizeClass1.Id,
                Description = "Loading Unit Type #1",
                HasCompartments = false,
                EmptyWeight = 17
            };
            this.LoadingUnitType2 = new Common.DataModels.LoadingUnitType
            {
                Id = 2,
                LoadingUnitHeightClassId = this.LoadingUnitHeightClass1.Id,
                LoadingUnitWeightClassId = this.LoadingUnitWeightClass1.Id,
                LoadingUnitSizeClassId = this.LoadingUnitSizeClass1.Id,
                Description = "Loading Unit Type #2",
                HasCompartments = false,
                EmptyWeight = 13
            };
            this.LoadingUnit1 = new Common.DataModels.LoadingUnit
                { Id = 1, Code = "Loading Unit #1", CellId = this.Cell1.Id, LoadingUnitTypeId = this.LoadingUnitType1.Id, Weight = 100 };
            this.LoadingUnit2 = new Common.DataModels.LoadingUnit
                { Id = 2, Code = "Loading Unit #2", CellId = this.Cell2.Id, LoadingUnitTypeId = this.LoadingUnitType2.Id, Weight = 200 };
            this.LoadingUnit3 = new Common.DataModels.LoadingUnit
                { Id = 3, Code = "Loading Unit #3", CellId = this.Cell3.Id, LoadingUnitTypeId = this.LoadingUnitType1.Id };
            this.LoadingUnit4 = new Common.DataModels.LoadingUnit
                { Id = 4, Code = "Loading Unit #4", CellId = this.Cell4.Id, LoadingUnitTypeId = this.LoadingUnitType1.Id };

            using (var context = this.CreateContext())
            {
                context.AbcClasses.Add(this.AbcClass1);
                context.Areas.Add(this.Area1);
                context.Areas.Add(this.Area2);
                context.Areas.Add(this.Area3);
                context.Aisles.Add(this.Aisle1);
                context.Aisles.Add(this.Aisle2);
                context.Aisles.Add(this.Aisle3);
                context.MachineTypes.Add(this.MachineType1);
                context.Machines.Add(this.Machine1);
                context.BayTypes.Add(this.BayType1);
                context.Bays.Add(this.Bay1);
                context.Bays.Add(this.Bay2);
                context.CellStatuses.Add(this.CellStatus1);
                context.CellTypes.Add(this.CellType1);
                context.Cells.Add(this.Cell1);
                context.Cells.Add(this.Cell2);
                context.Cells.Add(this.Cell3);
                context.Cells.Add(this.Cell4);
                context.Cells.Add(this.Cell5);
                context.Cells.Add(this.Cell6);
                context.LoadingUnitHeightClasses.Add(this.LoadingUnitHeightClass1);
                context.LoadingUnitSizeClasses.Add(this.LoadingUnitSizeClass1);
                context.LoadingUnitWeightClasses.Add(this.LoadingUnitWeightClass1);
                context.LoadingUnitTypes.Add(this.LoadingUnitType1);
                context.LoadingUnitTypes.Add(this.LoadingUnitType2);
                context.LoadingUnits.Add(this.LoadingUnit1);
                context.LoadingUnits.Add(this.LoadingUnit2);
                context.LoadingUnits.Add(this.LoadingUnit3);
                context.LoadingUnits.Add(this.LoadingUnit4);

                context.SaveChanges();
            }
        }

        #endregion
    }
}
