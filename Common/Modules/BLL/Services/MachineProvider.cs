using System;
using System.Linq;
using System.Linq.Expressions;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public MachineProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;

            //TODO: use interface for EnumerationProvider
            this.enumerationProvider = new EnumerationProvider(dataContext);
        }

        #endregion Constructors

        #region Methods

        public IQueryable<Machine> GetAll()
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return GetAllMachinesWithFilter(context);
        }

        public int GetAllCount()
        {
            using (var context = ServiceLocator.Current.GetInstance<DatabaseContext>())
            {
                return context.Machines.AsNoTracking().Count();
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

            var existingModel = this.dataContext.Machines.Find(model.Id);

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            return this.dataContext.SaveChanges();
        }

        private static IQueryable<Machine> GetAllMachinesWithFilter(DatabaseContext context, Expression<Func<DataModels.Machine, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Machines
               .AsNoTracking()
               .Include(m => m.Aisle)
               .Include(m => m.MachineType)
               .Where(actualWhereFunc)
               .Select(m => new Machine
               {
                   Id = m.Id,
                   ActualWeight = m.ActualWeight,
                   AisleName = m.Aisle.Name,
                   AutomaticTime = m.AutomaticTime,
                   BuildDate = m.BuildDate,
                   CradlesCount = m.Cradles, // TODO: update DataModel to use CradlesCount
                   CustomerAddress = m.CustomerAddress,
                   CustomerCity = m.CustomerCity,
                   CustomerCountry = m.CustomerCountry,
                   CustomerCode = m.CustomerCode,
                   CustomerName = m.CustomerName,
                   ErrorTime = m.ErrorTime,
                   Image = m.Image,
                   InputLoadingUnitsCount = m.InputLoadingUnitsCount,
                   InstallationDate = m.InstallationDate,
                   LastPowerOn = m.LastPoweOn,  // TODO: update LastPoweOn
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
