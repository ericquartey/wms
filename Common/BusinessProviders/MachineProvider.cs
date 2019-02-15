using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
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

        #endregion

        #region Constructors

        public MachineProvider(
            IDatabaseContextService dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public Task<IOperationResult> AddAsync(MachineDetails model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<Machine> GetAll()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current);
        }

        public int GetAllCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Machines.AsNoTracking().Count();
            }
        }

        public IQueryable<Machine> GetAllTraslo()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current, TrasloFilter);
        }

        public int GetAllTrasloCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Machines.AsNoTracking().Where(TrasloFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimag()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current, VertimagFilter);
        }

        public int GetAllVertimagCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Machines.AsNoTracking().Where(VertimagFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimagModelM()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current, VertimagModelMFilter);
        }

        public int GetAllVertimagModelMCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Machines.AsNoTracking().Where(VertimagModelMFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimagModelXs()
        {
            return GetAllMachinesWithFilter(this.dataContext.Current, VertimagModelXSFilter);
        }

        public int GetAllVertimagModelXsCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Machines.AsNoTracking().Where(VertimagModelXSFilter).Count();
            }
        }

        public Task<MachineDetails> GetByIdAsync(int id) => throw new NotSupportedException();

        public MachineDetails GetNew()
        {
            throw new NotSupportedException();
        }

        public async Task<IOperationResult> SaveAsync(MachineDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dc = this.dataContext.Current)
                {
                    var existingModel = dc.Machines.Find(model.Id);

                    dc.Entry(existingModel).CurrentValues.SetValues(model);

                    var changedEntityCount = await dc.SaveChangesAsync();

                    return new OperationResult(changedEntityCount > 0);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
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
                });
        }

        #endregion
    }
}
