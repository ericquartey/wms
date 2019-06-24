using System.Linq;
using Ferretto.Common.DataModels;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public abstract class BaseControllerTest
    {
        #region Fields

        private ServiceProvider serviceProvider;

        #endregion

        #region Properties

        public LoadingUnitStatus LoadingUnitStatus1 { get; set; }

        protected AbcClass AbcClass1 { get; set; }

        protected Aisle Aisle1 { get; set; }

        protected Aisle Aisle2 { get; set; }

        protected Aisle Aisle3 { get; set; }

        protected Area Area1 { get; set; }

        protected Area Area2 { get; set; }

        protected Area Area3 { get; set; }

        protected Bay Bay1 { get; set; }

        protected Bay Bay2 { get; set; }

        protected BayType BayType1 { get; set; }

        protected Cell Cell1 { get; set; }

        protected Cell Cell2 { get; set; }

        protected Cell Cell3 { get; set; }

        protected Cell Cell4 { get; set; }

        protected Cell Cell5 { get; set; }

        protected Cell Cell6 { get; set; }

        protected CellStatus CellStatus1 { get; set; }

        protected CellType CellType1 { get; set; }

        protected LoadingUnit LoadingUnit1 { get; set; }

        protected LoadingUnit LoadingUnit2 { get; set; }

        protected LoadingUnit LoadingUnit3 { get; set; }

        protected LoadingUnit LoadingUnit4 { get; set; }

        protected LoadingUnitHeightClass LoadingUnitHeightClass1 { get; set; }

        protected LoadingUnitSizeClass LoadingUnitSizeClass1 { get; set; }

        protected LoadingUnitType LoadingUnitType1 { get; set; }

        protected LoadingUnitTypeAisle LoadingUnitType1Aisle1 { get; set; }

        protected LoadingUnitTypeAisle LoadingUnitType1Aisle2 { get; set; }

        protected LoadingUnitTypeAisle LoadingUnitType1Aisle3 { get; set; }

        protected LoadingUnitType LoadingUnitType2 { get; set; }

        protected LoadingUnitTypeAisle LoadingUnitType2Aisle1 { get; set; }

        protected LoadingUnitTypeAisle LoadingUnitType2Aisle2 { get; set; }

        protected LoadingUnitTypeAisle LoadingUnitType2Aisle3 { get; set; }

        protected LoadingUnitWeightClass LoadingUnitWeightClass1 { get; set; }

        protected Machine Machine1 { get; set; }

        protected MachineType MachineType1 { get; set; }

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
            services.AddSingleton<INotificationService, NotificationServiceMock>();
            services.AddSingleton(new Mock<IHubContext<DataHub, IDataHub>>().Object);
            services.AddSingleton(new Mock<IApplicationLifetime>().Object);

            services.AddDbContext<DatabaseContext>(
                options => options.UseInMemoryDatabase(databaseName),
                ServiceLifetime.Transient);

            services.AddTransient(typeof(MissionOperationsController));
            services.AddSingleton(new Mock<ILogger<MissionOperationsController>>().Object);

            return services.BuildServiceProvider();
        }

        protected static string GetDescription(ActionResult actionResult)
        {
            var resultValue = (actionResult as ObjectResult)?.Value;

            if (resultValue == null)
            {
                return string.Empty;
            }

            if (resultValue is ProblemDetails problemDetails)
            {
                return problemDetails.Detail;
            }

            if (resultValue
                .GetType()
                .GetInterfaces()
                .Any(i => i.Name.StartsWith("IOperationResult", System.StringComparison.InvariantCulture)))
            {
                dynamic dynamicValue = resultValue;
                return dynamicValue.Description;
            }

            return string.Empty;
        }

        protected DatabaseContext CreateContext()
        {
            return this.ServiceProvider.GetService<DatabaseContext>();
        }

        protected virtual void InitializeDatabase()
        {
            this.AbcClass1 = new AbcClass { Id = "A", Description = "A Class" };
            this.Area1 = new Area { Id = 1, Name = "Area #1" };
            this.Area2 = new Area { Id = 2, Name = "Area #2" };
            this.Area3 = new Area { Id = 3, Name = "Area #3" };
            this.Aisle1 = new Aisle { Id = 1, AreaId = this.Area1.Id, Name = "Aisle #1" };
            this.Aisle2 = new Aisle { Id = 2, AreaId = this.Area2.Id, Name = "Aisle #2" };
            this.Aisle3 = new Aisle { Id = 3, AreaId = this.Area2.Id, Name = "Aisle #3" };
            this.MachineType1 = new MachineType { Id = "T", Description = "Machine Type #1" };
            this.Machine1 = new Machine
            {
                Id = 1,
                AisleId = this.Aisle1.Id,
                MachineTypeId = this.MachineType1.Id,
                Model = "Model Machine #1",
                Nickname = "Machine #1",
                RegistrationNumber = "Registration Number Machine #1",
                TotalMaxWeight = 70000
            };
            this.BayType1 = new BayType { Id = "T", Description = "Bay Type #1" };
            this.Bay1 = new Bay
            {
                Id = 1,
                AreaId = this.Area1.Id,
                BayTypeId = this.BayType1.Id,
                Description = "Bay #1",
                LoadingUnitsBufferSize = 10,
                MachineId = this.Machine1.Id,
            };
            this.Bay2 = new Bay
            {
                Id = 2,
                AreaId = this.Area1.Id,
                BayTypeId = this.BayType1.Id,
                Description = "Bay #2",
                LoadingUnitsBufferSize = 10,
                MachineId = this.Machine1.Id,
            };
            this.CellStatus1 = new CellStatus { Id = 1, Description = "Cell Status #1" };
            this.CellType1 = new CellType { Id = 1, Description = "Cell Type #1" };
            this.Cell1 = new Cell
            {
                Id = 1,
                AisleId = this.Aisle1.Id,
                AbcClassId = this.AbcClass1.Id,
                CellTypeId = this.CellType1.Id,
                CellStatusId = this.CellStatus1.Id,
                Priority = 1
            };
            this.Cell2 = new Cell
            {
                Id = 2,
                AisleId = this.Aisle1.Id,
                AbcClassId = this.AbcClass1.Id,
                CellTypeId = this.CellType1.Id,
                CellStatusId = this.CellStatus1.Id,
                Priority = 2
            };
            this.Cell3 = new Cell
            {
                Id = 3,
                AisleId = this.Aisle2.Id,
                AbcClassId = this.AbcClass1.Id,
                CellTypeId = this.CellType1.Id,
                CellStatusId = this.CellStatus1.Id,
                Priority = 3
            };
            this.Cell4 = new Cell
            {
                Id = 4,
                AisleId = this.Aisle2.Id,
                AbcClassId = this.AbcClass1.Id,
                CellTypeId = this.CellType1.Id,
                CellStatusId = this.CellStatus1.Id,
                Priority = 4
            };
            this.Cell5 = new Cell
            {
                Id = 5,
                AisleId = this.Aisle2.Id,
                AbcClassId = this.AbcClass1.Id,
                CellTypeId = this.CellType1.Id,
                CellStatusId = this.CellStatus1.Id,
                Priority = 5
            };
            this.Cell6 = new Cell
            {
                Id = 6,
                AisleId = this.Aisle2.Id,
                AbcClassId = this.AbcClass1.Id,
                CellTypeId = this.CellType1.Id,
                CellStatusId = this.CellStatus1.Id,
                Priority = 6
            };
            this.LoadingUnitHeightClass1 = new LoadingUnitHeightClass
            { Id = 1, Description = "Loading Unit Height Class #1", MaxHeight = 1, MinHeight = 100 };
            this.LoadingUnitSizeClass1 = new LoadingUnitSizeClass
            { Id = 1, Description = "Loading Unit Size Class #1", Length = 100, Width = 100 };
            this.LoadingUnitWeightClass1 = new LoadingUnitWeightClass
            { Id = 1, Description = "Loading Unit Weight Class #1", MinWeight = 1, MaxWeight = 10 };
            this.LoadingUnitType1 = new LoadingUnitType
            {
                Id = 1,
                LoadingUnitHeightClassId = this.LoadingUnitHeightClass1.Id,
                LoadingUnitWeightClassId = this.LoadingUnitWeightClass1.Id,
                LoadingUnitSizeClassId = this.LoadingUnitSizeClass1.Id,
                Description = "Loading Unit Type #1",
                HasCompartments = true,
                EmptyWeight = 17
            };
            this.LoadingUnitType2 = new LoadingUnitType
            {
                Id = 2,
                LoadingUnitHeightClassId = this.LoadingUnitHeightClass1.Id,
                LoadingUnitWeightClassId = this.LoadingUnitWeightClass1.Id,
                LoadingUnitSizeClassId = this.LoadingUnitSizeClass1.Id,
                Description = "Loading Unit Type #2",
                HasCompartments = true,
                EmptyWeight = 13
            };
            this.LoadingUnitType1Aisle1 = new LoadingUnitTypeAisle
            {
                AisleId = this.Aisle1.Id,
                LoadingUnitTypeId = this.LoadingUnitType1.Id,
            };
            this.LoadingUnitType2Aisle1 = new LoadingUnitTypeAisle
            {
                AisleId = this.Aisle1.Id,
                LoadingUnitTypeId = this.LoadingUnitType2.Id,
            };
            this.LoadingUnitType1Aisle2 = new LoadingUnitTypeAisle
            {
                AisleId = this.Aisle2.Id,
                LoadingUnitTypeId = this.LoadingUnitType1.Id,
            };
            this.LoadingUnitType2Aisle2 = new LoadingUnitTypeAisle
            {
                AisleId = this.Aisle2.Id,
                LoadingUnitTypeId = this.LoadingUnitType2.Id,
            };
            this.LoadingUnitType1Aisle3 = new LoadingUnitTypeAisle
            {
                AisleId = this.Aisle3.Id,
                LoadingUnitTypeId = this.LoadingUnitType1.Id,
            };
            this.LoadingUnitType2Aisle3 = new LoadingUnitTypeAisle
            {
                AisleId = this.Aisle3.Id,
                LoadingUnitTypeId = this.LoadingUnitType2.Id,
            };
            this.LoadingUnitStatus1 = new LoadingUnitStatus
            {
                Id = "1",
                Description = "Loading Unit Status #1"
            };
            this.LoadingUnit1 = new LoadingUnit
            {
                Id = 11,
                Code = "Loading Unit #1",
                CellId = this.Cell1.Id,
                LoadingUnitTypeId = this.LoadingUnitType1.Id,
                Weight = 100,
                AbcClassId = this.AbcClass1.Id,
                Height = 10,
                LoadingUnitStatusId = this.LoadingUnitStatus1.Id,
            };
            this.LoadingUnit2 = new LoadingUnit
            {
                Id = 12,
                Code = "Loading Unit #2",
                CellId = this.Cell2.Id,
                LoadingUnitTypeId = this.LoadingUnitType2.Id,
                Weight = 200,
                AbcClassId = this.AbcClass1.Id,
                Height = 10,
                LoadingUnitStatusId = this.LoadingUnitStatus1.Id,
            };
            this.LoadingUnit3 = new LoadingUnit
            {
                Id = 13,
                Code = "Loading Unit #3",
                CellId = this.Cell3.Id,
                LoadingUnitTypeId = this.LoadingUnitType1.Id,
                AbcClassId = this.AbcClass1.Id,
                Height = 10,
                LoadingUnitStatusId = this.LoadingUnitStatus1.Id,
            };
            this.LoadingUnit4 = new LoadingUnit
            {
                Id = 14,
                Code = "Loading Unit #4",
                CellId = this.Cell4.Id,
                LoadingUnitTypeId = this.LoadingUnitType1.Id,
                AbcClassId = this.AbcClass1.Id,
                Height = 10,
                LoadingUnitStatusId = this.LoadingUnitStatus1.Id,
            };

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
                context.LoadingUnitTypesAisles.Add(this.LoadingUnitType1Aisle1);
                context.LoadingUnitTypesAisles.Add(this.LoadingUnitType2Aisle1);
                context.LoadingUnitTypesAisles.Add(this.LoadingUnitType1Aisle2);
                context.LoadingUnitTypesAisles.Add(this.LoadingUnitType2Aisle2);
                context.LoadingUnitTypesAisles.Add(this.LoadingUnitType1Aisle3);
                context.LoadingUnitTypesAisles.Add(this.LoadingUnitType2Aisle3);
                context.LoadingUnitStatuses.Add(this.LoadingUnitStatus1);
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
