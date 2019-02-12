using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public MachineProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Machine>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            Expression<Func<Machine, bool>> whereExpression = null,
            Expression<Func<Machine, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ApplyTransform(
                           skip,
                           take,
                           orderBy,
                           whereExpression,
                           searchExpression)
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync(
            Expression<Func<Machine, bool>> whereExpression = null,
            Expression<Func<Machine, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ApplyTransform(whereExpression, searchExpression)
                       .CountAsync();
        }

        public async Task<Machine> GetByIdAsync(int id)
        {
            return await this.GetAllBase()
                       .SingleOrDefaultAsync(i => i.Id == id);
        }

        private IQueryable<Machine> GetAllBase()
        {
            return this.dataContext.Machines
                .Include(m => m.Aisle)
                .ThenInclude(a => a.Area)
                .Include(m => m.MachineType)
                .Select(m => new Machine
                {
                    Id = m.Id,
                    ActualWeight = m.ActualWeight,
                    AisleId = m.AisleId,
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
                    FillRate = new Random().Next(100),
                    Image = m.Image,
                    InputLoadingUnitsCount = m.InputLoadingUnitsCount,
                    InstallationDate = m.InstallationDate,
                    LastPowerOn = m.LastPowerOn,
                    LastServiceDate = m.LastServiceDate,
                    Latitude = m.Latitude,
                    Longitude = m.Longitude,
                    LoadingUnitsPerCradle = m.LoadingUnitsPerCradle,
                    MachineTypeId = m.MachineTypeId,
                    MachineTypeDescription = m.MachineType.Description,
                    MaintenanceStatus = (MaintenanceStatus)((m.Id - 1) % 3),
                    ManualTime = m.ManualTime,
                    MissionTime = m.MissionTime,
                    Model = m.Model,
                    MovedLoadingUnitsCount = m.MovedLoadingUnitsCount,
                    NextServiceDate = m.NextServiceDate,
                    Nickname = m.Nickname,
                    OutputLoadingUnitsCount = m.OutputLoadingUnitsCount,
                    PowerOnTime = m.PowerOnTime,
                    RegistrationNumber = m.RegistrationNumber,
                    Status = (MachineStatus)(m.Id - 1),
                    TestDate = m.TestDate,
                    TotalMaxWeight = m.TotalMaxWeight
                });
        }

        #endregion
    }
}
