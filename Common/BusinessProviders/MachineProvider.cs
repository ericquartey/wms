using System;
using System.Linq;
using System.Linq.Expressions;
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

        private readonly DatabaseContext dataContet;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public MachineProvider(
            DatabaseContext dataContext,
            EnumerationProvider enumerationProvider)
        {
            this.enumerationProvider = enumerationProvider;
            this.dataContet = dataContext;
        }

        #endregion Constructors

        #region Methods

        public Int32 Add(MachineDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Machine> GetAll()
        {
            var tempContext = new DatabaseContext();

            return GetAllMachinesWithFilter(tempContext);
        }

        public int GetAllCount()
        {
            lock (this.dataContet)
            {
                return this.dataContet.Machines.AsNoTracking().Count();
            }
        }

        public IQueryable<Machine> GetAllTraslo()
        {
            var tempContext = new DatabaseContext();

            return GetAllMachinesWithFilter(this.dataContet, TrasloFilter);
        }

        public int GetAllTrasloCount()
        {
            lock (this.dataContet)
            {
                return this.dataContet.Machines.AsNoTracking().Where(TrasloFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimag()
        {
            var tempContext = new DatabaseContext();

            return GetAllMachinesWithFilter(this.dataContet, VertimagFilter);
        }

        public int GetAllVertimagCount()
        {
            lock (this.dataContet)
            {
                return this.dataContet.Machines.AsNoTracking().Where(VertimagFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimagModelM()
        {
            var tempContext = new DatabaseContext();

            return GetAllMachinesWithFilter(this.dataContet, VertimagModelMFilter);
        }

        public int GetAllVertimagModelMCount()
        {
            lock (this.dataContet)
            {
                return this.dataContet.Machines.AsNoTracking().Where(VertimagModelMFilter).Count();
            }
        }

        public IQueryable<Machine> GetAllVertimagModelXS()
        {
            var tempContext = new DatabaseContext();

            return GetAllMachinesWithFilter(this.dataContet, VertimagModelXSFilter);
        }

        public int GetAllVertimagModelXSCount()
        {
            lock (this.dataContet)
            {
                return this.dataContet.Machines.AsNoTracking().Where(VertimagModelXSFilter).Count();
            }
        }

        public MachineDetails GetById(int id)
        {
            throw new NotImplementedException();
        }

        public int Save(MachineDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContet)
            {
                var existingModel = this.dataContet.Machines.Find(model.Id);

                this.dataContet.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContet.SaveChanges();
            }
        }

        private static IQueryable<Machine> GetAllMachinesWithFilter(DatabaseContext context, Expression<Func<DataModels.Machine, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            lock (context)
            {
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
        }

        #endregion Methods
    }
}
