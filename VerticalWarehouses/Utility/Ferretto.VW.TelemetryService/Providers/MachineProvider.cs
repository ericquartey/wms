using System;
using System.Linq;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Data;
using Microsoft.Extensions.Caching.Memory;

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

        private readonly IDataContext dataContext;

        #endregion

        #region Constructors

        public MachineProvider(IDataContext dataContext, IMemoryCache cache)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.cache = cache;
        }

        #endregion

        #region Methods

        public IMachine? Get()
        {
            if (!this.cache.TryGetValue<Data.Machine>(MachineCacheKey, out var machine))
            {
                lock (this.dataContext)
                {
                    var loadedMachine = this.dataContext.Machines.SingleOrDefault();

                    if (loadedMachine is null)
                    {
                        return null;
                    }

                    var newMachine = new Data.Machine
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
            }

            return machine;
        }

        public IMachine GetRaw()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.SingleOrDefault();
            }
        }

        public void SaveAsync(IMachine machine)
        {
            // TODO: add check to avoid inserting duplicate records

            lock (this.dataContext)
            {
                var machineInDatabase = this.dataContext.Machines.SingleOrDefault();

                if (machineInDatabase is null)
                {
                    var machineRecord = new Data.Machine
                    {
                        ModelName = machine.ModelName,
                        SerialNumber = machine.SerialNumber,
                        Version = machine.Version
                    };

                    this.dataContext.Machines.Add(machineRecord);
                    this.dataContext.SaveChanges();
                }
                else
                {
                    machineInDatabase.ModelName = machine.ModelName;
                    machineInDatabase.SerialNumber = machine.SerialNumber;
                    machineInDatabase.Version = machine.Version;

                    this.dataContext.Machines.Update(machineInDatabase);
                    this.dataContext.SaveChanges();

                    this.cache.Remove(MachineCacheKey);
                }
            }
        }

        public void SaveRawDatabaseContent(byte[]? rawDatabaseContent)
        {
            lock (this.dataContext)
            {
                var machineInDatabase = this.dataContext.Machines.SingleOrDefault();

                if (machineInDatabase != null)
                {
                    machineInDatabase.RawDatabaseContent = rawDatabaseContent;

                    this.dataContext.Machines.Update(machineInDatabase);
                    this.dataContext.SaveChanges();
                }
            }
        }

        #endregion
    }
}
