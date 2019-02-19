using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public MachineProvider(
            WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService)
        {
            this.machinesDataService = machinesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Machine>> GetAllAsync(
            int skip = 0,
            int take = 0,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            IExpression searchExpression = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            return (await this.machinesDataService.GetAllAsync(skip, take, whereExpression?.ToString(), orderByString, searchExpression?.ToString()))
                .Select(m => new Machine
                {
                    AisleName = m.AisleName,
                    ActualWeight = m.ActualWeight,
                    AreaName = m.AreaName,
                    AutomaticTime = m.AutomaticTime,
                    BuildDate = m.BuildDate,
                    CradlesCount = m.CradlesCount,
                    CustomerAddress = m.CustomerAddress,
                    CustomerCity = m.CustomerCity,
                    CustomerCode = m.CustomerCode,
                    CustomerCountry = m.CustomerCountry,
                    CustomerName = m.CustomerName,
                    Id = m.Id,
                    ErrorTime = m.ErrorTime,
                    Image = m.Image,
                    InstallationDate = m.InstallationDate,
                    LastPowerOn = m.LastPowerOn,
                    LastServiceDate = m.LastServiceDate,
                    MachineTypeDescription = m.MachineTypeDescription,
                    Model = m.Model,
                    Longitude = m.Longitude,
                    Latitude = m.Latitude,
                    ManualTime = m.ManualTime,
                    InputLoadingUnitsCount = m.InputLoadingUnitsCount,
                    LoadingUnitsPerCradle = m.LoadingUnitsPerCradle,
                    MovedLoadingUnitsCount = m.MovedLoadingUnitsCount,
                    MissionTime = m.MissionTime,
                    NextServiceDate = m.NextServiceDate,
                    Nickname = m.Nickname,
                    OutputLoadingUnitsCount = m.OutputLoadingUnitsCount,
                    PowerOnTime = m.PowerOnTime,
                    RegistrationNumber = m.RegistrationNumber,
                    TestDate = m.TestDate,
                    TotalMaxWeight = m.TotalMaxWeight
                });
        }

        public async Task<int> GetAllCountAsync(IExpression whereExpression = null, IExpression searchExpression = null)
        {
            return await this.machinesDataService.GetAllCountAsync(whereExpression?.ToString(), searchExpression?.ToString());
        }

        public async Task<Machine> GetByIdAsync(int id)
        {
            var machine = await this.machinesDataService.GetByIdAsync(id);

            return new Machine
            {
                AisleName = machine.AisleName,
                ActualWeight = machine.ActualWeight,
                AreaName = machine.AreaName,
                AutomaticTime = machine.AutomaticTime,
                BuildDate = machine.BuildDate,
                CradlesCount = machine.CradlesCount,
                CustomerAddress = machine.CustomerAddress,
                CustomerCity = machine.CustomerCity,
                CustomerCode = machine.CustomerCode,
                CustomerCountry = machine.CustomerCountry,
                CustomerName = machine.CustomerName,
                Id = machine.Id,
                ErrorTime = machine.ErrorTime,
                Image = machine.Image,
                InstallationDate = machine.InstallationDate,
                LastPowerOn = machine.LastPowerOn,
                LastServiceDate = machine.LastServiceDate,
                MachineTypeDescription = machine.MachineTypeDescription,
                Model = machine.Model,
                Longitude = machine.Longitude,
                Latitude = machine.Latitude,
                ManualTime = machine.ManualTime,
                InputLoadingUnitsCount = machine.InputLoadingUnitsCount,
                LoadingUnitsPerCradle = machine.LoadingUnitsPerCradle,
                MovedLoadingUnitsCount = machine.MovedLoadingUnitsCount,
                MissionTime = machine.MissionTime,
                NextServiceDate = machine.NextServiceDate,
                Nickname = machine.Nickname,
                OutputLoadingUnitsCount = machine.OutputLoadingUnitsCount,
                PowerOnTime = machine.PowerOnTime,
                RegistrationNumber = machine.RegistrationNumber,
                TestDate = machine.TestDate,
                TotalMaxWeight = machine.TotalMaxWeight
            };
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.machinesDataService.GetUniqueValuesAsync(propertyName);
        }

        #endregion
    }
}
