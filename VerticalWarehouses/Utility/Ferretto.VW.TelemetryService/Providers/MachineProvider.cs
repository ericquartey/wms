﻿using System.Linq;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
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
                    SerialNumber = loadedMachine.SerialNumber
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
            var machineRecord = new Models.Machine
            {
                ModelName = machine.ModelName,
                SerialNumber = machine.SerialNumber
            };

            await this.realm.WriteAsync((r) => r.Add(machineRecord));
        }

        #endregion
    }
}
