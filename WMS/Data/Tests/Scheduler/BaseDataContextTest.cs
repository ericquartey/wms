using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests.Scheduler
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

            services.AddDbContext<DatabaseContext>(
                options => options.UseInMemoryDatabase(this.GetType().FullName),
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
