using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public abstract class BaseWarehouseTest
    {
        #region Fields

        private ServiceProvider serviceProvider;

        #endregion

        #region Properties

        protected Common.DataModels.Aisle Aisle1 { get; private set; }

        protected Common.DataModels.Area Area1 { get; private set; }

        protected Common.DataModels.Bay Bay1 { get; private set; }

        protected Common.DataModels.Cell Cell1 { get; private set; }

        protected Common.DataModels.Item Item1 { get; private set; }

        protected Common.DataModels.Item ItemFifo { get; private set; }

        protected Common.DataModels.Item ItemVolume { get; private set; }

        protected Common.DataModels.LoadingUnit LoadingUnit1 { get; private set; }

        protected ServiceProvider ServiceProvider => this.serviceProvider ?? (this.serviceProvider = CreateServices());

        #endregion

        #region Methods

        protected static ServiceProvider CreateServices()
        {
            var databaseName = typeof(BaseWarehouseTest).FullName;

            var services = new ServiceCollection();
            services.AddSchedulerServiceProviders();

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

        protected void InitializeDatabase()
        {
            this.Area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
            this.Aisle1 = new Common.DataModels.Aisle { Id = 1, AreaId = this.Area1.Id, Name = "Aisle #1" };
            this.Cell1 = new Common.DataModels.Cell { Id = 1, AisleId = this.Aisle1.Id };
            this.LoadingUnit1 = new Common.DataModels.LoadingUnit { Id = 1, Code = "Loading Unit #1", CellId = this.Cell1.Id };
            this.Bay1 = new Common.DataModels.Bay { Id = 1, Description = "Bay #1", AreaId = this.Area1.Id, LoadingUnitsBufferSize = 2 };
            this.Item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1", ManagementType = Common.DataModels.ItemManagementType.FIFO };
            this.ItemFifo = new Common.DataModels.Item { Id = 2, Code = "Item #2", ManagementType = Common.DataModels.ItemManagementType.FIFO };
            this.ItemVolume = new Common.DataModels.Item { Id = 3, Code = "Item #3", ManagementType = Common.DataModels.ItemManagementType.Volume };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.Area1);
                context.Aisles.Add(this.Aisle1);
                context.Bays.Add(this.Bay1);
                context.Cells.Add(this.Cell1);
                context.LoadingUnits.Add(this.LoadingUnit1);
                context.Items.Add(this.Item1);
                context.Items.Add(this.ItemFifo);
                context.Items.Add(this.ItemVolume);

                context.SaveChanges();
            }
        }

        #endregion
    }
}
