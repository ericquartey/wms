using Ferretto.Common.DataModels;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public abstract class BaseWarehouseTest
    {
        #region Fields

        private ServiceProvider serviceProvider;

        #endregion

        #region Properties

        protected Aisle Aisle1Area1 { get; private set; }

        protected Aisle Aisle2Area1 { get; private set; }

        protected Area Area1 { get; private set; }

        protected Bay Bay1Aisle1 { get; private set; }

        protected Bay Bay2Aisle2 { get; private set; }

        protected Cell Cell1Aisle1 { get; private set; }

        protected Cell Cell2Aisle2 { get; private set; }

        protected Item Item1 { get; private set; }

        protected Item ItemFifo { get; private set; }

        protected Item ItemVolume { get; private set; }

        protected LoadingUnit LoadingUnit1Cell1 { get; private set; }

        protected LoadingUnit LoadingUnit2Cell2 { get; private set; }

        protected Machine Machine1Aisle1 { get; private set; }

        protected Machine Machine2Aisle2 { get; private set; }

        private ServiceProvider ServiceProvider => this.serviceProvider ?? (this.serviceProvider = CreateServices());

        #endregion

        #region Methods

        protected static ServiceProvider CreateServices()
        {
            var databaseName = typeof(BaseWarehouseTest).FullName;

            var services = new ServiceCollection();
            services.AddSchedulerServiceProviders();
            services.AddDataServiceProviders();
            services.AddSingleton(new Mock<IConfiguration>().Object);
            services.AddSingleton(new Mock<IContentTypeProvider>().Object);
            services.AddSingleton(new Mock<IHostingEnvironment>().Object);

            services.AddDbContext<DatabaseContext>(
                options => options.UseInMemoryDatabase(databaseName),
                ServiceLifetime.Transient);

            return services.BuildServiceProvider();
        }

        protected void CleanupDatabase()
        {
            using (var context = this.CreateContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        protected DatabaseContext CreateContext()
        {
            return this.ServiceProvider.GetService<DatabaseContext>();
        }

        protected T GetService<T>()
            where T : class
        {
            return this.ServiceProvider.GetService(typeof(T)) as T;
        }

        protected void InitializeDatabase()
        {
            this.Area1 = new Area { Id = 1, Name = "Area #1" };
            this.Aisle1Area1 = new Aisle { Id = 1, AreaId = this.Area1.Id, Name = "Aisle #1" };
            this.Aisle2Area1 = new Aisle { Id = 2, AreaId = this.Area1.Id, Name = "Aisle #2" };
            this.Machine1Aisle1 = new Machine { Id = 1, AisleId = this.Aisle1Area1.Id };
            this.Machine2Aisle2 = new Machine { Id = 2, AisleId = this.Aisle2Area1.Id };
            this.Cell1Aisle1 = new Cell { Id = 1, AisleId = this.Aisle1Area1.Id };
            this.Cell2Aisle2 = new Cell { Id = 2, AisleId = this.Aisle2Area1.Id };
            this.LoadingUnit1Cell1 = new LoadingUnit { Id = 1, Code = "Loading Unit #1", CellId = this.Cell1Aisle1.Id };
            this.LoadingUnit2Cell2 = new LoadingUnit { Id = 2, Code = "Loading Unit #2", CellId = this.Cell2Aisle2.Id };
            this.Bay1Aisle1 = new Bay { Id = 1, Description = "Bay #1", AreaId = this.Area1.Id, LoadingUnitsBufferSize = 2, Priority = 1, MachineId = this.Machine1Aisle1.Id };
            this.Bay2Aisle2 = new Bay { Id = 2, Description = "Bay #2", AreaId = this.Area1.Id, LoadingUnitsBufferSize = 2, Priority = 1, MachineId = this.Machine2Aisle2.Id };
            this.Item1 = new Item { Id = 1, Code = "Item #1", ManagementType = ItemManagementType.FIFO };
            this.ItemFifo = new Item { Id = 2, Code = "Item #2", ManagementType = ItemManagementType.FIFO };
            this.ItemVolume = new Item { Id = 3, Code = "Item #3", ManagementType = ItemManagementType.Volume };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.Area1);
                context.Aisles.Add(this.Aisle1Area1);
                context.Aisles.Add(this.Aisle2Area1);
                context.Machines.Add(this.Machine1Aisle1);
                context.Machines.Add(this.Machine2Aisle2);
                context.Cells.Add(this.Cell1Aisle1);
                context.Cells.Add(this.Cell2Aisle2);
                context.LoadingUnits.Add(this.LoadingUnit1Cell1);
                context.LoadingUnits.Add(this.LoadingUnit2Cell2);
                context.Bays.Add(this.Bay1Aisle1);
                context.Bays.Add(this.Bay2Aisle2);
                context.Items.Add(this.Item1);
                context.Items.Add(this.ItemFifo);
                context.Items.Add(this.ItemVolume);

                context.SaveChanges();
            }
        }

        #endregion
    }
}
