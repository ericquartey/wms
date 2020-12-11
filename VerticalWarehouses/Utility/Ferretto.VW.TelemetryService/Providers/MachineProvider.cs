using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Realms;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class MachineProvider : IMachineProvider
    {
        #region Fields

        private const string MachineCacheKey = nameof(MachineCacheKey);

        private static readonly MemoryCacheEntryOptions DefaultMemoryCacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = System.TimeSpan.FromDays(1)
        };

        private readonly IMemoryCache cache;

        private readonly Realm realm;

        #endregion

        #region Constructors

        public MachineProvider(Realm realm, IMemoryCache cache)
        {
            this.realm = realm;
            this.cache = cache;
        }

        #endregion

        #region Methods

        public IMachine? Get()
        {
            if (!this.cache.TryGetValue<Models.Machine>(MachineCacheKey, out var machine))
            {
                var loadedMachine = this.realm.All<Models.Machine>().SingleOrDefault();

                if (loadedMachine is null)
                {
                    return null;
                }

                var newMachine = new Machine
                {
                    ModelName = loadedMachine.ModelName,
                    SerialNumber = loadedMachine.SerialNumber,
                    Version = loadedMachine.Version,
                };

                return this.cache.Set(
                    MachineCacheKey,
                    newMachine,
                    DefaultMemoryCacheOptions);
            }

            return machine;
        }

        public async Task SaveAsync(IMachine machine)
        {
            // TODO: add check to avoid inserting duplicate records

            var machineInDatabase = this.realm.All<Models.Machine>().SingleOrDefault();

            if (machineInDatabase is null)
            {
                var machineRecord = new Models.Machine
                {
                    ModelName = machine.ModelName,
                    SerialNumber = machine.SerialNumber,
                    Version = machine.Version
                };

                await this.realm.WriteAsync((r) => r.Add(machineRecord));
            }
            else
            {
                machineInDatabase.ModelName = machine.ModelName;
                machineInDatabase.SerialNumber = machine.SerialNumber;
                machineInDatabase.Version = machine.Version;

                await this.realm.WriteAsync((r) => r.Add(machineInDatabase, true));
            }
        }

        public Task SaveRawDatabaseContent(byte[] rawDatabaseContent)
        {
            var machineInDatabase = this.realm.All<Models.Machine>().SingleOrDefault();

            if (machineInDatabase != null)
            {
                // It will perform an update to entity inside the database
                var machineRecord = new Models.Machine
                {
                    Id = machineInDatabase.Id, // Set the Id mandatory (as Primary Key)
                    ModelName = machineInDatabase.ModelName,
                    SerialNumber = machineInDatabase.SerialNumber,
                    Version = machineInDatabase.Version,
                    RawDatabaseContent = rawDatabaseContent // Update the raw database content
                };

                this.realm.Write(() =>
                {
                    this.realm.Add(machineRecord, update: true);
                });
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
