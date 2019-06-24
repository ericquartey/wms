using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Tests;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    [TestClass]
    public abstract class BaseDataContextTest
    {
        #region Fields

        private ServiceProvider serviceProvider;

        #endregion

        #region Properties

        private ServiceProvider ServiceProvider => this.serviceProvider ?? (this.serviceProvider = this.CreateServices());

        #endregion

        #region Methods

        protected virtual void AddServices(IServiceCollection services)
        {
            // Do nothing here.
            // Derived tests can add further services to the service collection.
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

        protected virtual ServiceProvider CreateServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<INotificationService, NotificationServiceMock>();
            services.AddSingleton(new Mock<IHubContext<DataHub, IDataHub>>().Object);

            services.AddDbContext<DatabaseContext>(
                options => options
                    .UseInMemoryDatabase(this.GetType().FullName)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors(),
                ServiceLifetime.Transient);

            this.AddServices(services);

            return services.BuildServiceProvider();
        }

        protected T GetService<T>()
            where T : class
        {
            return this.ServiceProvider.GetService(typeof(T)) as T;
        }

        #endregion
    }
}
