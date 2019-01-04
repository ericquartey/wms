using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class MachineProvider : IMachineProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.Machine, bool>> TrasloFilter =
           m => m.MachineType.Description.ToUpperInvariant().Contains("TRASLO");

        private static readonly Expression<Func<DataModels.Machine, bool>> VertimagFilter =
            m => m.MachineType.Description.ToUpperInvariant().Contains("VERTIMAG");

        private static readonly Expression<Func<DataModels.Machine, bool>> VertimagModelMFilter =
            m => m.Model.Contains("VARIANT-M");

        private static readonly Expression<Func<DataModels.Machine, bool>> VertimagModelXSFilter =
            m => m.Model.Contains("VARIANT-XS");

        private readonly IDatabaseContextService dataContext;

        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public MachineProvider(
            IDatabaseContextService dataContext,
            EnumerationProvider enumerationProvider)
        {
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> AddAsync(MachineDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Machine> GetAll()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current);
        }

        public int GetAllCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Machines.AsNoTracking().Count();
            }
        }

        public IQueryable<Machine> GetAllTraslo()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current, TrasloFilter);
        }

        public int GetAllTrasloCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Machines.AsNoTracking().Where(TrasloFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimag()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current, VertimagFilter);
        }

        public int GetAllVertimagCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Machines.AsNoTracking().Where(VertimagFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimagModelM()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current, VertimagModelMFilter);
        }

        public int GetAllVertimagModelMCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Machines.AsNoTracking().Where(VertimagModelMFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimagModelXS()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current, VertimagModelXSFilter);
        }

        public int GetAllVertimagModelXSCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Machines.AsNoTracking().Where(VertimagModelXSFilter).Count();
            }
        }

        public Task<MachineDetails> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<int> SaveAsync(MachineDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dataContext = this.dataContext.Current;

            var existingModel = dataContext.Machines.Find(model.Id);

            dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            return await dataContext.SaveChangesAsync();
        }

        private static IQueryable<Machine> GetAllMachinesWithFilter(DatabaseContext context, Expression<Func<DataModels.Machine, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Machines
                .AsNoTracking()
                .Include(m => m.Aisle)
                     .ThenInclude(a => a.Area)
                .Include(m => m.MachineType)
                .Where(actualWhereFunc)
                .Select(m => new Machine
                {
                    Id = m.Id,
                    ActualWeight = m.ActualWeight,
                    AisleName = m.Aisle.Name,
                    AreaName = m.Aisle.Area.Name,
                    AutomaticTime = m.AutomaticTime,
                    BuildDate = m.BuildDate,
                    CradlesCount = m.CradlesCount,
                    CustomerAddress = m.CustomerAddress,
                    CustomerCity = m.CustomerCity,
                    CustomerCountry = m.CustomerCountry,
                    CustomerCode = m.CustomerCode,
                    CustomerName = m.CustomerName,
                    ErrorTime = m.ErrorTime,
                    Image = m.Image,
                    InputLoadingUnitsCount = m.InputLoadingUnitsCount,
                    InstallationDate = m.InstallationDate,
                    LastPowerOn = m.LastPowerOn,
                    LastServiceDate = m.LastServiceDate,
                    Latitude = m.Latitude,
                    Longitude = m.Longitude,
                    LoadingUnitsPerCradle = m.LoadingUnitsPerCradle,
                    MachineTypeDescription = m.MachineType.Description,
                    ManualTime = m.ManualTime,
                    MissionTime = m.MissionTime,
                    MovedLoadingUnitsCount = m.MovedLoadingUnitsCount,
                    NextServiceDate = m.NextServiceDate,
                    Nickname = m.Nickname,
                    OutputLoadingUnitsCount = m.OutputLoadingUnitsCount,
                    PowerOnTime = m.PowerOnTime,
                    RegistrationNumber = m.RegistrationNumber,
                    TestDate = m.TestDate,
                    TotalMaxWeight = m.TotalMaxWeight
                }
                );
        }

        #endregion Methods
    }
}
